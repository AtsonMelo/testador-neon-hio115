namespace TestadorCLPHI.App.Industrial.Platform.Simulation;

internal enum SimulationSignalKind
{
    DigitalInput,
    AnalogInput,
    VirtualOutput
}

internal enum SimulationVisualizationType
{
    Unspecified,
    Pivot,
    Well
}

internal enum SimulationIoDirection
{
    Unspecified,
    Input,
    Output
}

internal enum SimulationIoType
{
    Unspecified,
    Digital,
    Analog
}

internal enum SimulationIoEvidenceStatus
{
    Unspecified,
    SimulationProfile,
    PhysicalConfirmed
}

internal enum SimulationComparison
{
    Equals,
    NotEquals,
    LessThan,
    LessThanOrEqual,
    GreaterThan,
    GreaterThanOrEqual
}

internal sealed class SimulationProfile
{
    public int SchemaVersion { get; set; }
    public string? Id { get; set; }
    public string? DisplayName { get; set; }
    public string? InitialState { get; set; }
    public SimulationVisualizationDefinition Visualization { get; set; } = new();
    public List<SimulationSignalDefinition> Signals { get; set; } = [];
    public List<SimulationIoBinding> IoBindings { get; set; } = [];
    public List<SimulationRule> Rules { get; set; } = [];
    public List<SimulationExclusiveOutputGroup> ExclusiveOutputGroups { get; set; } = [];
    public List<SimulationScenario> Scenarios { get; set; } = [];
}

internal sealed class SimulationVisualizationDefinition
{
    public SimulationVisualizationType Type { get; set; }
    public int? TowerCount { get; set; }
    public int? FaultTowerIndex { get; set; }
    public Dictionary<string, string> SignalRoles { get; set; } =
        new(StringComparer.OrdinalIgnoreCase);
}

internal sealed class SimulationSignalDefinition
{
    public string? Id { get; set; }
    public string? Label { get; set; }
    public SimulationSignalKind Kind { get; set; }
    public double DefaultValue { get; set; }
    public double Minimum { get; set; }
    public double Maximum { get; set; }
    public string? Unit { get; set; }
    public string? Group { get; set; }
}

internal sealed class SimulationIoBinding
{
    public string? SignalId { get; set; }
    public string? SignalLabel { get; set; }
    public SimulationIoDirection Direction { get; set; }
    public SimulationIoType IoType { get; set; }
    public int Channel { get; set; } = -1;
    public int Register { get; set; }
    public string? RegisterAlias { get; set; }
    public string? Description { get; set; }
    public SimulationIoEvidenceStatus EvidenceStatus { get; set; }
}

internal sealed class SimulationRule
{
    public string? Id { get; set; }
    public int Priority { get; set; }
    public List<SimulationCondition> Conditions { get; set; } = [];
    public string? ResultState { get; set; }
    public string? AlarmId { get; set; }
    public bool BlockAllOutputs { get; set; }
}

internal sealed class SimulationCondition
{
    public string? SignalId { get; set; }
    public SimulationComparison Comparison { get; set; }
    public double ExpectedValue { get; set; }
}

internal sealed class SimulationExclusiveOutputGroup
{
    public string? Id { get; set; }
    public List<string> SignalIds { get; set; } = [];
}

internal sealed class SimulationScenario
{
    public string? Id { get; set; }
    public string? DisplayName { get; set; }
    public Dictionary<string, double> Values { get; set; } = new(StringComparer.OrdinalIgnoreCase);
}

internal sealed record SimulationSnapshot(
    string ProfileId,
    string State,
    IReadOnlyDictionary<string, double> Values,
    IReadOnlyList<string> ActiveAlarms,
    bool OutputsBlocked);

internal sealed record SimulationLogEntry(
    DateTimeOffset Timestamp,
    string Profile,
    string Operation,
    string Signal,
    double? Value,
    string Result,
    bool Simulated);

internal sealed class SimulationCounters
{
    private int _simulatedOperations;
    private int _simulatedCommands;

    internal int SimulatedOperations => Volatile.Read(ref _simulatedOperations);
    internal int SimulatedCommands => Volatile.Read(ref _simulatedCommands);
    internal int PhysicalConnections => 0;
    internal int PhysicalReads => 0;
    internal int PhysicalWrites => 0;
    internal int PhysicalCommands => 0;

    internal void RecordOperation() => Interlocked.Increment(ref _simulatedOperations);
    internal void RecordCommand() => Interlocked.Increment(ref _simulatedCommands);
}
