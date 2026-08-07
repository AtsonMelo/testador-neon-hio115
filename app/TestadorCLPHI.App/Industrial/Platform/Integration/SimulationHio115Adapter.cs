using TestadorCLPHI.App.Industrial.Platform.Rtu;
using TestadorCLPHI.App.Industrial.Platform.Simulation;
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
    internal static SimulationHio115Mapping ForProfile(string profileId) =>
        profileId.ToLowerInvariant() switch
        {
            "pivo-central" => new(
                "pivo-central",
                new Dictionary<int, string>
                {
                    [0] = "Emergencia",
                    [1] = "Pressostato",
                    [2] = "Alinhamento",
                    [3] = "FimDeCurso",
                    [4] = "FalhaTorre",
                    [5] = "PermissivoAgua"
                },
                new Dictionary<int, string>
                {
                    [0] = "Pressao",
                    [1] = "Corrente",
                    [2] = "PosicaoPercentual"
                },
                new Dictionary<Layout3OutputChannel, string>
                {
                    [Layout3OutputChannel.DO00] = "Bomba",
                    [Layout3OutputChannel.DO01] = "Frente",
                    [Layout3OutputChannel.DO02] = "Reverso",
                    [Layout3OutputChannel.DO03] = "ValvulaAgua"
                }),
            "poco" => new(
                "poco",
                new Dictionary<int, string>
                {
                    [0] = "NivelMinimo",
                    [1] = "NivelMaximo",
                    [2] = "FaltaFase",
                    [3] = "Pressostato",
                    [4] = "Emergencia",
                    [5] = "SensorInvalido"
                },
                new Dictionary<int, string>
                {
                    [0] = "Nivel",
                    [1] = "Pressao",
                    [2] = "Corrente"
                },
                new Dictionary<Layout3OutputChannel, string>
                {
                    [Layout3OutputChannel.DO00] = "Bomba",
                    [Layout3OutputChannel.DO01] = "Valvula"
                }),
            _ => throw new ArgumentException($"Perfil sem mapeamento HIO115: {profileId}.", nameof(profileId))
        };
}
