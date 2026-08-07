using System.Reflection;
using TestadorCLPHI.App.Industrial.Platform.Integration;
using TestadorCLPHI.App.Industrial.Platform.Rtu;
using TestadorCLPHI.App.Industrial.Platform.Simulation;
using TestadorCLPHI.App.Ui.Industrial.Layout3;

namespace TestadorCLPHI.App.Ui.Industrial.Platform;

internal static class IndustrialPlatformUiValidator
{
    internal static int Validate(TextWriter output, TextWriter error)
    {
        CanonicalCounters? canonicalCounters = null;
        List<(string Name, Func<bool> Validate)> scenarios =
        [
            ("host expoe somente Testador e Simulador", HostHasTwoModes),
            ("modo Testador carrega sob demanda", TesterLoadsLazily),
            ("modo Simulador carrega sob demanda", SimulatorLoadsLazily),
            ("Testador exibe aliases e Mapa de I/O do Pivo", PivotAliasesAndIoMapAreVisible),
            ("Mapa de I/O e aliases acompanham o perfil Poco", WellAliasesAndIoMapAreVisible),
            ("sessao padrao usa endereco fake 1", SessionUsesFakeAddressOne),
            ("perfil Pivo pode ser manipulado offline", PivotProfileIsInteractive),
            ("perfil Poco pode ser carregado offline", WellProfileLoads),
            ("Testador identifica fake sem transporte fisico", TesterIdentifiesFake),
            ("saida simulada usa contrato fechado e retorna OFF", SimulatedOutputIsMomentary),
            ("novas camadas nao dependem de API serial ou TCP", NewLayersHaveNoPhysicalTransportTypes),
            ("contadores simulados do ciclo canonico sao deterministicos", () =>
            {
                canonicalCounters = CaptureCanonicalCounters();
                return canonicalCounters == new CanonicalCounters(1, 5, 2, 2, 2, 2);
            }),
            ("contadores fisicos permanecem zero", PhysicalCountersRemainZero)
        ];

        int passed = 0;
        foreach ((string name, Func<bool> validate) in scenarios)
        {
            try
            {
                if (!validate())
                {
                    error.WriteLine($"[FAIL] {name}");
                    continue;
                }

                output.WriteLine($"[OK] {name}");
                passed++;
            }
            catch (Exception ex)
            {
                error.WriteLine($"[FAIL] {name}: {ex.Message}");
            }
        }

        output.WriteLine($"Industrial platform UI: {passed}/{scenarios.Count}");
        if (canonicalCounters is not null)
        {
            output.WriteLine(
                $"SimulatedConnections={canonicalCounters.RtuConnections} "
                + $"SimulatedReads={canonicalCounters.RtuReads} "
                + $"SimulatedWrites={canonicalCounters.RtuWrites} "
                + $"SimulatedCommands={canonicalCounters.RtuCommands} "
                + $"SimulationOperations={canonicalCounters.SimulationOperations} "
                + $"SimulationCommands={canonicalCounters.SimulationCommands}");
        }

        output.WriteLine("PhysicalConnections=0 PhysicalReads=0 PhysicalWrites=0 PhysicalCommands=0");
        return passed == scenarios.Count ? 0 : 1;
    }

    private static bool HostHasTwoModes()
    {
        using IndustrialPlatformForm form = new();
        return form.TesterModeButton.Text == "TESTADOR"
            && form.SimulatorModeButton.Text == "SIMULADOR";
    }

    private static bool TesterLoadsLazily()
    {
        using IndustrialPlatformForm form = new();
        form.ShowTester();
        IndustrialTesterControl? tester = Find<IndustrialTesterControl>(form);
        return tester is { UsesOnlyInMemoryTransport: true, RealCommunicationEnabled: false };
    }

    private static bool SimulatorLoadsLazily()
    {
        using IndustrialPlatformForm form = new();
        form.ShowSimulator();
        return Find<IndustrialSimulatorControl>(form) is { HasPhysicalTransport: false };
    }

    private static bool PivotAliasesAndIoMapAreVisible()
    {
        using IndustrialPlatformSession session = new("pivo-central");
        using IndustrialTesterControl tester = new(session);
        return tester.HasIoMappingTab
            && tester.IoMappingRowCount == session.Profile.IoBindings.Count
            && tester.IoMappingDeclaresSimulationEvidence
            && tester.ShowsProcessAlias("DI00", "Emergência")
            && tester.ShowsProcessAlias("AI00", "Pressão")
            && tester.ShowsProcessAlias("DO00", "Bomba");
    }

    private static bool WellAliasesAndIoMapAreVisible()
    {
        using IndustrialPlatformSession session = new("poco");
        using IndustrialTesterControl tester = new(session);
        return tester.IoMappingRowCount == session.Profile.IoBindings.Count
            && tester.ShowsProcessAlias("DI00", "Nível mínimo")
            && tester.ShowsProcessAlias("AI00", "Nível")
            && tester.ShowsProcessAlias("DO01", "Válvula");
    }

    private static bool SessionUsesFakeAddressOne()
    {
        using IndustrialPlatformSession session = new("pivo-central");
        return session.Device.Address == 1;
    }

    private static bool PivotProfileIsInteractive()
    {
        using IndustrialPlatformSession session = new("pivo-central");
        session.SetDigitalInput("Emergencia", true);
        bool emergency = session.Simulation.Snapshot.OutputsBlocked;
        session.ResetSimulation();
        return emergency && !session.Simulation.Snapshot.OutputsBlocked;
    }

    private static bool WellProfileLoads()
    {
        using IndustrialPlatformSession session = new("poco");
        session.ApplyScenario("falta-fase");
        return session.Simulation.Snapshot.State == "Falha";
    }

    private static bool TesterIdentifiesFake()
    {
        return Task.Run(async () =>
        {
            using IndustrialPlatformSession session = new("pivo-central");
            RtuIdentificationResult result = await session.IdentifyAsync(1, CancellationToken.None);
            return result.State == RtuIdentificationState.Identified;
        }).GetAwaiter().GetResult();
    }

    private static bool SimulatedOutputIsMomentary()
    {
        return Task.Run(async () =>
        {
            using IndustrialPlatformSession session = new("pivo-central");
            await session.IdentifyAsync(1, CancellationToken.None);
            RtuOutputTestResult result = await session.ActivateOutputAsync(
                1,
                Layout3OutputChannel.DO00,
                TimeSpan.FromMilliseconds(2),
                operatorExplicitlyEnabled: true,
                CancellationToken.None);
            return result.State == RtuOutputTestState.Completed
                && result.TurnOffConfirmed
                && !session.Device.GetDigitalOutput(Layout3OutputChannel.DO00);
        }).GetAwaiter().GetResult();
    }

    private static bool NewLayersHaveNoPhysicalTransportTypes()
    {
        Type[] types = typeof(IndustrialPlatformSession).Assembly.GetTypes()
            .Where(type => type.Namespace?.StartsWith(
                "TestadorCLPHI.App.Industrial.Platform",
                StringComparison.Ordinal) == true
                || type.Namespace?.StartsWith(
                    "TestadorCLPHI.App.Ui.Industrial.Platform",
                    StringComparison.Ordinal) == true)
            .ToArray();
        string[] forbidden = ["System.IO.Ports", "System.Net.Sockets", "TcpClient", "SerialPort"];
        return types.SelectMany(type => type.GetFields(
                BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic))
            .All(field => forbidden.All(item =>
                field.FieldType.FullName?.Contains(item, StringComparison.OrdinalIgnoreCase) != true));
    }

    private static bool PhysicalCountersRemainZero()
    {
        return Task.Run(async () =>
        {
            using IndustrialPlatformSession session = new("pivo-central");
            await session.IdentifyAsync(1, CancellationToken.None);
            return session.Counters.PhysicalConnections == 0
                && session.Counters.PhysicalReads == 0
                && session.Counters.PhysicalWrites == 0
                && session.Counters.PhysicalCommands == 0
                && session.Simulation.Counters.PhysicalConnections == 0
                && session.Simulation.Counters.PhysicalReads == 0
                && session.Simulation.Counters.PhysicalWrites == 0
                && session.Simulation.Counters.PhysicalCommands == 0;
        }).GetAwaiter().GetResult();
    }

    private static CanonicalCounters CaptureCanonicalCounters() => Task.Run(async () =>
    {
        using IndustrialPlatformSession session = new("pivo-central");
        await session.DiscoverAsync(1, 1, TimeSpan.Zero, progress: null, CancellationToken.None);
        await session.ReadInputsAsync(1, CancellationToken.None);
        await session.ActivateOutputAsync(
            1,
            Layout3OutputChannel.DO00,
            TimeSpan.FromMilliseconds(2),
            operatorExplicitlyEnabled: true,
            CancellationToken.None);
        return new CanonicalCounters(
            session.Counters.SimulatedConnections,
            session.Counters.SimulatedReads,
            session.Counters.SimulatedWrites,
            session.Counters.SimulatedCommands,
            session.Simulation.Counters.SimulatedOperations,
            session.Simulation.Counters.SimulatedCommands);
    }).GetAwaiter().GetResult();

    private static T? Find<T>(Control root)
        where T : Control
    {
        if (root is T match)
        {
            return match;
        }

        foreach (Control child in root.Controls)
        {
            T? found = Find<T>(child);
            if (found is not null)
            {
                return found;
            }
        }

        return null;
    }

    private sealed record CanonicalCounters(
        int RtuConnections,
        int RtuReads,
        int RtuWrites,
        int RtuCommands,
        int SimulationOperations,
        int SimulationCommands);
}
