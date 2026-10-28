using System.Net;
using System.Net.Http.Headers;
using System.Text.Json;

namespace appPedidosGrupo5.Services;

public sealed class PermisosService(FirebaseAuthService.SesionFirebase sesion)
{
    private static readonly HttpClient Cliente = new() { Timeout = TimeSpan.FromSeconds(30) };
    public string Rol { get; private set; } = "";
    private readonly HashSet<string> pantallas = new(StringComparer.Ordinal);

    public bool PuedeAcceder(string pantalla) => pantallas.Contains(pantalla);

    public async Task ActualizarAsync()
    {
        pantallas.Clear();
        Rol = "";
        var rol = (await new FirebaseAuthService().ObtenerRolAsync(sesion)).Trim().ToLowerInvariant();
        if (rol is not ("cliente" or "administrador" or "vendedor"))
            throw new InvalidOperationException(Constantes.ErrorRolDesconocido);

        var url = "https://firestore.googleapis.com/v1/projects/" +
            Uri.EscapeDataString(sesion.ProyectoId) + "/databases/(default)/documents/roles/" + rol;
        using var solicitud = new HttpRequestMessage(HttpMethod.Get, url);
        solicitud.Headers.Authorization = new AuthenticationHeaderValue("Bearer", sesion.IdToken);
        using var respuesta = await Cliente.SendAsync(solicitud);
        if (respuesta.StatusCode == HttpStatusCode.Unauthorized)
            throw new InvalidOperationException(Constantes.ErrorSesionVencida);
        if (respuesta.StatusCode == HttpStatusCode.NotFound)
            throw new InvalidOperationException(string.Format(Constantes.ErrorRolAusente, rol));
        if (respuesta.StatusCode == HttpStatusCode.Forbidden)
            throw new InvalidOperationException(string.Format(Constantes.ErrorPermisosDenegados, rol, sesion.Uid));
        if (!respuesta.IsSuccessStatusCode)
            throw new InvalidOperationException(string.Format(Constantes.ErrorConsultaPermisos, rol));

        using var documento = JsonDocument.Parse(await respuesta.Content.ReadAsStringAsync());
        if (!documento.RootElement.TryGetProperty("fields", out var campos) ||
            !campos.TryGetProperty("pantallas", out var mapa) ||
            !mapa.TryGetProperty("mapValue", out var valorMapa))
            throw new InvalidOperationException(Constantes.ErrorMapaPantallas);

        if (valorMapa.TryGetProperty("fields", out var permisos))
            foreach (var permiso in permisos.EnumerateObject())
                if (permiso.Value.TryGetProperty("booleanValue", out var habilitado) &&
                    habilitado.ValueKind == JsonValueKind.True)
                    pantallas.Add(permiso.Name);
        Rol = rol;
    }
}
