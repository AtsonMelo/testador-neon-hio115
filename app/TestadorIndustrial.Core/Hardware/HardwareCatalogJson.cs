using System.Text.Json;

namespace TestadorCLPHI.App.Hardware;

public static class HardwareCatalogJson
{
    private static readonly JsonSerializerOptions Options = new()
    {
        PropertyNameCaseInsensitive = true,
        ReadCommentHandling = JsonCommentHandling.Disallow,
        AllowTrailingCommas = false
    };

    public static HardwareCatalog Deserialize(string json, string? sourceDescription = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(json);

        HardwareCatalog? catalog = JsonSerializer.Deserialize<HardwareCatalog>(json, Options);
        return catalog ?? throw new InvalidDataException(
            $"Catalogo de hardware vazio ou invalido{FormatSource(sourceDescription)}.");
    }

    private static string FormatSource(string? sourceDescription) =>
        string.IsNullOrWhiteSpace(sourceDescription) ? string.Empty : $" ({sourceDescription})";
}
