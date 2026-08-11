namespace TestadorCLPHI.App.Ui.Theme;

internal static class IndustrialTypography
{
    private const string UiFamily = "Segoe UI";
    private const string UiSemiboldFamily = "Segoe UI Semibold";
    private const string TechnicalFamily = "Consolas";

    internal static Font Display() => new(UiSemiboldFamily, 20F, FontStyle.Regular);
    internal static Font Title() => new(UiSemiboldFamily, 16F, FontStyle.Regular);
    internal static Font Section() => new(UiSemiboldFamily, 11F, FontStyle.Regular);
    internal static Font Body() => new(UiFamily, 9.5F, FontStyle.Regular);
    internal static Font BodyStrong() => new(UiSemiboldFamily, 9.5F, FontStyle.Regular);
    internal static Font Caption() => new(UiFamily, 8.5F, FontStyle.Regular);
    internal static Font CaptionStrong() => new(UiSemiboldFamily, 8.5F, FontStyle.Regular);
    internal static Font Technical() => new(TechnicalFamily, 9F, FontStyle.Regular);
}
