using System.Text.Json;

namespace appPedidosGrupo5.Services;

internal static class FirebaseConfiguration
{
    public static async Task<string> LeerApiKeyAsync()
    {
        using var archivo = typeof(FirebaseConfiguration).Assembly
            .GetManifestResourceStream("appPedidosGrupo5.firebase.local.json");

        if (archivo is null)
            throw new InvalidOperationException(
                Constantes.ErrorConfiguracionAusente);

        try
        {
            using var documento = await JsonDocument.ParseAsync(archivo);
            if (documento.RootElement.ValueKind == JsonValueKind.Object &&
                documento.RootElement.TryGetProperty("ApiKey", out var campo) &&
                campo.ValueKind == JsonValueKind.String &&
                !string.IsNullOrWhiteSpace(campo.GetString()))
            {
                return campo.GetString()!.Trim();
            }
        }
        catch (JsonException)
        {
            throw new InvalidOperationException(
                Constantes.ErrorConfiguracionJson);
        }

        throw new InvalidOperationException(
            Constantes.ErrorApiKey);
    }
}
