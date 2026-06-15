using TestadorCLPHI.App.Hardware;
using TestadorCLPHI.App.Ui.Controls;
using TestadorCLPHI.App.Ui.Hardware;
using TestadorCLPHI.App.Ui.Industrial;

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

        ApplicationConfiguration.Initialize();

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
