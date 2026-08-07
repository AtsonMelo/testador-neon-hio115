using TestadorCLPHI.App.Industrial.Platform.Rtu;
using TestadorCLPHI.App.Industrial.Platform.Simulation;
using TestadorCLPHI.App.Industrial.Platform.Mapping;
using TestadorCLPHI.App.Ui.Industrial.Layout3;

namespace TestadorCLPHI.App.Industrial.Platform.Integration;

internal sealed record SimulationHio115Mapping(
    string ProfileId,
    IReadOnlyDictionary<int, string> DigitalInputs,
    IReadOnlyDictionary<int, string> AnalogInputs,
    IReadOnlyDictionary<Layout3OutputChannel, string> DigitalOutputs);

internal sealed class SimulationHio115Adapter : IDisposable
{
    private readonly SimulationEngine _engine;
    private readonly NeonHio115FakeDevice _device;
    private readonly SimulationHio115Mapping _mapping;

    internal SimulationHio115Adapter(
        SimulationEngine engine,
        NeonHio115FakeDevice device,
        SimulationHio115Mapping mapping)
    {
        _engine = engine ?? throw new ArgumentNullException(nameof(engine));
        _device = device ?? throw new ArgumentNullException(nameof(device));
        _mapping = mapping ?? throw new ArgumentNullException(nameof(mapping));
        if (!string.Equals(engine.Snapshot.ProfileId, mapping.ProfileId, StringComparison.OrdinalIgnoreCase))
        {
            throw new ArgumentException("Mapeamento nao corresponde ao perfil de simulacao.", nameof(mapping));
        }

        _device.OutputChanged += HandleOutputChanged;
    }

    internal int RejectedOutputCommands { get; private set; }

    internal void SyncInputsToDevice()
    {
        foreach ((int channel, string signalId) in _mapping.DigitalInputs)
        {
            _device.SetDigitalInput(channel, _engine.GetValue(signalId) != 0);
        }

        foreach ((int channel, string signalId) in _mapping.AnalogInputs)
        {
            double value = _engine.GetValue(signalId);
            if (value < 0 || value > ushort.MaxValue)
            {
                throw new InvalidOperationException(
                    $"Raw simulado de {signalId} deve permanecer em 0..{ushort.MaxValue}.");
            }

            _device.SetAnalogInput(channel, checked((ushort)Math.Round(value, MidpointRounding.AwayFromZero)));
        }
    }

    public void Dispose()
    {
        _device.OutputChanged -= HandleOutputChanged;
    }

    private void HandleOutputChanged(Layout3OutputChannel channel, bool state)
    {
        if (!_mapping.DigitalOutputs.TryGetValue(channel, out string? signalId))
        {
            return;
        }

        if (!_engine.SetVirtualOutput(signalId, state))
        {
            RejectedOutputCommands++;
        }
    }
}

internal static class SimulationHio115Mappings
{
    internal static SimulationHio115Mapping FromProfile(SimulationProfile profile)
    {
        ArgumentNullException.ThrowIfNull(profile);
        IndustrialIoMappingValidationResult validation = IndustrialIoMappingValidator.Validate(profile);
        if (!validation.IsValid)
        {
            throw new ArgumentException(validation.ToDisplayText(), nameof(profile));
        }

        Dictionary<int, string> digitalInputs = profile.IoBindings
            .Where(binding => binding.Direction == SimulationIoDirection.Input
                && binding.IoType == SimulationIoType.Digital)
            .ToDictionary(binding => binding.Channel, binding => binding.SignalId!);
        Dictionary<int, string> analogInputs = profile.IoBindings
            .Where(binding => binding.Direction == SimulationIoDirection.Input
                && binding.IoType == SimulationIoType.Analog)
            .ToDictionary(binding => binding.Channel, binding => binding.SignalId!);
        Dictionary<Layout3OutputChannel, string> digitalOutputs = profile.IoBindings
            .Where(binding => binding.Direction == SimulationIoDirection.Output
                && binding.IoType == SimulationIoType.Digital)
            .ToDictionary(binding => ToOutputChannel(binding.Channel), binding => binding.SignalId!);

        return new(
            profile.Id!,
            digitalInputs,
            analogInputs,
            digitalOutputs);
    }

    private static Layout3OutputChannel ToOutputChannel(int channel) => channel switch
    {
        0 => Layout3OutputChannel.DO00,
        1 => Layout3OutputChannel.DO01,
        2 => Layout3OutputChannel.DO02,
        3 => Layout3OutputChannel.DO03,
        _ => throw new ArgumentOutOfRangeException(nameof(channel))
    };
}
