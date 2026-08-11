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
        CaptureScreenMatrix(directory);
        CaptureControlLab(directory);
        return true;
    }

    private static void CaptureScreenMatrix(string directory)
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
                    foreach ((string page, Action<IndustrialPlatformForm> showPage) in new[]
                             {
                                 ("home", new Action<IndustrialPlatformForm>(form => form.ShowHome())),
                                 ("tester", new Action<IndustrialPlatformForm>(form => form.ShowTester())),
                                 ("simulator", new Action<IndustrialPlatformForm>(form => form.ShowSimulator()))
                             })
                    {
                        IndustrialTheme.SetMode(mode);
                        using IndustrialPlatformForm form = CreateOffscreenShell(width);
                        showPage(form);
                        form.Show();
                        Application.DoEvents();
                        PerformLayoutTree(form);
                        form.Update();
                        using Bitmap bitmap = new(form.ClientSize.Width, form.ClientSize.Height);
                        form.DrawToBitmap(bitmap, form.ClientRectangle);
                        bitmap.Save(Path.Combine(directory, $"{page}-{theme}-{width}.png"));
                        if (page == "tester")
                        {
                            bitmap.Save(Path.Combine(directory, $"rtu-{theme}-{width}.png"));
                        }

                        form.Hide();
                    }
                }
            }
        }
        finally
        {
            IndustrialTheme.SetMode(original);
        }
    }

    private static void CaptureControlLab(string directory)
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
                IndustrialTheme.SetMode(mode);
                using IndustrialDesignSystemPreviewForm form = new()
                {
                    StartPosition = FormStartPosition.Manual,
                    Location = new Point(-32000, -32000)
                };
                form.Show();
                Application.DoEvents();
                PerformLayoutTree(form);
                form.Update();
                using Bitmap bitmap = new(form.ClientSize.Width, form.ClientSize.Height);
                form.DrawToBitmap(bitmap, form.ClientRectangle);
                bitmap.Save(Path.Combine(directory, $"control-lab-{theme}.png"));
                form.Hide();
            }
        }
        finally
        {
            IndustrialTheme.SetMode(original);
        }
    }

    private static IndustrialPlatformForm CreateOffscreenShell(int width) => new()
    {
        MinimumSize = Size.Empty,
        ClientSize = width == 1366 ? new Size(1366, 768) : new Size(1920, 1080),
        FormBorderStyle = FormBorderStyle.None,
        ShowInTaskbar = false,
        StartPosition = FormStartPosition.Manual,
        Location = new Point(-32000, -32000)
    };

    private static void PerformLayoutTree(Control control)
    {
        control.PerformLayout();
        foreach (Control child in control.Controls)
        {
            PerformLayoutTree(child);
        }
    }
}
