using System.Net.Http.Json;
using System.Text.Json;

namespace appPedidosGrupo5.Services;

public class FirebaseAuthService
{
    // Coloca aquí el valor current_key de google-services.json.
    private const string ApiKey = "AIzaSyDS4T9EgMostkbKjyW8HIwfaJtkI9ON8OA";

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

    public async Task<string> IniciarSesionAsync(
        string correo, string contrasena)
    {
        var respuesta = await EnviarAsync(
            "accounts:signInWithPassword", new
            {
                email = correo.Trim(),
                password = contrasena,
                returnSecureToken = true
            });

        // Devuelve el token de acceso de la sesión.
        return respuesta.GetProperty("idToken").GetString()!;
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
        if (string.IsNullOrWhiteSpace(ApiKey))
        {
            throw new InvalidOperationException(
                "Configura la clave API de Firebase.");
        }

        var url =
            $"https://identitytoolkit.googleapis.com/v1/{operacion}" +
            $"?key={ApiKey}";

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
