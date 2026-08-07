using TestadorCLPHI.App.Hardware;
using TestadorCLPHI.App.Industrial.Platform.Rtu;
using TestadorCLPHI.App.Ui.Controls;
using TestadorCLPHI.App.Ui.Hardware;
using TestadorCLPHI.App.Ui.Industrial;
using TestadorCLPHI.App.Ui.Industrial.Layout3;

namespace TestadorCLPHI.App;

internal static class Program
{
    [STAThread]
    private static void Main(string[] args)
    {
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

        ApplicationConfiguration.Initialize();

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

        bool useIndustrialHost = args.Contains("--use-industrial-host", StringComparer.OrdinalIgnoreCase);
        HardwareCatalog hardwareCatalog = LoadHardwareCatalogForApp();
        Application.Run(new MainForm(useIndustrialHost, hardwareCatalog));
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
