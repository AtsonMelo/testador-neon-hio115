namespace TestadorCLPHI.App.Hardware;

public static class HardwareCatalogLoader
{
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

        return HardwareCatalogJson.Deserialize(File.ReadAllText(filePath), filePath);
    }
}
