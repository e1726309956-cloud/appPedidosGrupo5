using Microsoft.Maui.Networking;
using System.Net.Mail;
using System.Text.Json;

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
        HttpRequestException => Constantes.ErrorConexion,
        TaskCanceledException => Constantes.ErrorTiempoEspera,
        JsonException => Constantes.ErrorDatosFirebase,
        FormatException => Constantes.ErrorFormatoFirebase,
        InvalidOperationException => error.Message,
        _ => Constantes.ErrorGeneral
    };
}
