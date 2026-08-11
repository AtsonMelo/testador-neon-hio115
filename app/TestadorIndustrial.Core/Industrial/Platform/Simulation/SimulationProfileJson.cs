using System.Text.Json;
using System.Text.Json.Serialization;

namespace TestadorCLPHI.App.Industrial.Platform.Simulation;

internal static class SimulationProfileJson
{
    private static readonly JsonSerializerOptions Options = new()
    {
        PropertyNameCaseInsensitive = true,
        UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow,
        Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) }
    };

    internal static SimulationProfile Deserialize(string json, string? sourceDescription = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(json);

        return JsonSerializer.Deserialize<SimulationProfile>(json, Options)
            ?? throw new InvalidDataException(
                $"Perfil de simulacao vazio ou invalido{FormatSource(sourceDescription)}.");
    }

    private static string FormatSource(string? sourceDescription) =>
        string.IsNullOrWhiteSpace(sourceDescription) ? string.Empty : $" ({sourceDescription})";
}
