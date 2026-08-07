using TestadorCLPHI.App.Industrial.Platform.Simulation;
using TestadorCLPHI.App.Ui.Industrial.Layout3;

namespace TestadorCLPHI.App.Industrial.Platform.Mapping;

internal sealed record IndustrialIoMappingValidationResult(IReadOnlyList<string> Failures)
{
    internal bool IsValid => Failures.Count == 0;
    internal string ToDisplayText() => string.Join(Environment.NewLine, Failures);
}

internal static class Hio115SimulationIoPolicy
{
    internal static bool TryResolve(
        SimulationIoDirection direction,
        SimulationIoType ioType,
        int channel,
        out string registerAlias,
        out int documentedRegister)
    {
        registerAlias = string.Empty;
        documentedRegister = 0;
        if (direction == SimulationIoDirection.Input
            && ioType == SimulationIoType.Digital
            && channel is >= 0 and < 8)
        {
            registerAlias = $"DI{channel:00}";
            documentedRegister = 31120 + channel;
            return true;
        }

        if (direction == SimulationIoDirection.Input
            && ioType == SimulationIoType.Analog
            && channel is >= 0 and < 3)
        {
            registerAlias = $"AI{channel:00}";
            documentedRegister = 31132 + channel;
            return true;
        }

        if (direction == SimulationIoDirection.Output
            && ioType == SimulationIoType.Digital
            && channel is >= 0 and < 4)
        {
            registerAlias = $"DO{channel:00}";
            documentedRegister = 31128 + channel;
            return true;
        }

        return false;
    }

    internal static bool IsAllowListed(
        SimulationIoDirection direction,
        int documentedRegister) =>
        direction switch
        {
            SimulationIoDirection.Input => Layout3BenchWorkflowPolicy.IsReadReferenceAllowed(
                Layout3BenchMode.InputTest,
                documentedRegister),
            SimulationIoDirection.Output =>
                Layout3BenchWorkflowPolicy.IsOutputReferenceAllowed(documentedRegister),
            _ => false
        };
}

internal static class IndustrialIoMappingValidator
{
    private sealed record Scenario(string Name, Func<bool> Run);

    internal static IndustrialIoMappingValidationResult Validate(SimulationProfile profile)
    {
        ArgumentNullException.ThrowIfNull(profile);
        List<string> failures = [];
        if (profile.IoBindings.Count == 0)
        {
            failures.Add("Perfil sem ioBindings para o fake HIO115.");
            return new(failures);
        }

        HashSet<string> signalIds = new(StringComparer.OrdinalIgnoreCase);
        HashSet<string> channelKeys = new(StringComparer.OrdinalIgnoreCase);
        foreach (SimulationIoBinding binding in profile.IoBindings)
        {
            SimulationSignalDefinition? signal = profile.Signals.FirstOrDefault(item =>
                string.Equals(item.Id, binding.SignalId, StringComparison.OrdinalIgnoreCase));
            if (signal is null)
            {
                failures.Add($"Binding referencia sinal inexistente: {binding.SignalId ?? "AUSENTE"}.");
                continue;
            }

            if (!signalIds.Add(binding.SignalId!))
            {
                failures.Add($"Sinal duplicado no mapa: {binding.SignalId}.");
            }

            string channelKey = $"{binding.Direction}:{binding.IoType}:{binding.Channel}";
            if (!channelKeys.Add(channelKey))
            {
                failures.Add($"Canal duplicado no mapa: {channelKey}.");
            }

            if (!IsCompatible(signal.Kind, binding.Direction, binding.IoType))
            {
                failures.Add($"Binding {binding.SignalId}: tipo incompativel com o sinal.");
            }

            if (binding.EvidenceStatus != SimulationIoEvidenceStatus.SimulationProfile)
            {
                failures.Add($"Binding {binding.SignalId}: evidenceStatus deve ser SimulationProfile.");
            }

            if (!Hio115SimulationIoPolicy.TryResolve(
                    binding.Direction,
                    binding.IoType,
                    binding.Channel,
                    out string expectedAlias,
                    out int expectedRegister))
            {
                failures.Add($"Binding {binding.SignalId}: canal fora do fake HIO115 permitido.");
                continue;
            }

            if (!string.Equals(binding.RegisterAlias, expectedAlias, StringComparison.Ordinal))
            {
                failures.Add(
                    $"Binding {binding.SignalId}: alias {binding.RegisterAlias ?? "AUSENTE"}; esperado {expectedAlias}.");
            }

            if (binding.Register != expectedRegister)
            {
                failures.Add(
                    $"Binding {binding.SignalId}: registro {binding.Register}; esperado {expectedRegister}.");
            }

            if (!Hio115SimulationIoPolicy.IsAllowListed(binding.Direction, binding.Register))
            {
                failures.Add($"Binding {binding.SignalId}: registro fora da allow-list.");
            }

            if (binding.RegisterAlias?.Contains("PWM", StringComparison.OrdinalIgnoreCase) == true
                || binding.Description?.Contains("reservado", StringComparison.OrdinalIgnoreCase) == true)
            {
                failures.Add($"Binding {binding.SignalId}: PWM/reservado nao permitido.");
            }
        }

        return new(failures);
    }

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
        new("perfil Pivo possui mapa valido", () => Validate(Load("pivo-central")).IsValid),
        new("perfil Poco possui mapa valido", () => Validate(Load("poco")).IsValid),
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
        new("DI00 usa registro documentado 31120", () => HasKnownBinding(
            Load("pivo-central"), "DI00", 31120)),
        new("AI00 usa registro documentado 31132", () => HasKnownBinding(
            Load("pivo-central"), "AI00", 31132)),
        new("DO00 usa registro documentado 31128", () => HasKnownBinding(
            Load("pivo-central"), "DO00", 31128)),
        new("todos registros pertencem a allow-list", () => Load("pivo-central").IoBindings.All(binding =>
            Hio115SimulationIoPolicy.IsAllowListed(binding.Direction, binding.Register))),
        new("nenhum binding usa reservado ou PWM", () => Load("pivo-central").IoBindings.All(binding =>
            binding.RegisterAlias?.Contains("PWM", StringComparison.OrdinalIgnoreCase) != true
            && binding.Description?.Contains("reservado", StringComparison.OrdinalIgnoreCase) != true)),
        new("somente DO00 ate DO03 sao aceitos", () =>
        {
            SimulationProfile profile = Load("pivo-central");
            profile.IoBindings.First(binding => binding.RegisterAlias == "DO03").Channel = 4;
            return !Validate(profile).IsValid;
        }),
        new("registro reservado falha fechado", () =>
        {
            SimulationProfile profile = Load("pivo-central");
            profile.IoBindings[0].Register = 31135;
            return !Validate(profile).IsValid;
        }),
        new("alias PWM falha fechado", () =>
        {
            SimulationProfile profile = Load("pivo-central");
            profile.IoBindings[0].RegisterAlias = "PWM00";
            return !Validate(profile).IsValid;
        }),
        new("canal duplicado falha fechado", () =>
        {
            SimulationProfile profile = Load("pivo-central");
            profile.IoBindings[1].Channel = profile.IoBindings[0].Channel;
            profile.IoBindings[1].Register = profile.IoBindings[0].Register;
            profile.IoBindings[1].RegisterAlias = profile.IoBindings[0].RegisterAlias;
            return !Validate(profile).IsValid;
        }),
        new("sinal inexistente falha fechado", () =>
        {
            SimulationProfile profile = Load("pivo-central");
            profile.IoBindings[0].SignalId = "NaoExiste";
            return !Validate(profile).IsValid;
        }),
        new("evidencia fisica nao pode ser declarada pelo perfil simulado", () =>
        {
            SimulationProfile profile = Load("pivo-central");
            profile.IoBindings[0].EvidenceStatus = SimulationIoEvidenceStatus.PhysicalConfirmed;
            return !Validate(profile).IsValid;
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

    private static bool IsCompatible(
        SimulationSignalKind signalKind,
        SimulationIoDirection direction,
        SimulationIoType ioType) =>
        (signalKind, direction, ioType) switch
        {
            (SimulationSignalKind.DigitalInput, SimulationIoDirection.Input, SimulationIoType.Digital) => true,
            (SimulationSignalKind.AnalogInput, SimulationIoDirection.Input, SimulationIoType.Analog) => true,
            (SimulationSignalKind.VirtualOutput, SimulationIoDirection.Output, SimulationIoType.Digital) => true,
            _ => false
        };
}
