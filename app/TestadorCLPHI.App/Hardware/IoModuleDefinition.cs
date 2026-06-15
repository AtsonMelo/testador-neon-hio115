namespace TestadorCLPHI.App.Hardware;

public sealed class IoModuleDefinition
{
    public string Id { get; init; } = string.Empty;

    public string DisplayName { get; init; } = string.Empty;

    public string Family { get; init; } = string.Empty;

    public string Generation { get; init; } = string.Empty;

    public string SourceStatus { get; init; } = string.Empty;

    public string ValidationStatus { get; init; } = string.Empty;

    public string ValidationNotes { get; init; } = string.Empty;

    public string Notes { get; init; } = string.Empty;

    public List<string> SupportedFamilies { get; init; } = [];

    public List<string> SupportedSlots { get; init; } = [];

    public List<string> DefaultCommunicationProfiles { get; init; } = [];

    public List<string> SupportedIoModules { get; init; } = [];

    public List<string> TestProfiles { get; init; } = [];
}
