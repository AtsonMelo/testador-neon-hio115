using TestadorCLPHI.App.Hardware;
using TestadorCLPHI.App.Industrial.Platform.Rtu;
using TestadorCLPHI.App.Industrial.Platform.Simulation;
using TestadorCLPHI.App.Industrial.Platform.Integration;
using TestadorCLPHI.App.Industrial.Platform.Mapping;
using TestadorCLPHI.App.Ui.Controls;
using TestadorCLPHI.App.Ui.Hardware;
using TestadorCLPHI.App.Ui.Industrial;
using TestadorCLPHI.App.Ui.Industrial.Layout3;
using TestadorCLPHI.App.Ui.Industrial.Platform;

namespace TestadorCLPHI.App;

internal enum StartupMode
{
    Legacy,
    Validator,
    Industrial,
    Auxiliary,
    Unknown
}

internal static class Program
{
    private static readonly string[] ValidatorArguments =
    [
        "--validate-hardware-catalog",
        "--validate-hardware-profile-selection",
        "--validate-hardware-test-report",
        "--validate-layout-3-host-readonly-safety",
        "--validate-layout-3-read-bridge-disabled",
        "--validate-layout-3-read-bridge-activation-gate",
        "--validate-layout-3-bench-readiness-self-tests",
        "--validate-layout-3-bench-readiness",
        "--validate-layout-3-rtu-offline",
        "--validate-layout-3-simulation-engine",
        "--validate-layout-3-test-simulator-integration",
        "--validate-layout-3-industrial-platform-ui",
        "--validate-industrial-io-mapping"
    ];

    private static readonly string[] AuxiliaryArguments =
    [
        "--layout-3-host-readonly",
        "--preview-layout-3-auto",
        "--preview-layout-3-light",
        "--preview-layout-3",
        "--preview-layout-alvo",
        "--preview-industrial-panel",
        "--preview-layout-alvo-2",
        "--preview-layout-alvo-2-compacto"
    ];

    [STAThread]
    private static void Main(string[] args)
    {
        StartupMode startupMode = ResolveStartupMode(args);

        if (args.Contains("--validate-hardware-catalog", StringComparer.OrdinalIgnoreCase))
        {
            Environment.ExitCode = ValidateHardwareCatalogForCommandLine();
            return;
        }

        if (args.Contains("--validate-hardware-profile-selection", StringComparer.OrdinalIgnoreCase))
        {
            Environment.ExitCode = HardwareProfileSelectionValidator.ValidateDefaultCatalog(
                Console.Out,
                Console.Error);
            return;
        }

        if (args.Contains("--validate-hardware-test-report", StringComparer.OrdinalIgnoreCase))
        {
            Environment.ExitCode = HardwareTestPreparationReportValidator.ValidateDefaultCatalog(
                Console.Out,
                Console.Error);
            return;
        }

        if (args.Contains("--validate-layout-3-host-readonly-safety", StringComparer.OrdinalIgnoreCase))
        {
            Environment.ExitCode = Layout3HostReadOnlySafetyValidator.ValidateDefaultCatalog(
                Console.Out,
                Console.Error);
            return;
        }

        if (args.Contains("--validate-layout-3-read-bridge-disabled", StringComparer.OrdinalIgnoreCase))
        {
            Environment.ExitCode = Layout3ReadBridgeSafetyValidator.ValidateDefaultCatalog(
                Console.Out,
                Console.Error);
            return;
        }

        if (args.Contains("--validate-layout-3-read-bridge-activation-gate", StringComparer.OrdinalIgnoreCase))
        {
            Environment.ExitCode = Layout3ReadBridgeActivationGateValidator.ValidateDefaultCatalog(
                Console.Out,
                Console.Error);
            return;
        }

        if (args.Contains("--validate-layout-3-bench-readiness-self-tests", StringComparer.OrdinalIgnoreCase))
        {
            Environment.ExitCode = Layout3BenchReadinessScenarioValidator.Validate(
                Console.Out,
                Console.Error);
            return;
        }

        if (args.Contains("--validate-layout-3-bench-readiness", StringComparer.OrdinalIgnoreCase))
        {
            Environment.ExitCode = Layout3BenchReadinessValidator.ValidateDefaultConfiguration(
                Console.Out,
                Console.Error);
            return;
        }

        if (args.Contains("--validate-layout-3-rtu-offline", StringComparer.OrdinalIgnoreCase))
        {
            Environment.ExitCode = RtuOfflineValidator.Validate(Console.Out, Console.Error);
            return;
        }

        if (args.Contains("--validate-layout-3-simulation-engine", StringComparer.OrdinalIgnoreCase))
        {
            Environment.ExitCode = SimulationEngineValidator.Validate(Console.Out, Console.Error);
            return;
        }

        if (args.Contains("--validate-layout-3-test-simulator-integration", StringComparer.OrdinalIgnoreCase))
        {
            Environment.ExitCode = TestSimulatorIntegrationValidator.Validate(Console.Out, Console.Error);
            return;
        }

        if (args.Contains("--validate-layout-3-industrial-platform-ui", StringComparer.OrdinalIgnoreCase))
        {
            Environment.ExitCode = IndustrialPlatformUiValidator.Validate(Console.Out, Console.Error);
            return;
        }

        if (args.Contains("--validate-industrial-io-mapping", StringComparer.OrdinalIgnoreCase))
        {
            Environment.ExitCode = IndustrialIoMappingValidator.ValidateDefaultProfiles(
                Console.Out,
                Console.Error);
            return;
        }

        if (startupMode == StartupMode.Unknown)
        {
            Environment.ExitCode = RejectUnknownStartupArguments(args, Console.Error);
            return;
        }

        ApplicationConfiguration.Initialize();

        if (startupMode is StartupMode.Industrial or StartupMode.Legacy)
        {
            Application.Run(CreateStartupForm(startupMode));
            return;
        }

        if (args.Contains("--layout-3-host-readonly", StringComparer.OrdinalIgnoreCase))
        {
            Application.Run(new Layout3HostForm(LoadHardwareCatalogForApp()));
            return;
        }

        if (args.Contains("--preview-layout-3-auto", StringComparer.OrdinalIgnoreCase))
        {
            Application.Run(new Layout3PreviewForm(
                LoadHardwareCatalogForApp(),
                Layout3PreviewTheme.Automatic));
            return;
        }

        if (args.Contains("--preview-layout-3-light", StringComparer.OrdinalIgnoreCase))
        {
            Application.Run(new Layout3PreviewForm(
                LoadHardwareCatalogForApp(),
                Layout3PreviewTheme.Light));
            return;
        }

        if (args.Contains("--preview-layout-3", StringComparer.OrdinalIgnoreCase))
        {
            Application.Run(new Layout3PreviewForm(LoadHardwareCatalogForApp()));
            return;
        }

        if (args.Contains("--preview-layout-alvo", StringComparer.OrdinalIgnoreCase))
        {
            Application.Run(new IndustrialMainLayoutPreviewForm());
            return;
        }

        if (args.Contains("--preview-industrial-panel", StringComparer.OrdinalIgnoreCase))
        {
            Application.Run(new IndustrialDigitalIoPanelPreviewForm());
            return;
        }

        if (args.Contains("--preview-layout-alvo-2", StringComparer.OrdinalIgnoreCase))
        {
            Application.Run(new IndustrialLayoutAlvo2PreviewForm());
            return;
        }

        if (args.Contains("--preview-layout-alvo-2-compacto", StringComparer.OrdinalIgnoreCase))
        {
            Application.Run(new IndustrialCompactLayoutAlvo2PreviewForm());
            return;
        }

        Environment.ExitCode = RejectUnknownStartupArguments(args, Console.Error);
    }

    internal static StartupMode ResolveStartupMode(IEnumerable<string>? args)
    {
        string[] startupArguments = args?.ToArray() ?? [];
        if (startupArguments.Any(argument => ValidatorArguments.Contains(
                argument,
                StringComparer.OrdinalIgnoreCase)))
        {
            return StartupMode.Validator;
        }

        if (startupArguments.Length == 0)
        {
            return StartupMode.Industrial;
        }

        if (startupArguments.Length != 1)
        {
            return StartupMode.Unknown;
        }

        string argument = startupArguments[0];
        if (argument.Equals("--industrial", StringComparison.OrdinalIgnoreCase)
            || argument.Equals("--industrial-platform", StringComparison.OrdinalIgnoreCase))
        {
            return StartupMode.Industrial;
        }

        if (argument.Equals("--legacy", StringComparison.OrdinalIgnoreCase))
        {
            return StartupMode.Legacy;
        }

        return AuxiliaryArguments.Contains(argument, StringComparer.OrdinalIgnoreCase)
            ? StartupMode.Auxiliary
            : StartupMode.Unknown;
    }

    internal static Form CreateStartupForm(StartupMode startupMode) => startupMode switch
    {
        StartupMode.Industrial => new IndustrialPlatformForm(),
        StartupMode.Legacy => new MainForm(
            useIndustrialHost: false,
            hardwareCatalog: LoadHardwareCatalogForApp()),
        _ => throw new ArgumentOutOfRangeException(
            nameof(startupMode),
            startupMode,
            "O modo informado nao possui formulario de startup.")
    };

    internal static Type? GetStartupFormType(StartupMode startupMode) => startupMode switch
    {
        StartupMode.Industrial => typeof(IndustrialPlatformForm),
        StartupMode.Legacy => typeof(MainForm),
        _ => null
    };

    internal static int RejectUnknownStartupArguments(
        IEnumerable<string>? args,
        TextWriter error)
    {
        string arguments = string.Join(" ", args ?? []);
        error.WriteLine($"Argumento de inicializacao desconhecido: {arguments}");
        return 1;
    }

    private static HardwareCatalog LoadHardwareCatalogForApp()
    {
        try
        {
            HardwareCatalog catalog = HardwareCatalogLoader.LoadDefault();
            HardwareCatalogValidationResult validation = HardwareCatalogValidator.Validate(catalog);

            if (validation.IsValid)
            {
                return catalog;
            }

            MessageBox.Show(
                validation.ToDisplayText(),
                "Catalogo de hardware invalido",
                MessageBoxButtons.OK,
                MessageBoxIcon.Warning);
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                ex.Message,
                "Falha ao carregar catalogo de hardware",
                MessageBoxButtons.OK,
                MessageBoxIcon.Warning);
        }

        return HardwareCatalog.Empty;
    }

    private static int ValidateHardwareCatalogForCommandLine()
    {
        try
        {
            HardwareCatalog catalog = HardwareCatalogLoader.LoadDefault();
            HardwareCatalogValidationResult validation = HardwareCatalogValidator.Validate(catalog);

            if (!validation.IsValid)
            {
                Console.Error.WriteLine(validation.ToDisplayText());
                return 1;
            }

            Console.WriteLine("Catalogo de hardware valido.");
            Console.WriteLine($"Familias: {catalog.Families.Count}");
            Console.WriteLine($"Modelos: {catalog.Models.Count}");
            Console.WriteLine($"Modulos de I/O: {catalog.IoModules.Count}");
            Console.WriteLine($"Perfis de comunicacao: {catalog.CommunicationProfiles.Count}");
            Console.WriteLine($"Perfis de teste: {catalog.TestProfiles.Count}");
            return 0;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine(ex.Message);
            return 1;
        }
    }
}
