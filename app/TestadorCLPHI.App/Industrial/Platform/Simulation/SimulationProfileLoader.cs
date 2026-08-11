namespace TestadorCLPHI.App.Industrial.Platform.Simulation;

internal static class SimulationProfileLoader
{
    internal static SimulationProfile Load(string profileId)
    {
        if (string.IsNullOrWhiteSpace(profileId))
        {
            throw new ArgumentException("Perfil ausente.", nameof(profileId));
        }

        string fileName = profileId + ".json";
        string path = ResolveProfilePath(fileName);
        return SimulationProfileJson.Deserialize(File.ReadAllText(path), path);
    }

    internal static IReadOnlyList<string> AvailableProfileIds() =>
        Directory.GetFiles(ResolveProfileDirectory(), "*.json")
            .Select(path => Path.GetFileNameWithoutExtension(path)!)
            .Order(StringComparer.OrdinalIgnoreCase)
            .ToArray();

    private static string ResolveProfilePath(string fileName)
    {
        string path = Path.Combine(ResolveProfileDirectory(), fileName);
        if (!File.Exists(path))
        {
            throw new FileNotFoundException($"Perfil de simulacao ausente: {fileName}.", path);
        }

        return path;
    }

    private static string ResolveProfileDirectory()
    {
        string outputPath = Path.Combine(AppContext.BaseDirectory, "Data", "Simulation", "Profiles");
        if (Directory.Exists(outputPath))
        {
            return outputPath;
        }

        DirectoryInfo? current = new(AppContext.BaseDirectory);
        while (current is not null)
        {
            string candidate = Path.Combine(
                current.FullName,
                "app",
                "TestadorCLPHI.App",
                "Data",
                "Simulation",
                "Profiles");
            if (Directory.Exists(candidate))
            {
                return candidate;
            }

            current = current.Parent;
        }

        throw new DirectoryNotFoundException("Diretorio Data/Simulation/Profiles nao encontrado.");
    }
}
