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
        Font = IndustrialTypography.BodyStrong();
        ItemSize = new Size(150, IndustrialSpacing.TabHeight);
        Padding = new Point(IndustrialSpacing.Md, IndustrialSpacing.Xs);
    }

    internal Color HeaderRemainderColor => IndustrialTheme.Palette.Background;

    internal void ApplyTheme()
    {
        BackColor = IndustrialTheme.Palette.Background;
        ForeColor = IndustrialTheme.Palette.TextPrimary;
        Invalidate();
    }

    protected override void OnCreateControl()
    {
        base.OnCreateControl();
        ApplyTheme();
    }

    protected override void OnDrawItem(DrawItemEventArgs e)
    {
        IndustrialPalette palette = IndustrialTheme.Palette;
        bool selected = e.Index == SelectedIndex;
        using SolidBrush background = new(selected ? palette.SelectedSurface : palette.Surface);
        e.Graphics.FillRectangle(background, e.Bounds);
        TextRenderer.DrawText(
            e.Graphics,
            TabPages[e.Index].Text,
            Font,
            e.Bounds,
            selected ? palette.SelectedText : palette.TextSecondary,
            TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis);
        if (selected)
        {
            using SolidBrush accent = new(palette.Accent);
            e.Graphics.FillRectangle(accent, e.Bounds.Left, e.Bounds.Bottom - 3, e.Bounds.Width, 3);
        }

        if (selected && Focused)
        {
            e.DrawFocusRectangle();
        }
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
        if (lastTab.Right < ClientSize.Width)
        {
            graphics.FillRectangle(
                brush,
                lastTab.Right,
                0,
                ClientSize.Width - lastTab.Right,
                headerBottom);
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
}
