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
                "Falta la configuración de Firebase. Copia firebase.example.json a " +
                "firebase.local.json, configura ApiKey y vuelve a compilar la aplicación.");

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
                "firebase.local.json no contiene un JSON válido. Corrige el archivo y vuelve a compilar.");
        }

        throw new InvalidOperationException(
            "Configura ApiKey en firebase.local.json y vuelve a compilar la aplicación.");
    }
}
