using System.Drawing.Drawing2D;
using System.ComponentModel;
using TestadorCLPHI.App.Ui.Theme;

namespace TestadorCLPHI.App.Ui.Controls;

internal sealed class IndustrialButton : Button
{
    private Region? _ownedRegion;
    private bool _isSelected;
    private bool _hovered;
    private bool _pressed;

    internal IndustrialButton()
    {
        SetStyle(
            ControlStyles.AllPaintingInWmPaint
            | ControlStyles.OptimizedDoubleBuffer
            | ControlStyles.ResizeRedraw
            | ControlStyles.UserPaint,
            true);
    }

    [Browsable(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    internal bool UsesSinglePassBorderRenderer => true;

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
        IndustrialPalette palette = IndustrialTheme.Palette;
        SmoothingMode previousSmoothing = e.Graphics.SmoothingMode;
        PixelOffsetMode previousPixelOffset = e.Graphics.PixelOffsetMode;
        e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
        e.Graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;

        Color fill = Enabled ? BackColor : palette.Surface;
        Color text = Enabled ? ForeColor : palette.Disabled;
        float borderWidth = IndustrialButtonVisuals.ScaleBorder(DeviceDpi);
        bool showFocus = Focused && ShowFocusCues;
        bool drawBorder = FlatAppearance.BorderSize > 0 || showFocus;
        Color border = showFocus
            ? palette.Focus
            : Enabled ? FlatAppearance.BorderColor : palette.Border;
        RectangleF strokeBounds = IndustrialButtonVisuals.InsetStrokeBounds(
            ClientRectangle,
            drawBorder ? borderWidth : 0F);
        float radius = IndustrialButtonVisuals.ScaleRadius(DeviceDpi);

        using (GraphicsPath path = IndustrialControlDrawing.RoundedRectangle(strokeBounds, radius))
        {
            using SolidBrush background = new(fill);
            e.Graphics.FillPath(background, path);

            if (Enabled && (_hovered || _pressed))
            {
                Color overlay = _pressed
                    ? IndustrialButtonVisuals.PressedOverlay(palette)
                    : IndustrialButtonVisuals.HoverOverlay(palette);
                using SolidBrush overlayBrush = new(overlay);
                e.Graphics.FillPath(overlayBrush, path);
            }

            if (drawBorder)
            {
                using Pen borderPen = new(border, borderWidth)
                {
                    Alignment = PenAlignment.Center
                };
                e.Graphics.DrawPath(borderPen, path);
            }
        }

        if (IsNavigation && IsSelected)
        {
            int indicatorWidth = Math.Max(2, (int)Math.Round(3D * DeviceDpi / 96D));
            int inset = Math.Max(3, (int)Math.Round(6D * DeviceDpi / 96D));
            Rectangle indicator = new(
                0,
                inset,
                indicatorWidth,
                Math.Max(1, ClientSize.Height - (inset * 2)));
            using SolidBrush indicatorBrush = new(palette.Accent);
            e.Graphics.FillRectangle(indicatorBrush, indicator);
        }

        Rectangle textBounds = new(
            Padding.Left,
            Padding.Top,
            Math.Max(0, ClientSize.Width - Padding.Horizontal),
            Math.Max(0, ClientSize.Height - Padding.Vertical));
        TextFormatFlags flags = TextFormatFlags.SingleLine
            | TextFormatFlags.VerticalCenter
            | TextFormatFlags.EndEllipsis
            | TextFormatFlags.NoPrefix;
        flags |= TextAlign == ContentAlignment.MiddleLeft
            ? TextFormatFlags.Left
            : TextFormatFlags.HorizontalCenter;
        TextRenderer.DrawText(e.Graphics, Text, Font, textBounds, text, flags);

        e.Graphics.SmoothingMode = previousSmoothing;
        e.Graphics.PixelOffsetMode = previousPixelOffset;
    }

    protected override void OnMouseEnter(EventArgs e)
    {
        _hovered = true;
        Invalidate();
        base.OnMouseEnter(e);
    }

    protected override void OnMouseLeave(EventArgs e)
    {
        _hovered = false;
        _pressed = false;
        Invalidate();
        base.OnMouseLeave(e);
    }

    protected override void OnMouseDown(MouseEventArgs e)
    {
        if (e.Button == MouseButtons.Left && Enabled)
        {
            _pressed = true;
            Invalidate();
        }

        base.OnMouseDown(e);
    }

    protected override void OnMouseUp(MouseEventArgs e)
    {
        _pressed = false;
        Invalidate();
        base.OnMouseUp(e);
    }

    protected override void OnKeyDown(KeyEventArgs kevent)
    {
        if (Enabled && kevent.KeyCode is Keys.Space or Keys.Enter)
        {
            _pressed = true;
            Invalidate();
        }

        base.OnKeyDown(kevent);
    }

    protected override void OnKeyUp(KeyEventArgs kevent)
    {
        if (_pressed && kevent.KeyCode is Keys.Space or Keys.Enter)
        {
            _pressed = false;
            Invalidate();
        }

        base.OnKeyUp(kevent);
    }

    protected override void OnGotFocus(EventArgs e)
    {
        base.OnGotFocus(e);
        Invalidate();
    }

    protected override void OnLostFocus(EventArgs e)
    {
        _pressed = false;
        base.OnLostFocus(e);
        Invalidate();
    }

    protected override void OnEnabledChanged(EventArgs e)
    {
        if (!Enabled)
        {
            _hovered = false;
            _pressed = false;
        }

        base.OnEnabledChanged(e);
        Invalidate();
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

        int radius = Math.Max(1, (int)Math.Round(IndustrialButtonVisuals.CornerRadius * DeviceDpi / 96D));
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
    private readonly bool _flatSection;

    internal IndustrialGroupBox(bool flatSection = false)
    {
        _flatSection = flatSection;
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

    [Browsable(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    internal bool IsFlatSection => _flatSection;

    protected override void OnPaint(PaintEventArgs e)
    {
        IndustrialPalette palette = IndustrialTheme.Palette;
        e.Graphics.Clear(palette.SurfaceElevated);
        if (!_flatSection)
        {
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
        }

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
