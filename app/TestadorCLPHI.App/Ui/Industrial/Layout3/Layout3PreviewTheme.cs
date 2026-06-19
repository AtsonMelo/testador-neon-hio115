namespace TestadorCLPHI.App.Ui.Industrial.Layout3;

public enum Layout3PreviewTheme
{
    Dark,
    Light,
    Automatic
}

internal sealed record Layout3ThemePalette(
    bool UseDarkTitleBar,
    Color Background,
    Color Surface,
    Color Raised,
    Color Field,
    Color Row,
    Color Divider,
    Color Text,
    Color MutedText,
    Color AccentBlue,
    Color Warning,
    Color WarningBackground,
    Color Emergency,
    Color EmergencyBackground,
    Color TerminalBackground,
    Color TerminalText,
    Color SummaryText,
    Color ButtonHover,
    Color ProfileButtonHover)
{
    public static Layout3ThemePalette For(Layout3PreviewTheme theme)
    {
        return theme switch
        {
            Layout3PreviewTheme.Dark => Dark,
            Layout3PreviewTheme.Light => Light,
            Layout3PreviewTheme.Automatic => AppThemeService.WindowsIsInDarkTheme() ? Dark : Light,
            _ => throw new ArgumentOutOfRangeException(nameof(theme), theme, null)
        };
    }

    private static Layout3ThemePalette Dark { get; } = new(
        UseDarkTitleBar: true,
        Background: Color.FromArgb(18, 24, 32),
        Surface: Color.FromArgb(24, 32, 42),
        Raised: Color.FromArgb(42, 52, 64),
        Field: Color.FromArgb(18, 25, 34),
        Row: Color.FromArgb(34, 44, 56),
        Divider: Color.FromArgb(72, 96, 116),
        Text: Color.FromArgb(226, 232, 240),
        MutedText: Color.FromArgb(170, 184, 198),
        AccentBlue: Color.FromArgb(83, 151, 210),
        Warning: Color.FromArgb(222, 178, 72),
        WarningBackground: Color.FromArgb(53, 45, 27),
        Emergency: Color.FromArgb(224, 92, 92),
        EmergencyBackground: Color.FromArgb(43, 30, 33),
        TerminalBackground: Color.FromArgb(5, 9, 13),
        TerminalText: Color.FromArgb(178, 191, 203),
        SummaryText: Color.FromArgb(190, 202, 213),
        ButtonHover: Color.FromArgb(31, 45, 58),
        ProfileButtonHover: Color.FromArgb(52, 64, 78));

    private static Layout3ThemePalette Light { get; } = new(
        UseDarkTitleBar: false,
        Background: Color.FromArgb(238, 242, 246),
        Surface: Color.FromArgb(250, 251, 252),
        Raised: Color.FromArgb(225, 231, 237),
        Field: Color.FromArgb(242, 245, 248),
        Row: Color.FromArgb(233, 238, 243),
        Divider: Color.FromArgb(174, 185, 196),
        Text: Color.FromArgb(36, 45, 54),
        MutedText: Color.FromArgb(92, 104, 116),
        AccentBlue: Color.FromArgb(45, 104, 160),
        Warning: Color.FromArgb(122, 87, 12),
        WarningBackground: Color.FromArgb(255, 245, 204),
        Emergency: Color.FromArgb(174, 42, 42),
        EmergencyBackground: Color.FromArgb(253, 232, 232),
        TerminalBackground: Color.FromArgb(245, 247, 249),
        TerminalText: Color.FromArgb(54, 65, 75),
        SummaryText: Color.FromArgb(65, 76, 87),
        ButtonHover: Color.FromArgb(212, 220, 228),
        ProfileButtonHover: Color.FromArgb(212, 220, 228));
}
