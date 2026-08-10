using System.Reflection;
using TestadorCLPHI.App.Hardware;
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
            ("startup sem argumentos seleciona a plataforma industrial", StartupWithoutArgumentsSelectsIndustrial),
            ("argumento industrial resolve para a plataforma industrial", IndustrialArgumentSelectsPlatform),
            ("argumento industrial ignora diferenca entre maiusculas e minusculas", IndustrialArgumentIsCaseInsensitive),
            ("argumento legacy resolve exclusivamente para MainForm", LegacyArgumentSelectsMainForm),
            ("argumento legacy ignora diferenca entre maiusculas e minusculas", LegacyArgumentIsCaseInsensitive),
            ("validadores preservam prioridade sobre o launcher industrial", ValidatorsKeepStartupPriority),
            ("argumento desconhecido e rejeitado sem abrir MainForm", UnknownArgumentIsRejected),
            ("rejeicao de argumento desconhecido permanece offline", UnknownArgumentStaysOffline),
            ("launcher industrial cria o host da plataforma", IndustrialLauncherCreatesPlatformHost),
            ("host expoe somente Testador e Simulador", HostHasTwoModes),
            ("host Layout 3 existente carrega Layout3HostControl", Layout3HostLoadsLayout3HostControl),
            ("ciclo de vida do launcher industrial permanece offline", IndustrialLauncherLifecycleStaysOffline),
            ("modo Testador carrega sob demanda", TesterLoadsLazily),
            ("modo Simulador carrega sob demanda", SimulatorLoadsLazily),
            ("Testador exibe aliases e Mapa de I/O do Pivo", PivotAliasesAndIoMapAreVisible),
            ("Mapa de I/O e aliases acompanham o perfil Poco", WellAliasesAndIoMapAreVisible),
            ("renderer leve do Pivo carrega quatro torres sem timer", PivotRendererLoadsWithoutTimer),
            ("renderer do Pivo atualiza somente por mudanca de estado", PivotRendererTracksStateChanges),
            ("perfil Poco preserva editor agrupado sem renderer de Pivo", WellUsesGenericGroupedEditor),
            ("layout estrutural permanece utilizavel em 1366x768", () => LayoutFits(new Size(1366, 768))),
            ("layout estrutural permanece utilizavel em 1920x1080", () => LayoutFits(new Size(1920, 1080))),
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

    private static bool StartupWithoutArgumentsSelectsIndustrial() =>
        Program.ResolveStartupMode([]) == StartupMode.Industrial;

    private static bool IndustrialArgumentSelectsPlatform() =>
        Program.ResolveStartupMode(["--industrial"]) == StartupMode.Industrial;

    private static bool IndustrialArgumentIsCaseInsensitive() =>
        Program.ResolveStartupMode(["--INDUSTRIAL"]) == StartupMode.Industrial;

    private static bool LegacyArgumentSelectsMainForm()
    {
        StartupMode mode = Program.ResolveStartupMode(["--legacy"]);
        return mode == StartupMode.Legacy
            && Program.GetStartupFormType(mode) == typeof(MainForm);
    }

    private static bool LegacyArgumentIsCaseInsensitive()
    {
        StartupMode mode = Program.ResolveStartupMode(["--LEGACY"]);
        return mode == StartupMode.Legacy
            && Program.GetStartupFormType(mode) == typeof(MainForm);
    }

    private static bool ValidatorsKeepStartupPriority()
    {
        string[] validators =
        [
            "--validate-hardware-catalog",
            "--validate-hardware-profile-selection",
            "--validate-hardware-test-report",
            "--validate-layout-3-host-readonly-safety",
            "--validate-layout-3-read-bridge-disabled",
            "--validate-layout-3-read-bridge-activation-gate",
            "--validate-layout-3-bench-readiness-self-tests",
            "--validate-layout-3-simulation-engine",
            "--validate-layout-3-test-simulator-integration",
            "--validate-layout-3-industrial-platform-ui",
            "--validate-industrial-io-mapping",
            "--validate-layout-3-rtu-offline",
            "--validate-layout-3-bench-readiness"
        ];
        return validators.All(argument =>
                Program.ResolveStartupMode([argument]) == StartupMode.Validator)
            && Program.ResolveStartupMode(
                ["--industrial", "--validate-layout-3-industrial-platform-ui"])
                == StartupMode.Validator
            && Program.ResolveStartupMode(
                ["--legacy", "--validate-layout-3-industrial-platform-ui"])
                == StartupMode.Validator
            && Program.ResolveStartupMode(
                ["--unknown-startup-mode", "--validate-layout-3-industrial-platform-ui"])
                == StartupMode.Validator;
    }

    private static bool UnknownArgumentIsRejected()
    {
        StartupMode mode = Program.ResolveStartupMode(["--unknown-startup-mode"]);
        using StringWriter error = new();
        int exitCode = Program.RejectUnknownStartupArguments(
            ["--unknown-startup-mode"],
            error);
        return mode == StartupMode.Unknown
            && Program.GetStartupFormType(mode) is null
            && Program.ResolveStartupMode(["--use-industrial-host"]) == StartupMode.Unknown
            && exitCode != 0
            && error.ToString().Contains("--unknown-startup-mode", StringComparison.Ordinal);
    }

    private static bool UnknownArgumentStaysOffline()
    {
        using IndustrialPlatformSession session = new("pivo-central");
        StartupMode mode = Program.ResolveStartupMode(["--unknown-startup-mode"]);
        using StringWriter error = new();
        int exitCode = Program.RejectUnknownStartupArguments(
            ["--unknown-startup-mode"],
            error);
        return mode == StartupMode.Unknown
            && exitCode != 0
            && PhysicalCountersAreZero(session);
    }

    private static bool IndustrialLauncherCreatesPlatformHost()
    {
        using Form form = Program.CreateStartupForm(StartupMode.Industrial);
        return form is IndustrialPlatformForm;
    }

    private static bool HostHasTwoModes()
    {
        using IndustrialPlatformForm form = new();
        return form.TesterModeButton.Text == "TESTADOR"
            && form.SimulatorModeButton.Text == "SIMULADOR";
    }

    private static bool Layout3HostLoadsLayout3HostControl()
    {
        using Layout3HostForm form = new(HardwareCatalog.Empty);
        return Find<Layout3HostControl>(form) is not null;
    }

    private static bool IndustrialLauncherLifecycleStaysOffline()
    {
        using Form form = Program.CreateStartupForm(StartupMode.Industrial);
        if (form is not IndustrialPlatformForm platform)
        {
            return false;
        }

        return PhysicalCountersAreZero(platform.Session);
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

    private static bool PivotRendererLoadsWithoutTimer()
    {
        using IndustrialPlatformSession session = new("pivo-central");
        using IndustrialSimulatorControl simulator = new(session);
        return simulator.HasPivotRenderer
            && simulator.RenderedTowerCount == 4
            && !simulator.UsesContinuousAnimation
            && simulator.UsesGroupedSignalEditor;
    }

    private static bool PivotRendererTracksStateChanges()
    {
        using IndustrialPlatformSession session = new("pivo-central");
        using IndustrialSimulatorControl simulator = new(session);
        int initialRevision = simulator.PivotStateRevision;
        session.ApplyScenario("movendo-frente");
        int changedRevision = simulator.PivotStateRevision;
        session.ApplyScenario("movendo-frente");
        return initialRevision > 0
            && changedRevision == initialRevision + 1
            && simulator.PivotStateRevision == changedRevision;
    }

    private static bool WellUsesGenericGroupedEditor()
    {
        using IndustrialPlatformSession session = new("poco");
        using IndustrialSimulatorControl simulator = new(session);
        return !simulator.HasPivotRenderer
            && simulator.RenderedTowerCount == 0
            && simulator.UsesGroupedSignalEditor;
    }

    private static bool LayoutFits(Size clientSize)
    {
        using IndustrialPlatformForm form = new()
        {
            ClientSize = clientSize,
            Opacity = 0,
            ShowInTaskbar = false,
            StartPosition = FormStartPosition.Manual,
            Location = new Point(-32000, -32000)
        };
        form.ShowSimulator();
        form.Show();
        PerformLayoutTree(form);
        IndustrialSimulatorControl? simulator = Find<IndustrialSimulatorControl>(form);
        PivotProcessControl? pivot = Find<PivotProcessControl>(form);
        bool simulatorFits = simulator is not null
            && simulator.ClientSize.Width >= 680
            && simulator.ClientSize.Height >= 560
            && pivot is { Width: >= 420, Height: >= 240 };

        form.ShowTester();
        PerformLayoutTree(form);
        IndustrialTesterControl? tester = Find<IndustrialTesterControl>(form);
        TabControl? tabs = Find<TabControl>(form);
        TabPage? mapPage = tabs?.TabPages.Cast<TabPage>().FirstOrDefault(page => page.Name == "ioMappingTab");
        if (tabs is not null && mapPage is not null)
        {
            tabs.SelectedTab = mapPage;
            mapPage.CreateControl();
            PerformLayoutTree(form);
        }

        DataGridView? map = form.Controls.Find("ioMappingGrid", searchAllChildren: true)
            .OfType<DataGridView>()
            .FirstOrDefault();
        bool result = simulatorFits
            && tester is not null
            && tester.ClientSize.Width >= 900
            && tester.ClientSize.Height >= 560
            && map is { Width: >= 700, Height: >= 180 };
        if (!result)
        {
            throw new InvalidOperationException(
                $"cliente={clientSize.Width}x{clientSize.Height}; "
                + $"simulador={simulator?.ClientSize.Width ?? -1}x{simulator?.ClientSize.Height ?? -1}; "
                + $"pivo={pivot?.Width ?? -1}x{pivot?.Height ?? -1}; "
                + $"testador={tester?.ClientSize.Width ?? -1}x{tester?.ClientSize.Height ?? -1}; "
                + $"mapa={map?.Width ?? -1}x{map?.Height ?? -1}.");
        }

        return true;
    }

    private static void PerformLayoutTree(Control control)
    {
        control.PerformLayout();
        foreach (Control child in control.Controls)
        {
            PerformLayoutTree(child);
        }
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
            return PhysicalCountersAreZero(session);
        }).GetAwaiter().GetResult();
    }

    private static bool PhysicalCountersAreZero(IndustrialPlatformSession session) =>
        session.Counters.PhysicalConnections == 0
        && session.Counters.PhysicalReads == 0
        && session.Counters.PhysicalWrites == 0
        && session.Counters.PhysicalCommands == 0
        && session.Simulation.Counters.PhysicalConnections == 0
        && session.Simulation.Counters.PhysicalReads == 0
        && session.Simulation.Counters.PhysicalWrites == 0
        && session.Simulation.Counters.PhysicalCommands == 0;

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
