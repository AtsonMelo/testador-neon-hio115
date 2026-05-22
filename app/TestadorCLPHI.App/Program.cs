using TestadorCLPHI.App.Ui.Controls;
using TestadorCLPHI.App.Ui.Industrial;

namespace TestadorCLPHI.App;

internal static class Program
{
    [STAThread]
    private static void Main(string[] args)
    {
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
        Application.Run(new MainForm(useIndustrialHost));
    }
}
