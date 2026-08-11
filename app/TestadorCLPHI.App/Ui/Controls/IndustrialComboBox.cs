using TestadorCLPHI.App.Ui.Theme;

namespace TestadorCLPHI.App.Ui.Controls;

internal sealed class IndustrialComboBox : ComboBox
{
    private const int WmPaint = 0x000F;
    private const int WmPrint = 0x0317;
    private const int WmPrintClient = 0x0318;

    internal IndustrialComboBox()
    {
        DrawMode = DrawMode.OwnerDrawFixed;
        DropDownStyle = ComboBoxStyle.DropDownList;
        FlatStyle = FlatStyle.Flat;
        IntegralHeight = false;
        ItemHeight = 28;
        Height = 32;
        Font = IndustrialTypography.Body();
    }

    internal void ApplyTheme()
    {
        IndustrialPalette palette = IndustrialTheme.Palette;
        BackColor = Enabled ? palette.Field : palette.SurfaceInteractive;
        ForeColor = Enabled ? palette.TextPrimary : palette.Disabled;
        Invalidate();
    }

    protected override void OnCreateControl()
    {
        base.OnCreateControl();
        ApplyTheme();
    }

    protected override void OnEnabledChanged(EventArgs e)
    {
        base.OnEnabledChanged(e);
        ApplyTheme();
    }

    protected override void OnDrawItem(DrawItemEventArgs e)
    {
        IndustrialPalette palette = IndustrialTheme.Palette;
        bool selected = (e.State & DrawItemState.Selected) == DrawItemState.Selected;
        Color background = selected ? palette.SelectedSurface : palette.Field;
        Color foreground = selected ? palette.SelectedText : palette.TextPrimary;
        using SolidBrush brush = new(background);
        e.Graphics.FillRectangle(brush, e.Bounds);
        Rectangle textBounds = Rectangle.Inflate(e.Bounds, -IndustrialSpacing.Sm, 0);
        if (e.Index >= 0)
        {
            TextRenderer.DrawText(
                e.Graphics,
                GetItemText(Items[e.Index]),
                Font,
                textBounds,
                foreground,
                TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis);
        }

        if ((e.State & DrawItemState.Focus) == DrawItemState.Focus)
        {
            e.DrawFocusRectangle();
        }
    }

    protected override void WndProc(ref Message message)
    {
        base.WndProc(ref message);
        if (message.Msg == WmPaint && IsHandleCreated)
        {
            using Graphics graphics = Graphics.FromHwnd(Handle);
            PaintField(graphics);
        }
        else if ((message.Msg == WmPrint || message.Msg == WmPrintClient)
                 && message.WParam != IntPtr.Zero)
        {
            using Graphics graphics = Graphics.FromHdc(message.WParam);
            PaintField(graphics);
        }
    }

    private void PaintField(Graphics graphics)
    {
        IndustrialPalette palette = IndustrialTheme.Palette;
        Color background = Enabled ? palette.Field : palette.SurfaceInteractive;
        Color foreground = Enabled ? palette.TextPrimary : palette.Disabled;
        Rectangle bounds = ClientRectangle;
        using SolidBrush backgroundBrush = new(background);
        graphics.FillRectangle(backgroundBrush, bounds);

        int arrowWidth = Math.Max(IndustrialSpacing.Xl, Height);
        Rectangle textBounds = new(
            IndustrialSpacing.Sm,
            0,
            Math.Max(0, Width - arrowWidth - IndustrialSpacing.Sm),
            Height);
        TextRenderer.DrawText(
            graphics,
            SelectedIndex >= 0 ? GetItemText(SelectedItem) : Text,
            Font,
            textBounds,
            foreground,
            TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis);

        int centerX = Width - (arrowWidth / 2);
        int centerY = Height / 2;
        using Pen arrowPen = new(foreground, IndustrialSpacing.BorderWidth + 1F);
        graphics.DrawLine(arrowPen, centerX - 4, centerY - 2, centerX, centerY + 2);
        graphics.DrawLine(arrowPen, centerX, centerY + 2, centerX + 4, centerY - 2);
        using Pen borderPen = new(Focused ? palette.Focus : palette.BorderStrong, IndustrialSpacing.BorderWidth);
        graphics.DrawRectangle(borderPen, 0, 0, Math.Max(0, Width - 1), Math.Max(0, Height - 1));
    }
}
