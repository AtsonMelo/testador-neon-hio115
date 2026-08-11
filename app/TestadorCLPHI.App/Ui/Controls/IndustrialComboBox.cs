using TestadorCLPHI.App.Ui.Theme;

namespace TestadorCLPHI.App.Ui.Controls;

internal sealed class IndustrialComboBox : ComboBox
{
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
        if (e.Index < 0)
        {
            return;
        }

        IndustrialPalette palette = IndustrialTheme.Palette;
        bool selected = (e.State & DrawItemState.Selected) == DrawItemState.Selected;
        Color background = selected ? palette.SelectedSurface : palette.Field;
        Color foreground = selected ? palette.SelectedText : palette.TextPrimary;
        using SolidBrush brush = new(background);
        e.Graphics.FillRectangle(brush, e.Bounds);
        Rectangle textBounds = Rectangle.Inflate(e.Bounds, -IndustrialSpacing.Sm, 0);
        TextRenderer.DrawText(
            e.Graphics,
            GetItemText(Items[e.Index]),
            Font,
            textBounds,
            foreground,
            TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis);
        if ((e.State & DrawItemState.Focus) == DrawItemState.Focus)
        {
            e.DrawFocusRectangle();
        }
    }
}
