namespace TestadorCLPHI.App.Hardware;

public sealed class CommunicationProfile
{
    public string Id { get; init; } = string.Empty;

    public string DisplayName { get; init; } = string.Empty;

    public string Transport { get; init; } = string.Empty;

    public string Protocol { get; init; } = string.Empty;

    public string Generation { get; init; } = string.Empty;

    public string SourceStatus { get; init; } = string.Empty;

    public string ValidationStatus { get; init; } = string.Empty;

    public string Notes { get; init; } = string.Empty;

    public List<string> ApplicableFamilies { get; init; } = [];

    public Dictionary<string, string> Settings { get; init; } = [];
}
