using System.Text.Json;

namespace TestadorCLPHI.App.Hardware;

public static class HardwareCatalogLoader
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        ReadCommentHandling = JsonCommentHandling.Disallow,
        AllowTrailingCommas = false
    };

    public static string GetDefaultCatalogPath()
    {
        return Path.Combine(
            AppContext.BaseDirectory,
            "Data",
            "Hardware",
            "hi-hardware-catalog.json");
    }

    public static HardwareCatalog LoadDefault()
    {
        return LoadFromFile(GetDefaultCatalogPath());
    }

    public static HardwareCatalog LoadFromFile(string filePath)
    {
        if (!File.Exists(filePath))
        {
            throw new FileNotFoundException("Catalogo de hardware nao encontrado.", filePath);
        }

        string json = File.ReadAllText(filePath);
        HardwareCatalog? catalog = JsonSerializer.Deserialize<HardwareCatalog>(json, JsonOptions);

        if (catalog is null)
        {
            throw new InvalidDataException("Catalogo de hardware vazio ou invalido.");
        }

        return catalog;
    }
}
