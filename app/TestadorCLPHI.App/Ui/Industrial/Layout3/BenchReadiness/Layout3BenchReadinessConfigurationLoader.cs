using System.Text.Json;
using System.Text.Json.Serialization;

namespace TestadorCLPHI.App.Ui.Industrial.Layout3;

internal sealed record Layout3BenchReadinessLoadResult(
    Layout3BenchReadinessConfiguration Configuration,
    string RepositoryRoot,
    string ConfigurationPath,
    ISet<string> AvailableDocuments);

internal static class Layout3BenchReadinessConfigurationLoader
{
    private static readonly string[] ConfigurationSegments =
    [
        "app",
        "TestadorCLPHI.App",
        "Data",
        "Bench",
        "layout-3-bench-readiness.json"
    ];

    public static Layout3BenchReadinessLoadResult LoadDefault()
    {
        string repositoryRoot = FindRepositoryRoot();
        string configurationPath = Path.Combine([repositoryRoot, .. ConfigurationSegments]);

        if (!File.Exists(configurationPath))
        {
            throw new FileNotFoundException(
                "Configuracao local de prontidao nao encontrada.",
                configurationPath);
        }

        JsonSerializerOptions options = new()
        {
            PropertyNameCaseInsensitive = true,
            UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow
        };

        string json = File.ReadAllText(configurationPath);
        Layout3BenchReadinessConfiguration? configuration =
            JsonSerializer.Deserialize<Layout3BenchReadinessConfiguration>(json, options);

        if (configuration is null)
        {
            throw new InvalidDataException("Configuracao local de prontidao esta vazia.");
        }

        HashSet<string> availableDocuments = new(StringComparer.OrdinalIgnoreCase);
        foreach (string document in Layout3BenchReadinessEvaluator.RequiredDocuments)
        {
            string fullPath = Path.Combine(repositoryRoot, document.Replace('/', Path.DirectorySeparatorChar));
            if (File.Exists(fullPath))
            {
                availableDocuments.Add(document);
            }
        }

        return new Layout3BenchReadinessLoadResult(
            configuration,
            repositoryRoot,
            configurationPath,
            availableDocuments);
    }

    private static string FindRepositoryRoot()
    {
        string[] startingPoints =
        [
            Directory.GetCurrentDirectory(),
            AppContext.BaseDirectory
        ];

        HashSet<string> visited = new(StringComparer.OrdinalIgnoreCase);
        foreach (string startingPoint in startingPoints)
        {
            DirectoryInfo? current = new(Path.GetFullPath(startingPoint));
            while (current is not null && visited.Add(current.FullName))
            {
                string projectPath = Path.Combine(
                    current.FullName,
                    "app",
                    "TestadorCLPHI.App",
                    "TestadorCLPHI.App.csproj");
                string docsPath = Path.Combine(current.FullName, "docs");

                if (File.Exists(projectPath) && Directory.Exists(docsPath))
                {
                    return current.FullName;
                }

                current = current.Parent;
            }
        }

        throw new DirectoryNotFoundException(
            "Raiz local do repositorio nao encontrada; preflight bloqueado.");
    }
}
