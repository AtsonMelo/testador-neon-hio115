namespace TestadorCLPHI.App.Hardware;

public sealed class TestProfile
{
    public string Id { get; init; } = string.Empty;

    public string DisplayName { get; init; } = string.Empty;

    public string Generation { get; init; } = string.Empty;

    public string SourceStatus { get; init; } = string.Empty;

    public string ValidationStatus { get; init; } = string.Empty;

    public string Notes { get; init; } = string.Empty;

    public List<HardwareOfficialReference> OfficialReferences { get; init; } = [];

    public List<string> ApplicableFamilies { get; init; } = [];

    public List<string> ApplicableIoModules { get; init; } = [];

    public List<string> Prerequisites { get; init; } = [];

    public List<string> TestActions { get; init; } = [];

    public List<string> AcceptanceCriteria { get; init; } = [];

    public List<string> RisksAndCare { get; init; } = [];
}
