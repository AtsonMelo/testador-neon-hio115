using System.Drawing.Drawing2D;
using System.ComponentModel;
using TestadorCLPHI.App.Ui.Theme;

namespace TestadorCLPHI.App.Ui.Controls;

internal sealed class IndustrialButton : Button
{
    private const int CornerRadius = 6;
    private Region? _ownedRegion;
    private bool _isSelected;

    [Browsable(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    internal bool IsNavigation { get; set; }

    [Browsable(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    internal bool IsSelected
    {
        get => _isSelected;
        set
        {
            if (_isSelected == value)
            {
                return;
            }

            _isSelected = value;
            Invalidate();
        }
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
        if (!IsNavigation || !IsSelected)
        {
            return;
        }

        int indicatorWidth = Math.Max(2, (int)Math.Round(3D * DeviceDpi / 96D));
        int inset = Math.Max(3, (int)Math.Round(6D * DeviceDpi / 96D));
        Rectangle indicator = new(
            0,
            inset,
            indicatorWidth,
            Math.Max(1, ClientSize.Height - (inset * 2)));
        using SolidBrush brush = new(IndustrialTheme.Palette.Accent);
        e.Graphics.FillRectangle(brush, indicator);
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

internal sealed class IndustrialGroupBox : GroupBox
{
    private const int CornerRadius = 6;

    internal IndustrialGroupBox()
    {
        SetStyle(
            ControlStyles.AllPaintingInWmPaint
            | ControlStyles.OptimizedDoubleBuffer
            | ControlStyles.ResizeRedraw
            | ControlStyles.UserPaint,
            true);
        Font = IndustrialTypography.SectionTitle();
        Padding = new Padding(
            IndustrialSpacing.Md,
            IndustrialSpacing.Xxl,
            IndustrialSpacing.Md,
            IndustrialSpacing.Md);
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        IndustrialPalette palette = IndustrialTheme.Palette;
        e.Graphics.Clear(palette.SurfaceElevated);
        Rectangle border = ClientRectangle;
        border.Width = Math.Max(0, border.Width - 1);
        border.Height = Math.Max(0, border.Height - 1);
        int radius = Math.Max(1, (int)Math.Round(CornerRadius * DeviceDpi / 96D));
        SmoothingMode previous = e.Graphics.SmoothingMode;
        e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
        using GraphicsPath path = IndustrialControlDrawing.RoundedRectangle(border, radius);
        using Pen borderPen = new(palette.Border, IndustrialSpacing.BorderWidth);
        e.Graphics.DrawPath(borderPen, path);
        e.Graphics.SmoothingMode = previous;

        Rectangle titleBounds = new(
            IndustrialSpacing.Md,
            IndustrialSpacing.Xs,
            Math.Max(0, Width - (IndustrialSpacing.Md * 2)),
            IndustrialSpacing.Xl);
        TextRenderer.DrawText(
            e.Graphics,
            Text,
            Font,
            titleBounds,
            palette.TextPrimary,
            TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis);
        int dividerY = IndustrialSpacing.Xxl - IndustrialSpacing.Xs;
        using Pen divider = new(palette.Border, IndustrialSpacing.BorderWidth);
        e.Graphics.DrawLine(
            divider,
            IndustrialSpacing.Md,
            dividerY,
            Math.Max(IndustrialSpacing.Md, Width - IndustrialSpacing.Md),
            dividerY);
    }
}
