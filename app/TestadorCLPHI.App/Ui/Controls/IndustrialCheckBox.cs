using System.ComponentModel;
using System.Drawing.Drawing2D;
using TestadorCLPHI.App.Ui.Theme;

namespace TestadorCLPHI.App.Ui.Controls;

internal sealed class IndustrialCheckBox : CheckBox
{
    private bool _isHovered;
    private bool _isPressed;

    internal IndustrialCheckBox()
    {
        SetStyle(
            ControlStyles.AllPaintingInWmPaint
            | ControlStyles.OptimizedDoubleBuffer
            | ControlStyles.ResizeRedraw
            | ControlStyles.UserPaint
            | ControlStyles.Selectable,
            true);
        AutoSize = true;
        MinimumSize = new Size(0, IndustrialSpacing.InteractiveHeight);
        Font = IndustrialTypography.Body();
        TabStop = true;
        AccessibleRole = AccessibleRole.CheckButton;
    }

    [Browsable(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    internal bool UsesIndustrialChrome => true;

    internal void ApplyTheme()
    {
        Font = IndustrialTypography.Body();
        ForeColor = Enabled
            ? IndustrialTheme.Palette.TextPrimary
            : IndustrialTheme.Palette.Disabled;
        Invalidate();
    }

    public override Size GetPreferredSize(Size proposedSize)
    {
        int box = ScaleLogical(16);
        int gap = ScaleLogical(8);
        Size text = TextRenderer.MeasureText(
            Text,
            Font,
            new Size(int.MaxValue, int.MaxValue),
            TextFormatFlags.NoPadding | TextFormatFlags.SingleLine);
        return new Size(
            Padding.Horizontal + box + gap + text.Width,
            Math.Max(IndustrialSpacing.InteractiveHeight, Padding.Vertical + Math.Max(box, text.Height)));
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        IndustrialPalette palette = IndustrialTheme.Palette;
        Color background = Parent?.BackColor ?? palette.SurfaceElevated;
        e.Graphics.Clear(background);

        int boxSize = ScaleLogical(16);
        int left = Padding.Left;
        int top = Math.Max(Padding.Top, (ClientSize.Height - boxSize) / 2);
        Rectangle box = new(left, top, boxSize, boxSize);
        Color fill = ResolveFill(palette);
        Color border = Focused && ShowFocusCues ? palette.Focus : palette.BorderStrong;

        SmoothingMode previous = e.Graphics.SmoothingMode;
        e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
        using (GraphicsPath path = IndustrialControlDrawing.RoundedRectangle(box, ScaleLogical(3)))
        using (SolidBrush fillBrush = new(fill))
        using (Pen borderPen = new(border, Math.Max(1F, IndustrialSpacing.BorderWidth)))
        {
            e.Graphics.FillPath(fillBrush, path);
            e.Graphics.DrawPath(borderPen, path);
        }

        if (Checked)
        {
            Point[] check =
            [
                new(box.Left + ScaleLogical(4), box.Top + ScaleLogical(8)),
                new(box.Left + ScaleLogical(7), box.Top + ScaleLogical(11)),
                new(box.Left + ScaleLogical(12), box.Top + ScaleLogical(5))
            ];
            using Pen checkPen = new(palette.AccentText, Math.Max(1.5F, ScaleLogical(2)))
            {
                StartCap = LineCap.Round,
                EndCap = LineCap.Round,
                LineJoin = LineJoin.Round
            };
            e.Graphics.DrawLines(checkPen, check);
        }

        e.Graphics.SmoothingMode = previous;
        Rectangle textBounds = new(
            box.Right + ScaleLogical(8),
            0,
            Math.Max(0, ClientSize.Width - box.Right - ScaleLogical(8) - Padding.Right),
            ClientSize.Height);
        TextRenderer.DrawText(
            e.Graphics,
            Text,
            Font,
            textBounds,
            Enabled ? palette.TextPrimary : palette.Disabled,
            TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis
            | TextFormatFlags.NoPadding | TextFormatFlags.SingleLine);

        if (Focused && ShowFocusCues && textBounds.Width > 0)
        {
            Rectangle focus = Rectangle.Inflate(textBounds, -1, -ScaleLogical(3));
            ControlPaint.DrawFocusRectangle(e.Graphics, focus, palette.Focus, background);
        }
    }

    protected override void OnMouseEnter(EventArgs eventargs)
    {
        _isHovered = true;
        Invalidate();
        base.OnMouseEnter(eventargs);
    }

    protected override void OnMouseLeave(EventArgs eventargs)
    {
        _isHovered = false;
        _isPressed = false;
        Invalidate();
        base.OnMouseLeave(eventargs);
    }

    protected override void OnMouseDown(MouseEventArgs mevent)
    {
        _isPressed = mevent.Button == MouseButtons.Left;
        Invalidate();
        base.OnMouseDown(mevent);
    }

    protected override void OnMouseUp(MouseEventArgs mevent)
    {
        _isPressed = false;
        Invalidate();
        base.OnMouseUp(mevent);
    }

    protected override void OnCheckedChanged(EventArgs e)
    {
        base.OnCheckedChanged(e);
        Invalidate();
    }

    protected override void OnEnabledChanged(EventArgs e)
    {
        base.OnEnabledChanged(e);
        ApplyTheme();
    }

    protected override void OnGotFocus(EventArgs e)
    {
        base.OnGotFocus(e);
        Invalidate();
    }

    protected override void OnLostFocus(EventArgs e)
    {
        base.OnLostFocus(e);
        Invalidate();
    }

    private Color ResolveFill(IndustrialPalette palette)
    {
        if (!Enabled)
        {
            return palette.SurfaceInteractive;
        }

        if (Checked)
        {
            return _isPressed ? palette.AccentPressed : _isHovered ? palette.AccentHover : palette.Accent;
        }

        return _isHovered ? palette.SurfaceInteractive : palette.Field;
    }

    private int ScaleLogical(int value) => Math.Max(1, (int)Math.Round(value * DeviceDpi / 96D));
}
