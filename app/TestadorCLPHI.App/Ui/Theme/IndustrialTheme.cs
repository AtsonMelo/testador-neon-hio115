namespace TestadorCLPHI.App.Ui.Theme;

internal enum IndustrialThemeMode
{
    System,
    Dark,
    Light
}

internal static class IndustrialTheme
{
    private static IndustrialThemeMode _mode = IndustrialThemeMode.Dark;

    internal static event EventHandler? ThemeChanged;

    internal static IndustrialThemeMode Mode => _mode;

    internal static IndustrialPalette Palette => Resolve(_mode);

    internal static void SetMode(IndustrialThemeMode mode)
    {
        if (_mode == mode)
        {
            return;
        }

        _mode = mode;
        ThemeChanged?.Invoke(null, EventArgs.Empty);
    }

    internal static IndustrialPalette Resolve(IndustrialThemeMode mode) => mode switch
    {
        IndustrialThemeMode.Dark => IndustrialPalette.Dark,
        IndustrialThemeMode.Light => IndustrialPalette.Light,
        IndustrialThemeMode.System => ResolveSystemPalette(),
        _ => throw new ArgumentOutOfRangeException(nameof(mode), mode, null)
    };

    internal static IReadOnlyDictionary<string, double> CriticalContrastRatios(
        IndustrialPalette palette) => new Dictionary<string, double>(StringComparer.Ordinal)
        {
            ["TextPrimary/Background"] = ContrastRatio(palette.TextPrimary, palette.Background),
            ["TextPrimary/Surface"] = ContrastRatio(palette.TextPrimary, palette.Surface),
            ["TextSecondary/Surface"] = ContrastRatio(palette.TextSecondary, palette.Surface),
            ["Success/SuccessSurface"] = ContrastRatio(palette.Success, palette.SuccessSurface),
            ["Warning/WarningSurface"] = ContrastRatio(palette.Warning, palette.WarningSurface),
            ["Danger/DangerSurface"] = ContrastRatio(palette.Danger, palette.DangerSurface),
            ["Disabled/Surface"] = ContrastRatio(palette.Disabled, palette.Surface),
            ["AccentText/Accent"] = ContrastRatio(palette.AccentText, palette.Accent),
            ["SelectedText/SelectedSurface"] = ContrastRatio(
                palette.SelectedText,
                palette.SelectedSurface)
        };

    internal static double ContrastRatio(Color foreground, Color background)
    {
        double foregroundLuminance = RelativeLuminance(foreground);
        double backgroundLuminance = RelativeLuminance(background);
        double lighter = Math.Max(foregroundLuminance, backgroundLuminance);
        double darker = Math.Min(foregroundLuminance, backgroundLuminance);
        return (lighter + 0.05D) / (darker + 0.05D);
    }

    internal static void ApplyTitleBar(Form form)
    {
        WindowsTitleBarTheme.Apply(form, Palette.IsDark);
    }

    private static IndustrialPalette ResolveSystemPalette()
    {
        try
        {
            return AppThemeService.WindowsIsInDarkTheme()
                ? IndustrialPalette.Dark
                : IndustrialPalette.Light;
        }
        catch
        {
            return IndustrialPalette.Dark;
        }
    }

    private static double RelativeLuminance(Color color)
    {
        static double Linearize(byte component)
        {
            double value = component / 255D;
            return value <= 0.04045D
                ? value / 12.92D
                : Math.Pow((value + 0.055D) / 1.055D, 2.4D);
        }

        return (0.2126D * Linearize(color.R))
            + (0.7152D * Linearize(color.G))
            + (0.0722D * Linearize(color.B));
    }
}
