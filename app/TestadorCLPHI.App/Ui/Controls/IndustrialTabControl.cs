using TestadorCLPHI.App.Ui.Theme;

namespace TestadorCLPHI.App.Ui.Controls;

internal sealed class IndustrialTabControl : TabControl
{
    private const int WmPaint = 0x000F;
    private const int WmPrint = 0x0317;
    private const int WmPrintClient = 0x0318;

    internal IndustrialTabControl()
    {
        DrawMode = TabDrawMode.OwnerDrawFixed;
        SizeMode = TabSizeMode.Fixed;
        Font = IndustrialTypography.Button();
        ItemSize = new Size(132, IndustrialSpacing.TabHeight);
        Padding = new Point(IndustrialSpacing.Md, IndustrialSpacing.Xs);
    }

    internal Color HeaderRemainderColor => IndustrialTheme.Palette.Background;

    internal void ApplyTheme()
    {
        BackColor = IndustrialTheme.Palette.Background;
        ForeColor = IndustrialTheme.Palette.TextPrimary;
        foreach (TabPage page in TabPages)
        {
            page.BackColor = IndustrialTheme.Palette.SurfaceElevated;
            page.ForeColor = IndustrialTheme.Palette.TextPrimary;
        }

        Invalidate();
    }

    protected override void OnCreateControl()
    {
        base.OnCreateControl();
        ApplyTheme();
    }

    protected override void OnDrawItem(DrawItemEventArgs e)
    {
        DrawTabItem(e.Graphics, e.Index, e.Bounds, drawFocus: true);
    }

    protected override void OnResize(EventArgs e)
    {
        base.OnResize(e);
        Invalidate();
    }

    protected override void WndProc(ref Message message)
    {
        base.WndProc(ref message);
        if (message.Msg == WmPaint && IsHandleCreated && TabCount > 0)
        {
            using Graphics graphics = Graphics.FromHwnd(Handle);
            PaintNativeChromeRemainder(graphics);
        }
        else if ((message.Msg == WmPrint || message.Msg == WmPrintClient)
                 && message.WParam != IntPtr.Zero
                 && TabCount > 0)
        {
            using Graphics graphics = Graphics.FromHdc(message.WParam);
            PaintNativeChromeRemainder(graphics);
        }
    }

    private void PaintNativeChromeRemainder(Graphics graphics)
    {
        Rectangle display = DisplayRectangle;
        Rectangle lastTab = GetTabRect(TabCount - 1);
        using SolidBrush brush = new(HeaderRemainderColor);

        int headerBottom = Math.Max(display.Top, lastTab.Bottom);
        graphics.FillRectangle(brush, 0, 0, ClientSize.Width, headerBottom);
        for (int index = 0; index < TabCount; index++)
        {
            DrawTabItem(graphics, index, GetTabRect(index), drawFocus: false);
        }

        if (display.Left > 0)
        {
            graphics.FillRectangle(brush, 0, display.Top, display.Left, display.Height);
        }

        if (display.Right < ClientSize.Width)
        {
            graphics.FillRectangle(
                brush,
                display.Right,
                display.Top,
                ClientSize.Width - display.Right,
                display.Height);
        }

        if (display.Bottom < ClientSize.Height)
        {
            graphics.FillRectangle(
                brush,
                0,
                display.Bottom,
                ClientSize.Width,
                ClientSize.Height - display.Bottom);
        }
    }

    private void DrawTabItem(Graphics graphics, int index, Rectangle bounds, bool drawFocus)
    {
        IndustrialPalette palette = IndustrialTheme.Palette;
        bool selected = index == SelectedIndex;
        using SolidBrush background = new(selected ? palette.SurfaceElevated : palette.Surface);
        graphics.FillRectangle(background, bounds);
        TextRenderer.DrawText(
            graphics,
            TabPages[index].Text,
            Font,
            bounds,
            selected ? palette.TextPrimary : palette.TextSecondary,
            TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis);
        if (selected)
        {
            using SolidBrush accent = new(palette.Accent);
            graphics.FillRectangle(accent, bounds.Left, bounds.Bottom - 2, bounds.Width, 2);
        }

        if (drawFocus && selected && Focused)
        {
            ControlPaint.DrawFocusRectangle(graphics, bounds);
        }
    }
}
