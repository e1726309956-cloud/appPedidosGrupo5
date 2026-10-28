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

        // Devuelve el identificador del usuario creado.
        return respuesta.GetProperty("localId").GetString()!;
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

        var uid = respuesta.GetProperty("localId").GetString()!;
        var token = respuesta.GetProperty("idToken").GetString()!;

        var partes = token.Split('.');
        if (partes.Length != 3)
            throw new InvalidOperationException("Firebase devolvió un token de sesión inválido.");
        var payload = partes[1].Replace('-', '+').Replace('_', '/');
        payload = payload.PadRight((payload.Length + 3) / 4 * 4, '=');
        using var documento = JsonDocument.Parse(Convert.FromBase64String(payload));
        var proyectoId = documento.RootElement.GetProperty("aud").GetString();
        if (string.IsNullOrWhiteSpace(proyectoId))
            throw new InvalidOperationException("No se pudo identificar el proyecto de Firebase.");
        return new SesionFirebase(uid, token, proyectoId);
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
            throw new InvalidOperationException(
                "No se encontró el perfil en usuarios/" + sesion.Uid +
                ". Comprueba que el ID del documento coincida con el UID de Authentication.");
        if (respuesta.StatusCode == HttpStatusCode.Forbidden)
            throw new InvalidOperationException(
                "Firestore no permitió consultar el rol. Revisa las reglas de lectura del perfil del usuario.");
        if (respuesta.StatusCode == HttpStatusCode.Unauthorized)
            throw new InvalidOperationException(
                "Firestore no aceptó la sesión. Vuelve a iniciar sesión.");
        if (!respuesta.IsSuccessStatusCode)
            throw new InvalidOperationException(
                "No se pudo consultar el rol en Firestore. Inténtalo nuevamente.");

        using var documento = JsonDocument.Parse(await respuesta.Content.ReadAsStringAsync());
        if (!documento.RootElement.TryGetProperty("fields", out var campos) ||
            !campos.TryGetProperty("rol", out var campoRol) ||
            !campoRol.TryGetProperty("stringValue", out var valorRol) ||
            valorRol.ValueKind != JsonValueKind.String ||
            string.IsNullOrWhiteSpace(valorRol.GetString()))
        {
            throw new InvalidOperationException(
                "El perfil existe, pero el campo rol debe contener un texto válido.");
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
            return "La contraseña no cumple los requisitos de Firebase.";

        return codigo switch
        {
            "EMAIL_EXISTS" =>
                "Ya existe una cuenta con ese correo.",

            "INVALID_EMAIL" =>
                "El correo electrónico no es válido.",

            "INVALID_LOGIN_CREDENTIALS" or
            "EMAIL_NOT_FOUND" or
            "INVALID_PASSWORD" =>
                "El correo o la contraseña son incorrectos.",

            "USER_DISABLED" =>
                "Esta cuenta está deshabilitada.",

            "TOO_MANY_ATTEMPTS_TRY_LATER" =>
                "Demasiados intentos. Inténtalo más tarde.",

            "OPERATION_NOT_ALLOWED" =>
                "Activa correo y contraseña en Firebase Authentication.",

            _ => "No se pudo completar la operación con Firebase."
        };
    }
}
