using Microsoft.Maui.Networking;
using System.Net.Mail;

namespace appPedidosGrupo5.Views;

internal static class ValidacionSeguridad
{
    public static bool HayInternet() =>
        Connectivity.Current.NetworkAccess == NetworkAccess.Internet;

    public static bool CorreoValido(string? correo) =>
        !string.IsNullOrWhiteSpace(correo) &&
        MailAddress.TryCreate(correo.Trim(), out var direccion) &&
        direccion.Address == correo.Trim();

    public static string MensajeError(Exception error) => error switch
    {
        HttpRequestException => "No se pudo conectar. Revisa tu conexión a Internet.",
        TaskCanceledException => "La solicitud tardó demasiado. Inténtalo nuevamente.",
        InvalidOperationException => error.Message,
        _ => "No se pudo completar la operación. Inténtalo nuevamente."
    };
}
