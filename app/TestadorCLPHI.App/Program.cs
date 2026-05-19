using TestadorCLPHI.App.Ui.Controls;

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

        Application.Run(new MainForm());
    }
}
