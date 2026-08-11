namespace TestadorCLPHI.App.Ui.Theme;

internal static class IndustrialTypography
{
    private const string UiFamily = "Segoe UI";
    private const string UiSemiboldFamily = "Segoe UI Semibold";
    private const string TechnicalFamily = "Consolas";

    internal static Font ProductTitle() => new(UiSemiboldFamily, 16F, FontStyle.Regular);
    internal static Font PageTitle() => new(UiSemiboldFamily, 14F, FontStyle.Regular);
    internal static Font SectionTitle() => new(UiSemiboldFamily, 10.5F, FontStyle.Regular);
    internal static Font Body() => new(UiFamily, 9.5F, FontStyle.Regular);
    internal static Font FieldLabel() => new(UiFamily, 9F, FontStyle.Regular);
    internal static Font Button() => new(UiSemiboldFamily, 9.5F, FontStyle.Regular);
    internal static Font Navigation() => new(UiSemiboldFamily, 9.5F, FontStyle.Regular);
    internal static Font Status() => new(UiSemiboldFamily, 8.5F, FontStyle.Regular);
    internal static Font Technical() => new(TechnicalFamily, 9F, FontStyle.Regular);

    internal static Font Display() => new(UiSemiboldFamily, 20F, FontStyle.Regular);
    internal static Font Title() => PageTitle();
    internal static Font Section() => SectionTitle();
    internal static Font BodyStrong() => Button();
    internal static Font Caption() => new(UiFamily, 8.5F, FontStyle.Regular);
    internal static Font CaptionStrong() => Status();
}
