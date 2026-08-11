namespace TestadorCLPHI.App.Hardware;

public sealed class SelectedHardwareProfile
{
    public static SelectedHardwareProfile Empty { get; } = new(
        null,
        null,
        null,
        null,
        null,
        [],
        [],
        [],
        [],
        [],
        []);

    public SelectedHardwareProfile(
        HardwareFamily? family,
        HardwareModel? model,
        IoModuleDefinition? ioModule,
        CommunicationProfile? communicationProfile,
        TestProfile? testProfile,
        IEnumerable<IoModuleDefinition> compatibleModules,
        IEnumerable<CommunicationProfile> possibleCommunicationProfiles,
        IEnumerable<TestProfile> applicableTestProfiles,
        IEnumerable<string> pendingItems,
        IEnumerable<string> fieldObservedItems,
        IEnumerable<string> benchValidationNeeds)
    {
        Family = family;
        Model = model;
        IoModule = ioModule;
        CommunicationProfile = communicationProfile;
        TestProfile = testProfile;
        CompatibleModules = compatibleModules.ToArray();
        PossibleCommunicationProfiles = possibleCommunicationProfiles.ToArray();
        ApplicableTestProfiles = applicableTestProfiles.ToArray();
        PendingItems = pendingItems.ToArray();
        FieldObservedItems = fieldObservedItems.ToArray();
        BenchValidationNeeds = benchValidationNeeds.ToArray();
    }

    public SelectedHardwareProfile(HardwareProfileResolution resolution)
        : this(
            resolution.Family,
            resolution.Model,
            resolution.IoModule,
            resolution.CommunicationProfile,
            resolution.TestProfile,
            resolution.CompatibleModules,
            resolution.PossibleCommunicationProfiles,
            resolution.ApplicableTestProfiles,
            resolution.PendingItems,
            resolution.FieldObservedItems,
            resolution.BenchValidationNeeds)
    {
    }

    public HardwareFamily? Family { get; }

    public HardwareModel? Model { get; }

    public IoModuleDefinition? IoModule { get; }

    public CommunicationProfile? CommunicationProfile { get; }

    public TestProfile? TestProfile { get; }

    public IReadOnlyList<IoModuleDefinition> CompatibleModules { get; }

    public IReadOnlyList<CommunicationProfile> PossibleCommunicationProfiles { get; }

    public IReadOnlyList<TestProfile> ApplicableTestProfiles { get; }

    public IReadOnlyList<string> PendingItems { get; }

    public IReadOnlyList<string> FieldObservedItems { get; }

    public IReadOnlyList<string> BenchValidationNeeds { get; }

    public bool IsComplete =>
        Family is not null &&
        Model is not null &&
        IoModule is not null &&
        CommunicationProfile is not null &&
        TestProfile is not null;

    public bool RequiresManualValidation => PendingItems.Count > 0;
}
