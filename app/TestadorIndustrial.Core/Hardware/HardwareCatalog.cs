namespace TestadorCLPHI.App.Hardware;

public sealed class HardwareCatalog
{
    public static HardwareCatalog Empty { get; } = new();

    public string CatalogId { get; init; } = string.Empty;

    public string SchemaVersion { get; init; } = string.Empty;

    public string UpdatedAt { get; init; } = string.Empty;

    public string Notes { get; init; } = string.Empty;

    public List<HardwareFamily> Families { get; init; } = [];

    public List<HardwareModel> Models { get; init; } = [];

    public List<IoModuleDefinition> IoModules { get; init; } = [];

    public List<CommunicationProfile> CommunicationProfiles { get; init; } = [];

    public List<TestProfile> TestProfiles { get; init; } = [];

    public HardwareFamily? FindFamily(string id)
    {
        return Families.FirstOrDefault(item => HasId(item.Id, id));
    }

    public HardwareModel? FindModel(string id)
    {
        return Models.FirstOrDefault(item => HasId(item.Id, id));
    }

    public IoModuleDefinition? FindIoModule(string id)
    {
        return IoModules.FirstOrDefault(item => HasId(item.Id, id));
    }

    public CommunicationProfile? FindCommunicationProfile(string id)
    {
        return CommunicationProfiles.FirstOrDefault(item => HasId(item.Id, id));
    }

    public TestProfile? FindTestProfile(string id)
    {
        return TestProfiles.FirstOrDefault(item => HasId(item.Id, id));
    }

    public IReadOnlyList<HardwareModel> GetModelsByFamily(string familyId)
    {
        return Models
            .Where(item => HasId(item.Family, familyId))
            .ToArray();
    }

    public IReadOnlyList<IoModuleDefinition> GetIoModulesByFamily(string familyId)
    {
        return IoModules
            .Where(item =>
                HasId(item.Family, familyId) ||
                item.SupportedFamilies.Any(supportedFamily => HasId(supportedFamily, familyId)))
            .ToArray();
    }

    public IReadOnlyList<CommunicationProfile> GetCommunicationProfilesByFamily(string familyId)
    {
        return CommunicationProfiles
            .Where(item => item.ApplicableFamilies.Any(applicableFamily => HasId(applicableFamily, familyId)))
            .ToArray();
    }

    public IReadOnlyList<TestProfile> GetTestProfilesByModule(string moduleId)
    {
        return TestProfiles
            .Where(item => item.ApplicableIoModules.Any(applicableModule => HasId(applicableModule, moduleId)))
            .ToArray();
    }

    private static bool HasId(string currentId, string requestedId)
    {
        return string.Equals(
            currentId,
            requestedId,
            StringComparison.OrdinalIgnoreCase);
    }
}
