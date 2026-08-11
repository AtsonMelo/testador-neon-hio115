using TestadorCLPHI.App.Industrial.Platform.Devices;
using TestadorCLPHI.App.Industrial.Platform.Rtu;
using TestadorCLPHI.App.Industrial.Platform.Simulation;
using TestadorCLPHI.App.Industrial.Platform.Safety;

namespace TestadorCLPHI.App.Industrial.Platform.Integration;

internal sealed class IndustrialPlatformSession : IDisposable
{
    internal const byte SimulatedDeviceAddress = 1;

    private readonly SimulationHio115Adapter _adapter;
    private readonly RtuEquipmentIdentificationService _identification;
    private readonly RtuDiscoveryService _discovery;
    private readonly RtuInputTestService _inputs;
    private readonly RtuSupervisedOutputService _outputs;

    internal IndustrialPlatformSession(
        SimulationProfile profile,
        IndustrialDeviceProfile? deviceProfile = null)
    {
        ArgumentNullException.ThrowIfNull(profile);

        DeviceProfile = deviceProfile ?? NeonHio115DeviceProfile.Current;
        if (!DeviceProfile.CanCreateSimulatedSession)
        {
            throw new InvalidOperationException(
                $"Equipamento {DeviceProfile.DisplayName} nao possui suporte operacional simulado.");
        }

        Profile = profile;
        Simulation = new SimulationEngine(Profile);
        Device = new NeonHio115FakeDevice(SimulatedDeviceAddress, profile: DeviceProfile);
        IoMapping = SimulationHio115Mappings.FromProfile(Profile, DeviceProfile);
        _adapter = new SimulationHio115Adapter(
            Simulation,
            Device,
            IoMapping,
            DeviceProfile);
        _adapter.SyncInputsToDevice();

        Counters = new IndustrialOperationCounters();
        OperationLog = new InMemoryOperationLog();
        InMemoryRtuTransport transport = new([Device]);
        RtuClient client = new(
            transport,
            Counters,
            OperationLog,
            TimeSpan.FromMilliseconds(250),
            DeviceProfile.LogIdentity);
        _identification = new RtuEquipmentIdentificationService(
            client,
            DeviceProfile.IdentificationPolicy);
        _discovery = new RtuDiscoveryService(_identification, Counters);
        _inputs = new RtuInputTestService(client, DeviceProfile.InputMap);
        _outputs = new RtuSupervisedOutputService(
            client,
            DeviceProfile.OutputMap,
            DeviceProfile.Limits.MaximumSimulatedOutputDuration);
    }

    internal IndustrialDeviceProfile DeviceProfile { get; }
    internal SimulationProfile Profile { get; }
    internal SimulationEngine Simulation { get; }
    internal NeonHio115FakeDevice Device { get; }
    internal SimulationHio115Mapping IoMapping { get; }
    internal IndustrialOperationCounters Counters { get; }
    internal InMemoryOperationLog OperationLog { get; }
    internal RtuIdentificationResult? LastIdentification { get; private set; }

    internal event EventHandler? Changed;

    internal SimulationIoBinding? FindBinding(
        SimulationIoDirection direction,
        SimulationIoType ioType,
        int channel) =>
        Profile.IoBindings.FirstOrDefault(binding =>
            binding.Direction == direction
            && binding.IoType == ioType
            && binding.Channel == channel);

    internal SimulationSignalDefinition? FindSignal(string? signalId) =>
        Profile.Signals.FirstOrDefault(signal => string.Equals(
            signal.Id,
            signalId,
            StringComparison.OrdinalIgnoreCase));

    internal void ResetSimulation()
    {
        Simulation.Reset();
        Synchronize();
    }

    internal void ApplyScenario(string scenarioId)
    {
        Simulation.ApplyScenario(scenarioId);
        Synchronize();
    }

    internal void SetDigitalInput(string signalId, bool value)
    {
        Simulation.SetDigitalInput(signalId, value);
        Synchronize();
    }

    internal void SetAnalogInput(string signalId, double value)
    {
        Simulation.SetAnalogInput(signalId, value);
        Synchronize();
    }

    internal async Task<RtuIdentificationResult> IdentifyAsync(
        byte address,
        CancellationToken cancellationToken)
    {
        LastIdentification = await _identification.ProbeAsync(address, cancellationToken);
        Changed?.Invoke(this, EventArgs.Empty);
        return LastIdentification;
    }

    internal async Task<RtuDiscoveryResult> DiscoverAsync(
        byte startAddress,
        byte endAddress,
        TimeSpan interval,
        IProgress<RtuDiscoveryProgress>? progress,
        CancellationToken cancellationToken)
    {
        RtuDiscoveryResult result = await _discovery.DiscoverAsync(
            startAddress,
            endAddress,
            interval,
            progress,
            cancellationToken);
        LastIdentification = result.Match;
        Changed?.Invoke(this, EventArgs.Empty);
        return result;
    }

    internal async Task<RtuInputSnapshot> ReadInputsAsync(
        byte address,
        CancellationToken cancellationToken)
    {
        _adapter.SyncInputsToDevice();
        RtuInputSnapshot snapshot = await _inputs.ReadAsync(address, cancellationToken);
        Changed?.Invoke(this, EventArgs.Empty);
        return snapshot;
    }

    internal async Task<RtuOutputTestResult> ActivateOutputAsync(
        byte address,
        Layout3OutputChannel channel,
        TimeSpan duration,
        bool operatorExplicitlyEnabled,
        CancellationToken cancellationToken)
    {
        bool identified = LastIdentification?.State == RtuIdentificationState.Identified;
        Layout3OutputAuthorization simulationAuthorization = new(
            EquipmentIdentified: identified,
            SignatureValid: identified,
            CriticalFailureAbsent: LastIdentification?.GeneralFailureStatus == 0,
            BenchChecklistApproved: true,
            OperatorExplicitlyEnabled: operatorExplicitlyEnabled,
            PhysicalGateAuthorized: false,
            SimulationOnly: true);

        RtuOutputTestResult result = await _outputs.ActivateMomentaryAsync(
            address,
            channel,
            duration,
            simulationAuthorization,
            cancellationToken);
        Changed?.Invoke(this, EventArgs.Empty);
        return result;
    }

    internal async Task TurnOutputOffAsync(
        byte address,
        Layout3OutputChannel channel,
        CancellationToken cancellationToken)
    {
        await _outputs.TurnOffAsync(address, channel, cancellationToken);
        Changed?.Invoke(this, EventArgs.Empty);
    }

    internal IReadOnlyList<string> FormatOperationLog() =>
        OperationLog.Entries.Select(entry =>
            $"{entry.Timestamp:HH:mm:ss.fff} | SIMULATED | {entry.Mode} | {entry.Operation} | "
            + $"{entry.RegisterAlias ?? "-"} | {entry.Result}"
            + (string.IsNullOrWhiteSpace(entry.Error) ? string.Empty : $" | {entry.Error}"))
        .ToArray();

    public void Dispose()
    {
        _adapter.Dispose();
    }

    private void Synchronize()
    {
        _adapter.SyncInputsToDevice();
        Changed?.Invoke(this, EventArgs.Empty);
    }
}
