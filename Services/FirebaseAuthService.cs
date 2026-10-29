using System.Net.Http.Json;
using System.Text.Json;
using System.Net;
using System.Net.Http.Headers;

namespace appPedidosGrupo5.Services;

public class FirebaseAuthService
{
    private static readonly HttpClient Cliente = new()
    {
        Timeout = TimeSpan.FromSeconds(30)
    };

    public async Task<string> RegistrarAsync(
        string correo, string contrasena)
    {
        var respuesta = await EnviarAsync("accounts:signUp", new
        {
            email = correo.Trim(),
            password = contrasena,
            returnSecureToken = true
        });

        if (!respuesta.TryGetProperty("localId", out var localId) ||
            localId.ValueKind != JsonValueKind.String ||
            string.IsNullOrWhiteSpace(localId.GetString()))
            throw new InvalidOperationException(Constantes.ErrorIdentificadorCuenta);

        try
        {
            var sesion = LeerSesion(respuesta);
            await CrearPerfilClienteAsync(sesion);
        }
        catch (Exception error)
        {
            throw new InvalidOperationException(Constantes.ErrorRegistroParcial, error);
        }

        return localId.GetString()!;
    }

    public async Task<SesionFirebase> IniciarSesionAsync(
        string correo, string contrasena)
    {
        var respuesta = await EnviarAsync(
            "accounts:signInWithPassword", new
            {
                email = correo.Trim(),
                password = contrasena,
                returnSecureToken = true
            });

        return LeerSesion(respuesta);
    }

    private static SesionFirebase LeerSesion(JsonElement respuesta)
    {
        if (!respuesta.TryGetProperty("localId", out var localId) ||
            localId.ValueKind != JsonValueKind.String ||
            string.IsNullOrWhiteSpace(localId.GetString()))
            throw new InvalidOperationException(Constantes.ErrorIdentificadorSesion);

        if (!respuesta.TryGetProperty("idToken", out var tokenJson) ||
            tokenJson.ValueKind != JsonValueKind.String ||
            string.IsNullOrWhiteSpace(tokenJson.GetString()))
            throw new InvalidOperationException(Constantes.ErrorSesionInvalida);

        var uid = localId.GetString()!;
        var token = tokenJson.GetString()!;

        var partes = token.Split('.');
        if (partes.Length != 3)
            throw new InvalidOperationException(Constantes.ErrorTokenInvalido);
        var payload = partes[1].Replace('-', '+').Replace('_', '/');
        payload = payload.PadRight((payload.Length + 3) / 4 * 4, '=');
        using var documento = JsonDocument.Parse(Convert.FromBase64String(payload));
        if (!documento.RootElement.TryGetProperty("aud", out var aud) ||
            aud.ValueKind != JsonValueKind.String)
            throw new InvalidOperationException(Constantes.ErrorProyectoFirebase);

        var proyectoId = aud.GetString();
        if (string.IsNullOrWhiteSpace(proyectoId))
            throw new InvalidOperationException(Constantes.ErrorProyectoFirebase);
        return new SesionFirebase(uid, token, proyectoId);
    }

    private static async Task CrearPerfilClienteAsync(SesionFirebase sesion)
    {
        // createDocument solo crea: nunca reemplaza el rol de un perfil existente.
        var url = "https://firestore.googleapis.com/v1/projects/" +
            Uri.EscapeDataString(sesion.ProyectoId) +
            "/databases/(default)/documents/usuarios?documentId=" + Uri.EscapeDataString(sesion.Uid);
        using var solicitud = new HttpRequestMessage(HttpMethod.Post, url);
        solicitud.Headers.Authorization = new AuthenticationHeaderValue("Bearer", sesion.IdToken);
        solicitud.Content = JsonContent.Create(new
        {
            fields = new { rol = new { stringValue = Constantes.RolCliente } }
        });
        using var respuesta = await Cliente.SendAsync(solicitud);
        if (!respuesta.IsSuccessStatusCode)
            throw new InvalidOperationException(Constantes.ErrorCrearPerfilCliente);
    }

    public async Task<string> ObtenerRolAsync(SesionFirebase sesion)
    {
        var url = "https://firestore.googleapis.com/v1/projects/" +
            Uri.EscapeDataString(sesion.ProyectoId) +
            "/databases/(default)/documents/usuarios/" +
            Uri.EscapeDataString(sesion.Uid);

        using var solicitud = new HttpRequestMessage(HttpMethod.Get, url);
        solicitud.Headers.Authorization = new AuthenticationHeaderValue("Bearer", sesion.IdToken);
        using var respuesta = await Cliente.SendAsync(solicitud);
        if (respuesta.StatusCode == HttpStatusCode.NotFound)
        {
            // Recupera cuentas antiguas o registros cuyo segundo paso falló.
            await CrearPerfilClienteAsync(sesion);
            return Constantes.RolCliente;
        }
        if (respuesta.StatusCode == HttpStatusCode.Forbidden)
            throw new InvalidOperationException(
                Constantes.ErrorLecturaRol);
        if (respuesta.StatusCode == HttpStatusCode.Unauthorized)
            throw new InvalidOperationException(
                Constantes.ErrorSesionRechazada);
        if (!respuesta.IsSuccessStatusCode)
            throw new InvalidOperationException(
                Constantes.ErrorConsultaRol);

        using var documento = JsonDocument.Parse(await respuesta.Content.ReadAsStringAsync());
        if (!documento.RootElement.TryGetProperty("fields", out var campos) ||
            !campos.TryGetProperty("rol", out var campoRol) ||
            !campoRol.TryGetProperty("stringValue", out var valorRol) ||
            valorRol.ValueKind != JsonValueKind.String ||
            string.IsNullOrWhiteSpace(valorRol.GetString()))
        {
            throw new InvalidOperationException(
                Constantes.ErrorRolInvalido);
        }
        return valorRol.GetString()!;
    }

    public async Task RecuperarContrasenaAsync(string correo)
    {
        await EnviarAsync("accounts:sendOobCode", new
        {
            requestType = "PASSWORD_RESET",
            email = correo.Trim()
        });
    }

    private static async Task<JsonElement> EnviarAsync(
        string operacion, object datos)
    {
        var apiKey = await FirebaseConfiguration.LeerApiKeyAsync();

        var url =
            $"https://identitytoolkit.googleapis.com/v1/{operacion}" +
            $"?key={Uri.EscapeDataString(apiKey)}";

        using var respuesta = await Cliente.PostAsJsonAsync(url, datos);
        var contenido = await respuesta.Content.ReadAsStringAsync();

        using var documento = JsonDocument.Parse(contenido);
        var raiz = documento.RootElement;

        if (!respuesta.IsSuccessStatusCode)
        {
            var codigo = "";

            if (raiz.TryGetProperty("error", out var error) &&
                error.TryGetProperty("message", out var mensaje))
            {
                codigo = mensaje.GetString() ?? "";
            }

            throw new InvalidOperationException(
                TraducirError(codigo));
        }

        return raiz.Clone();
    }

    public sealed record SesionFirebase(string Uid, string IdToken, string ProyectoId);

    private static string TraducirError(string codigo)
    {
        if (codigo.StartsWith("WEAK_PASSWORD"))
            return Constantes.ErrorContrasenaFirebase;

        return codigo switch
        {
            "EMAIL_EXISTS" =>
                Constantes.ErrorCorreoRegistrado,

            "INVALID_EMAIL" =>
                Constantes.ErrorCorreoFirebase,

            "INVALID_LOGIN_CREDENTIALS" or
            "EMAIL_NOT_FOUND" or
            "INVALID_PASSWORD" =>
                Constantes.ErrorCredenciales,

            "USER_DISABLED" =>
                Constantes.ErrorCuentaDeshabilitada,

            "TOO_MANY_ATTEMPTS_TRY_LATER" =>
                Constantes.ErrorDemasiadosIntentos,

            "OPERATION_NOT_ALLOWED" =>
                Constantes.ErrorProveedorDeshabilitado,

            _ => Constantes.ErrorOperacionFirebase
        };
    }
}
