namespace TestadorCLPHI.App.Ui.Theme;

internal static class IndustrialButtonVisuals
{
    internal const int CornerRadius = 6;
    internal const int BorderWidth = 1;

    private const int DarkHoverOverlayAlpha = 18;
    private const int LightHoverOverlayAlpha = 12;
    private const int DarkPressedOverlayAlpha = 28;
    private const int LightPressedOverlayAlpha = 20;

    internal static Color HoverOverlay(IndustrialPalette palette) => palette.IsDark
        ? Color.FromArgb(DarkHoverOverlayAlpha, Color.White)
        : Color.FromArgb(LightHoverOverlayAlpha, Color.Black);

    internal static Color PressedOverlay(IndustrialPalette palette) => palette.IsDark
        ? Color.FromArgb(DarkPressedOverlayAlpha, Color.Black)
        : Color.FromArgb(LightPressedOverlayAlpha, Color.Black);

    internal static float ScaleBorder(int dpi) => Math.Max(
        1F,
        BorderWidth * Math.Max(96, dpi) / 96F);

    internal static float ScaleRadius(int dpi) => Math.Max(
        1F,
        CornerRadius * Math.Max(96, dpi) / 96F);

    internal static RectangleF InsetStrokeBounds(Rectangle clientRectangle, float strokeWidth)
    {
        float inset = strokeWidth / 2F;
        return new RectangleF(
            clientRectangle.Left + inset,
            clientRectangle.Top + inset,
            Math.Max(0F, clientRectangle.Width - strokeWidth),
            Math.Max(0F, clientRectangle.Height - strokeWidth));
    }
}
