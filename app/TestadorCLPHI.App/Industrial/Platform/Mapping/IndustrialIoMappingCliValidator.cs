using TestadorCLPHI.App.Industrial.Platform.Devices;
using TestadorCLPHI.App.Industrial.Platform.Simulation;

namespace TestadorCLPHI.App.Industrial.Platform.Mapping;

internal static class IndustrialIoMappingCliValidator
{
    private sealed record Scenario(string Name, Func<bool> Run);

    internal static int ValidateDefaultProfiles(TextWriter output, TextWriter error)
    {
        ArgumentNullException.ThrowIfNull(output);
        ArgumentNullException.ThrowIfNull(error);
        IReadOnlyList<Scenario> scenarios = BuildScenarios();
        int passed = 0;
        output.WriteLine("VALIDADOR DO MAPEAMENTO INDUSTRIAL DE I/O");
        output.WriteLine("Escopo: bindings dos perfis para o fake HIO115; nenhuma comunicacao fisica.");
        output.WriteLine();

        foreach (Scenario scenario in scenarios)
        {
            bool result;
            try
            {
                result = scenario.Run();
            }
            catch (Exception ex)
            {
                result = false;
                output.WriteLine($"[ERRO] {scenario.Name}: {ex.GetType().Name}: {ex.Message}");
            }

            output.WriteLine(result ? $"[OK] {scenario.Name}" : $"[ERRO] {scenario.Name}");
            if (result)
            {
                passed++;
            }
        }

        output.WriteLine();
        output.WriteLine($"Cenarios aprovados: {passed}/{scenarios.Count}");
        output.WriteLine("PhysicalConnections: 0");
        output.WriteLine("PhysicalReads: 0");
        output.WriteLine("PhysicalWrites: 0");
        output.WriteLine("PhysicalCommands: 0");
        output.WriteLine(passed == scenarios.Count
            ? "Resultado: IO_MAPPING_READY"
            : "Resultado: ERRO");
        if (passed != scenarios.Count)
        {
            error.WriteLine("Validacao do mapeamento industrial de I/O falhou.");
        }

        return passed == scenarios.Count ? 0 : 1;
    }

    private static IReadOnlyList<Scenario> BuildScenarios() =>
    [
        new("perfil Pivo possui mapa valido", () => IndustrialIoMappingValidator.Validate(Load("pivo-central")).IsValid),
        new("perfil Poco possui mapa valido", () => IndustrialIoMappingValidator.Validate(Load("poco")).IsValid),
        new("todos SignalIds do Pivo existem", () => AllSignalsExist(Load("pivo-central"))),
        new("todos SignalIds do Poco existem", () => AllSignalsExist(Load("poco"))),
        new("Pivo nao duplica canais DI AI ou DO", () => HasUniqueChannels(Load("pivo-central"))),
        new("Poco nao duplica canais DI AI ou DO", () => HasUniqueChannels(Load("poco"))),
        new("DI conecta somente entrada digital", () => BindingsOfTypeAreCompatible(
            Load("pivo-central"),
            SimulationIoDirection.Input,
            SimulationIoType.Digital,
            SimulationSignalKind.DigitalInput)),
        new("AI conecta somente entrada analogica", () => BindingsOfTypeAreCompatible(
            Load("pivo-central"),
            SimulationIoDirection.Input,
            SimulationIoType.Analog,
            SimulationSignalKind.AnalogInput)),
        new("DO conecta somente saida virtual", () => BindingsOfTypeAreCompatible(
            Load("pivo-central"),
            SimulationIoDirection.Output,
            SimulationIoType.Digital,
            SimulationSignalKind.VirtualOutput)),
        new("DI00 usa registro documentado 31120", () => HasKnownBinding(Load("pivo-central"), "DI00", 31120)),
        new("AI00 usa registro documentado 31132", () => HasKnownBinding(Load("pivo-central"), "AI00", 31132)),
        new("DO00 usa registro documentado 31128", () => HasKnownBinding(Load("pivo-central"), "DO00", 31128)),
        new("todos registros pertencem a allow-list", () => Load("pivo-central").IoBindings.All(binding =>
            DeviceProfileSimulationIoPolicy.IsAllowListed(
                NeonHio115DeviceProfile.Current,
                binding.Direction,
                binding.Register))),
        new("nenhum binding usa reservado ou PWM", () => Load("pivo-central").IoBindings.All(binding =>
            binding.RegisterAlias?.Contains("PWM", StringComparison.OrdinalIgnoreCase) != true
            && binding.Description?.Contains("reservado", StringComparison.OrdinalIgnoreCase) != true)),
        new("somente DO00 ate DO03 sao aceitos", () =>
        {
            SimulationProfile profile = Load("pivo-central");
            profile.IoBindings.First(binding => binding.RegisterAlias == "DO03").Channel = 4;
            return !IndustrialIoMappingValidator.Validate(profile).IsValid;
        }),
        new("registro reservado falha fechado", () =>
        {
            SimulationProfile profile = Load("pivo-central");
            profile.IoBindings[0].Register = 31135;
            return !IndustrialIoMappingValidator.Validate(profile).IsValid;
        }),
        new("alias PWM falha fechado", () =>
        {
            SimulationProfile profile = Load("pivo-central");
            profile.IoBindings[0].RegisterAlias = "PWM00";
            return !IndustrialIoMappingValidator.Validate(profile).IsValid;
        }),
        new("canal duplicado falha fechado", () =>
        {
            SimulationProfile profile = Load("pivo-central");
            profile.IoBindings[1].Channel = profile.IoBindings[0].Channel;
            profile.IoBindings[1].Register = profile.IoBindings[0].Register;
            profile.IoBindings[1].RegisterAlias = profile.IoBindings[0].RegisterAlias;
            return !IndustrialIoMappingValidator.Validate(profile).IsValid;
        }),
        new("sinal inexistente falha fechado", () =>
        {
            SimulationProfile profile = Load("pivo-central");
            profile.IoBindings[0].SignalId = "NaoExiste";
            return !IndustrialIoMappingValidator.Validate(profile).IsValid;
        }),
        new("evidencia fisica nao pode ser declarada pelo perfil simulado", () =>
        {
            SimulationProfile profile = Load("pivo-central");
            profile.IoBindings[0].EvidenceStatus = SimulationIoEvidenceStatus.PhysicalConfirmed;
            return !IndustrialIoMappingValidator.Validate(profile).IsValid;
        })
    ];

    private static SimulationProfile Load(string profileId) => SimulationProfileLoader.Load(profileId);

    private static bool AllSignalsExist(SimulationProfile profile) =>
        profile.IoBindings.All(binding => profile.Signals.Any(signal =>
            string.Equals(signal.Id, binding.SignalId, StringComparison.OrdinalIgnoreCase)));

    private static bool HasUniqueChannels(SimulationProfile profile) =>
        profile.IoBindings.Select(binding => $"{binding.Direction}:{binding.IoType}:{binding.Channel}")
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .Count() == profile.IoBindings.Count;

    private static bool BindingsOfTypeAreCompatible(
        SimulationProfile profile,
        SimulationIoDirection direction,
        SimulationIoType ioType,
        SimulationSignalKind expectedKind) =>
        profile.IoBindings.Where(binding => binding.Direction == direction && binding.IoType == ioType)
            .All(binding => profile.Signals.First(signal => string.Equals(
                signal.Id,
                binding.SignalId,
                StringComparison.OrdinalIgnoreCase)).Kind == expectedKind);

    private static bool HasKnownBinding(SimulationProfile profile, string alias, int register) =>
        profile.IoBindings.Any(binding => binding.RegisterAlias == alias && binding.Register == register);
}
