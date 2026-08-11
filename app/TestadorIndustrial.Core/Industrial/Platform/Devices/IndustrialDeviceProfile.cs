namespace TestadorCLPHI.App.Industrial.Platform.Devices;

internal enum IndustrialDeviceSupportStatus
{
    SimulationOnly,
    Pending,
    Unsupported
}

internal enum DeviceInputKind
{
    Digital,
    Analog
}

internal enum DeviceIdentificationProbeRole
{
    ProgramId,
    ProgramCrc,
    GeneralFailureStatus,
    Additional
}

internal enum DeviceIdentificationEvaluationState
{
    Identified,
    SignatureMismatch,
    CriticalFault,
    Unsupported
}

internal sealed record DeviceIdentificationProbe(
    string Alias,
    ushort DocumentedReference,
    DeviceIdentificationProbeRole Role,
    ushort? ExpectedValue = null,
    ushort CriticalMask = 0);

internal sealed record DeviceIdentificationEvaluation(
    DeviceIdentificationEvaluationState State,
    ushort? ProgramId,
    ushort? ProgramCrc,
    ushort? GeneralFailureStatus,
    string Detail);

internal sealed record DeviceIdentificationPolicy(
    bool IsSupported,
    string? ExpectedFirmwareFamily,
    string? ExpectedFirmwareVersion,
    IReadOnlyList<DeviceIdentificationProbe> Probes,
    string IdentifiedDetail,
    string SignatureMismatchDetail,
    string CriticalFaultDetail)
{
    internal static DeviceIdentificationPolicy Unsupported { get; } = new(
        IsSupported: false,
        ExpectedFirmwareFamily: null,
        ExpectedFirmwareVersion: null,
        Probes: [],
        IdentifiedDetail: "Identificacao operacional nao suportada.",
        SignatureMismatchDetail: "Identificacao operacional nao suportada.",
        CriticalFaultDetail: "Identificacao operacional nao suportada.");

    internal DeviceIdentificationEvaluation Evaluate(
        IReadOnlyDictionary<string, ushort> observedValues)
    {
        ArgumentNullException.ThrowIfNull(observedValues);
        if (!IsSupported)
        {
            return new(
                DeviceIdentificationEvaluationState.Unsupported,
                null,
                null,
                null,
                "Identificacao operacional nao suportada pelo perfil.");
        }

        foreach (DeviceIdentificationProbe probe in Probes)
        {
            if (!observedValues.ContainsKey(probe.Alias))
            {
                throw new ArgumentException(
                    $"Valor ausente para probe de identificacao {probe.Alias}.",
                    nameof(observedValues));
            }
        }

        ushort? programId = ResolveRole(observedValues, DeviceIdentificationProbeRole.ProgramId);
        ushort? programCrc = ResolveRole(observedValues, DeviceIdentificationProbeRole.ProgramCrc);
        ushort? generalFailureStatus = ResolveRole(
            observedValues,
            DeviceIdentificationProbeRole.GeneralFailureStatus);

        bool criticalFault = Probes.Any(probe =>
            probe.CriticalMask != 0
            && (observedValues[probe.Alias] & probe.CriticalMask) != 0);
        if (criticalFault)
        {
            return new(
                DeviceIdentificationEvaluationState.CriticalFault,
                programId,
                programCrc,
                generalFailureStatus,
                CriticalFaultDetail);
        }

        bool signatureMismatch = Probes.Any(probe =>
            probe.ExpectedValue is ushort expected
            && observedValues[probe.Alias] != expected);
        if (signatureMismatch)
        {
            return new(
                DeviceIdentificationEvaluationState.SignatureMismatch,
                programId,
                programCrc,
                generalFailureStatus,
                SignatureMismatchDetail);
        }

        return new(
            DeviceIdentificationEvaluationState.Identified,
            programId,
            programCrc,
            generalFailureStatus,
            IdentifiedDetail);
    }

    internal ushort GetExpectedValue(DeviceIdentificationProbeRole role)
    {
        DeviceIdentificationProbe probe = Probes.Single(item => item.Role == role);
        return probe.ExpectedValue
            ?? throw new InvalidOperationException($"Probe {probe.Alias} nao possui valor esperado.");
    }

    internal ushort GetReference(DeviceIdentificationProbeRole role) =>
        Probes.Single(item => item.Role == role).DocumentedReference;

    internal ushort GetCriticalMask(DeviceIdentificationProbeRole role) =>
        Probes.Single(item => item.Role == role).CriticalMask;

    internal DeviceIdentificationPolicy WithExpectedValue(
        DeviceIdentificationProbeRole role,
        ushort expectedValue)
    {
        DeviceIdentificationProbe[] probes = Probes
            .Select(probe => probe.Role == role
                ? probe with { ExpectedValue = expectedValue }
                : probe)
            .ToArray();
        return this with { Probes = probes };
    }

    private ushort? ResolveRole(
        IReadOnlyDictionary<string, ushort> observedValues,
        DeviceIdentificationProbeRole role)
    {
        DeviceIdentificationProbe? probe = Probes.FirstOrDefault(item => item.Role == role);
        return probe is not null ? observedValues[probe.Alias] : null;
    }
}

internal sealed record DeviceRegisterPoint(
    int Channel,
    string Alias,
    ushort DocumentedReference);

internal sealed record DeviceInputBlock(
    DeviceInputKind Kind,
    string Alias,
    IReadOnlyList<DeviceRegisterPoint> Points)
{
    internal ushort StartReference => Points[0].DocumentedReference;
}

internal sealed record DeviceInputMap(IReadOnlyList<DeviceInputBlock> Blocks)
{
    internal IReadOnlyList<DeviceRegisterPoint> DigitalInputs =>
        Blocks.Where(block => block.Kind == DeviceInputKind.Digital)
            .SelectMany(block => block.Points)
            .OrderBy(point => point.Channel)
            .ToArray();

    internal IReadOnlyList<DeviceRegisterPoint> AnalogInputs =>
        Blocks.Where(block => block.Kind == DeviceInputKind.Analog)
            .SelectMany(block => block.Points)
            .OrderBy(point => point.Channel)
            .ToArray();

    internal bool TryResolve(
        DeviceInputKind kind,
        int channel,
        out DeviceRegisterPoint point)
    {
        point = Blocks
            .Where(block => block.Kind == kind)
            .SelectMany(block => block.Points)
            .FirstOrDefault(item => item.Channel == channel)!;
        return point is not null;
    }

    internal bool ContainsReference(int documentedReference) =>
        Blocks.SelectMany(block => block.Points)
            .Any(point => point.DocumentedReference == documentedReference);
}

internal sealed record DeviceBlockedRegister(
    string Alias,
    ushort DocumentedReference,
    string Reason);

internal sealed record DeviceOutputMap(
    IReadOnlyList<DeviceRegisterPoint> Outputs,
    IReadOnlyList<DeviceBlockedRegister> BlockedRegisters)
{
    internal bool TryResolve(string alias, out DeviceRegisterPoint point)
    {
        point = Outputs.FirstOrDefault(item => string.Equals(
            item.Alias,
            alias,
            StringComparison.OrdinalIgnoreCase))!;
        return point is not null;
    }

    internal bool ContainsReference(int documentedReference) =>
        Outputs.Any(point => point.DocumentedReference == documentedReference);
}

internal sealed record IndustrialDeviceLimits(
    int DigitalInputCount,
    int AnalogInputCount,
    int DigitalOutputCount,
    TimeSpan MaximumSimulatedOutputDuration);

internal sealed record IndustrialDeviceCapabilities(
    bool IdentificationSupported,
    bool DigitalInputReadSupported,
    bool AnalogInputReadSupported,
    bool SupervisedOutputTestSupported,
    bool SimulationSupported);

internal sealed record IndustrialDeviceProfile(
    string Id,
    string DisplayName,
    string Family,
    string Model,
    string ControllerCpu,
    string IoModule,
    DeviceIdentificationPolicy IdentificationPolicy,
    DeviceInputMap InputMap,
    DeviceOutputMap OutputMap,
    IndustrialDeviceLimits Limits,
    IndustrialDeviceCapabilities Capabilities,
    IndustrialDeviceSupportStatus SupportStatus,
    bool PhysicalSupport,
    string LogIdentity,
    string ProtocolAdapterId)
{
    internal bool CanCreateSimulatedSession =>
        SupportStatus == IndustrialDeviceSupportStatus.SimulationOnly
        && Capabilities.SimulationSupported
        && Capabilities.IdentificationSupported
        && IdentificationPolicy.IsSupported;

    internal bool CanCreatePhysicalSession => false;

    internal static IndustrialDeviceProfile CreateUnsupported(string id, string displayName) => new(
        id,
        displayName,
        Family: string.Empty,
        Model: string.Empty,
        ControllerCpu: string.Empty,
        IoModule: string.Empty,
        IdentificationPolicy: DeviceIdentificationPolicy.Unsupported,
        InputMap: new([]),
        OutputMap: new([], []),
        Limits: new(0, 0, 0, TimeSpan.Zero),
        Capabilities: new(false, false, false, false, false),
        SupportStatus: IndustrialDeviceSupportStatus.Unsupported,
        PhysicalSupport: false,
        LogIdentity: id,
        ProtocolAdapterId: string.Empty);
}
