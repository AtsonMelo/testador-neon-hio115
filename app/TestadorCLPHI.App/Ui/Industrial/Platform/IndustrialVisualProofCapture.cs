using TestadorCLPHI.App.Ui.Theme;

namespace TestadorCLPHI.App.Ui.Industrial.Platform;

internal static class IndustrialVisualProofCapture
{
    private const string OutputDirectoryVariable = "TESTADOR_UI_PROOF_DIR";

    internal static bool CaptureRequested()
    {
        string? directory = Environment.GetEnvironmentVariable(OutputDirectoryVariable);
        if (string.IsNullOrWhiteSpace(directory))
        {
            return false;
        }

        Directory.CreateDirectory(directory);
        CaptureRtuMatrix(directory);
        return true;
    }

    private static void CaptureRtuMatrix(string directory)
    {
        IndustrialThemeMode original = IndustrialTheme.Mode;
        try
        {
            foreach ((IndustrialThemeMode mode, string theme) in new[]
                     {
                         (IndustrialThemeMode.Dark, "dark"),
                         (IndustrialThemeMode.Light, "light")
                     })
            {
                foreach (int width in new[] { 1366, 1920 })
                {
                    IndustrialTheme.SetMode(mode);
                    using IndustrialPlatformForm form = new()
                    {
                        MinimumSize = Size.Empty,
                        ClientSize = width == 1366 ? new Size(1366, 768) : new Size(1920, 1080),
                        FormBorderStyle = FormBorderStyle.None,
                        ShowInTaskbar = false,
                        StartPosition = FormStartPosition.Manual,
                        Location = new Point(-32000, -32000)
                    };
                    form.ShowTester();
                    form.Show();
                    PerformLayoutTree(form);
                    using Bitmap bitmap = new(form.ClientSize.Width, form.ClientSize.Height);
                    form.DrawToBitmap(bitmap, form.ClientRectangle);
                    bitmap.Save(Path.Combine(directory, $"rtu-{theme}-{width}.png"));
                    form.Hide();
                }
            }
        }
        finally
        {
            IndustrialTheme.SetMode(original);
        }
    }

    private static void PerformLayoutTree(Control control)
    {
        control.PerformLayout();
        foreach (Control child in control.Controls)
        {
            PerformLayoutTree(child);
        }
    }
}
