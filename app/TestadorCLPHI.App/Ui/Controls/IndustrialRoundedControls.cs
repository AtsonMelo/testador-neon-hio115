using System.Drawing.Drawing2D;
using TestadorCLPHI.App.Ui.Theme;

namespace TestadorCLPHI.App.Ui.Controls;

internal sealed class IndustrialButton : Button
{
    private const int CornerRadius = 6;
    private Region? _ownedRegion;

    protected override void OnHandleCreated(EventArgs e)
    {
        base.OnHandleCreated(e);
        UpdateRoundedRegion();
    }

    protected override void OnResize(EventArgs e)
    {
        base.OnResize(e);
        UpdateRoundedRegion();
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            Region = null;
            _ownedRegion?.Dispose();
            _ownedRegion = null;
        }

        base.Dispose(disposing);
    }

    private void UpdateRoundedRegion()
    {
        if (ClientSize.Width <= 0 || ClientSize.Height <= 0)
        {
            return;
        }

        int radius = Math.Max(1, (int)Math.Round(CornerRadius * DeviceDpi / 96D));
        using GraphicsPath path = IndustrialControlDrawing.RoundedRectangle(ClientRectangle, radius);
        Region replacement = new(path);
        Region? previous = _ownedRegion;
        _ownedRegion = replacement;
        Region = replacement;
        previous?.Dispose();
    }
}

internal sealed class IndustrialSurfacePanel : Panel
{
    private const int CornerRadius = 8;
    private Region? _ownedRegion;

    internal IndustrialSurfacePanel()
    {
        SetStyle(
            ControlStyles.AllPaintingInWmPaint
            | ControlStyles.OptimizedDoubleBuffer
            | ControlStyles.ResizeRedraw,
            true);
    }

    protected override void OnHandleCreated(EventArgs e)
    {
        base.OnHandleCreated(e);
        UpdateRoundedRegion();
    }

    protected override void OnResize(EventArgs e)
    {
        base.OnResize(e);
        UpdateRoundedRegion();
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);
        Rectangle border = ClientRectangle;
        border.Width = Math.Max(0, border.Width - 1);
        border.Height = Math.Max(0, border.Height - 1);
        int radius = Math.Max(1, (int)Math.Round(CornerRadius * DeviceDpi / 96D));
        SmoothingMode previous = e.Graphics.SmoothingMode;
        e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
        using GraphicsPath path = IndustrialControlDrawing.RoundedRectangle(border, radius);
        using Pen pen = new(IndustrialTheme.Palette.Border, IndustrialSpacing.BorderWidth);
        e.Graphics.DrawPath(pen, path);
        e.Graphics.SmoothingMode = previous;
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            Region = null;
            _ownedRegion?.Dispose();
            _ownedRegion = null;
        }

        base.Dispose(disposing);
    }

    private void UpdateRoundedRegion()
    {
        if (ClientSize.Width <= 0 || ClientSize.Height <= 0)
        {
            return;
        }

        int radius = Math.Max(1, (int)Math.Round(CornerRadius * DeviceDpi / 96D));
        using GraphicsPath path = IndustrialControlDrawing.RoundedRectangle(ClientRectangle, radius);
        Region replacement = new(path);
        Region? previous = _ownedRegion;
        _ownedRegion = replacement;
        Region = replacement;
        previous?.Dispose();
    }
}
