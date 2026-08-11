using System.Runtime.InteropServices;
using TestadorCLPHI.App.Ui.Theme;

namespace TestadorCLPHI.App.Ui.Controls;

internal sealed class IndustrialScrollPanel : Panel
{
    private readonly IndustrialScrollChrome? _scrollChrome;

    internal IndustrialScrollPanel()
    {
        AutoScroll = true;
        TabStop = true;
        AccessibleRole = AccessibleRole.Pane;
        Padding = new Padding(0, 0, IndustrialScrollChrome.ReservedWidth, 0);
        SetStyle(ControlStyles.OptimizedDoubleBuffer | ControlStyles.AllPaintingInWmPaint, true);
        _scrollChrome = new IndustrialScrollChrome(this);
    }

    internal bool UsesSlimThemedScrollbar => true;
    internal bool HasHorizontalScroll => _scrollChrome?.IsHorizontalScrollRequired == true;
    internal bool IsVerticalScrollRequired => _scrollChrome?.IsVerticalScrollRequired == true;
    internal static int ScrollbarThumbWidth => IndustrialScrollChrome.ThumbWidth;

    protected override void OnHandleCreated(EventArgs e)
    {
        base.OnHandleCreated(e);
        _scrollChrome?.HideNativeScrollbars();
    }

    protected override void OnLayout(LayoutEventArgs levent)
    {
        base.OnLayout(levent);
        _scrollChrome?.HideNativeScrollbars();
        Invalidate();
    }

    protected override void OnScroll(ScrollEventArgs se)
    {
        base.OnScroll(se);
        _scrollChrome?.HideNativeScrollbars();
        Invalidate();
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);
        _scrollChrome?.Draw(e.Graphics);
    }

    protected override void OnMouseDown(MouseEventArgs e)
    {
        base.OnMouseDown(e);
        _scrollChrome?.MouseDown(e);
    }

    protected override void OnMouseMove(MouseEventArgs e)
    {
        base.OnMouseMove(e);
        _scrollChrome?.MouseMove(e);
    }

    protected override void OnMouseUp(MouseEventArgs e)
    {
        _scrollChrome?.MouseUp();
        base.OnMouseUp(e);
    }

    protected override void OnMouseWheel(MouseEventArgs e)
    {
        base.OnMouseWheel(e);
        _scrollChrome?.HideNativeScrollbars();
        Invalidate();
    }

    protected override bool IsInputKey(Keys keyData) => keyData is Keys.Up or Keys.Down
        or Keys.PageUp or Keys.PageDown or Keys.Home or Keys.End
        || base.IsInputKey(keyData);

    protected override void OnKeyDown(KeyEventArgs e)
    {
        if (_scrollChrome?.KeyDown(e.KeyCode) == true)
        {
            e.Handled = true;
            return;
        }

        base.OnKeyDown(e);
    }
}

internal sealed class IndustrialFlowLayoutPanel : FlowLayoutPanel
{
    private readonly IndustrialScrollChrome? _scrollChrome;

    internal IndustrialFlowLayoutPanel()
    {
        AutoScroll = true;
        TabStop = true;
        AccessibleRole = AccessibleRole.Pane;
        SetStyle(ControlStyles.OptimizedDoubleBuffer | ControlStyles.AllPaintingInWmPaint, true);
        _scrollChrome = new IndustrialScrollChrome(this);
    }

    internal bool UsesSlimThemedScrollbar => true;
    internal bool HasHorizontalScroll => _scrollChrome?.IsHorizontalScrollRequired == true;
    internal bool IsVerticalScrollRequired => _scrollChrome?.IsVerticalScrollRequired == true;
    internal static int ScrollbarThumbWidth => IndustrialScrollChrome.ThumbWidth;

    protected override void OnHandleCreated(EventArgs e)
    {
        base.OnHandleCreated(e);
        _scrollChrome?.HideNativeScrollbars();
    }

    protected override void OnLayout(LayoutEventArgs levent)
    {
        base.OnLayout(levent);
        _scrollChrome?.HideNativeScrollbars();
        Invalidate();
    }

    protected override void OnScroll(ScrollEventArgs se)
    {
        base.OnScroll(se);
        _scrollChrome?.HideNativeScrollbars();
        Invalidate();
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);
        _scrollChrome?.Draw(e.Graphics);
    }

    protected override void OnMouseDown(MouseEventArgs e)
    {
        base.OnMouseDown(e);
        _scrollChrome?.MouseDown(e);
    }

    protected override void OnMouseMove(MouseEventArgs e)
    {
        base.OnMouseMove(e);
        _scrollChrome?.MouseMove(e);
    }

    protected override void OnMouseUp(MouseEventArgs e)
    {
        _scrollChrome?.MouseUp();
        base.OnMouseUp(e);
    }

    protected override void OnMouseWheel(MouseEventArgs e)
    {
        base.OnMouseWheel(e);
        _scrollChrome?.HideNativeScrollbars();
        Invalidate();
    }

    protected override bool IsInputKey(Keys keyData) => keyData is Keys.Up or Keys.Down
        or Keys.PageUp or Keys.PageDown or Keys.Home or Keys.End
        || base.IsInputKey(keyData);

    protected override void OnKeyDown(KeyEventArgs e)
    {
        if (_scrollChrome?.KeyDown(e.KeyCode) == true)
        {
            e.Handled = true;
            return;
        }

        base.OnKeyDown(e);
    }
}

internal sealed class IndustrialScrollChrome
{
    internal const int ReservedWidth = 10;
    internal const int ThumbWidth = 4;
    internal const int HoverThumbWidth = 7;
    private const int MinimumThumbHeight = 28;
    private const int ScrollBarBoth = 3;
    private readonly ScrollableControl _owner;
    private bool _dragging;
    private bool _hovering;
    private int _dragOffset;

    internal IndustrialScrollChrome(ScrollableControl owner)
    {
        _owner = owner;
    }

    internal bool IsVerticalScrollRequired => MaximumOffset > 0;
    internal bool IsHorizontalScrollRequired
    {
        get
        {
            int offset = Math.Max(0, -_owner.AutoScrollPosition.X);
            int rightEdge = Math.Max(0, _owner.ClientSize.Width - ReservedWidth);
            return _owner.Controls.Cast<Control>()
                .Where(control => control.Visible)
                .Any(control => control.Left + offset < _owner.Padding.Left
                    || control.Right + offset > rightEdge);
        }
    }

    internal void HideNativeScrollbars()
    {
        if (_owner.IsHandleCreated)
        {
            ShowScrollBar(_owner.Handle, ScrollBarBoth, show: false);
        }
    }

    internal void Draw(Graphics graphics)
    {
        Rectangle thumb = GetThumbBounds();
        if (thumb.IsEmpty)
        {
            return;
        }

        using SolidBrush brush = new(
            _hovering || _dragging
                ? IndustrialTheme.Palette.ScrollThumbHover
                : IndustrialTheme.Palette.ScrollThumb);
        graphics.FillRectangle(brush, thumb);
    }

    internal void MouseDown(MouseEventArgs e)
    {
        if (e.Button != MouseButtons.Left)
        {
            return;
        }

        Rectangle thumb = GetThumbBounds();
        if (GetThumbHitBounds(thumb).Contains(e.Location))
        {
            _dragging = true;
            _dragOffset = e.Y - thumb.Top;
            _owner.Capture = true;
        }
        else if (GetHitTrackBounds().Contains(e.Location))
        {
            SetOffset(CurrentOffset + (e.Y < thumb.Top ? -_owner.ClientSize.Height : _owner.ClientSize.Height));
        }
    }

    internal void MouseMove(MouseEventArgs e)
    {
        Rectangle track = GetTrackBounds();
        Rectangle hitTrack = GetHitTrackBounds();
        bool hovering = hitTrack.Contains(e.Location);
        if (_hovering != hovering)
        {
            _hovering = hovering;
            _owner.Invalidate(hitTrack);
            track = GetTrackBounds();
        }

        if (!_dragging)
        {
            return;
        }

        Rectangle thumb = GetThumbBounds();
        int availableTrack = Math.Max(1, track.Height - thumb.Height);
        int thumbTop = Math.Clamp(e.Y - _dragOffset, track.Top, track.Bottom - thumb.Height);
        int offset = (int)Math.Round((thumbTop - track.Top) / (double)availableTrack * MaximumOffset);
        SetOffset(offset);
    }

    internal void MouseUp()
    {
        _dragging = false;
        _owner.Capture = false;
        _owner.Invalidate(GetHitTrackBounds());
    }

    internal bool KeyDown(Keys key)
    {
        int line = Math.Max(24, _owner.Font.Height * 2);
        int target = key switch
        {
            Keys.Up => CurrentOffset - line,
            Keys.Down => CurrentOffset + line,
            Keys.PageUp => CurrentOffset - _owner.ClientSize.Height,
            Keys.PageDown => CurrentOffset + _owner.ClientSize.Height,
            Keys.Home => 0,
            Keys.End => MaximumOffset,
            _ => CurrentOffset
        };
        if (target == CurrentOffset && key is not Keys.Home and not Keys.End)
        {
            return false;
        }

        SetOffset(target);
        return true;
    }

    private int CurrentOffset => Math.Max(0, -_owner.AutoScrollPosition.Y);

    private int MaximumOffset => Math.Max(0, GetContentHeight() - _owner.ClientSize.Height);

    private int GetContentHeight()
    {
        int offset = CurrentOffset;
        int controlsBottom = _owner.Controls.Cast<Control>()
            .Where(control => control.Visible)
            .Select(control => control.Bottom + offset + control.Margin.Bottom)
            .DefaultIfEmpty(0)
            .Max();
        return Math.Max(_owner.DisplayRectangle.Height, controlsBottom + _owner.Padding.Bottom);
    }

    private int CurrentThumbWidth => _hovering || _dragging ? HoverThumbWidth : ThumbWidth;

    private Rectangle GetHitTrackBounds() => new(
        Math.Max(0, _owner.ClientSize.Width - ReservedWidth),
        IndustrialSpacing.Xs,
        ReservedWidth,
        Math.Max(0, _owner.ClientSize.Height - (IndustrialSpacing.Xs * 2)));

    private Rectangle GetTrackBounds() => new(
        Math.Max(0, _owner.ClientSize.Width - ReservedWidth + ((ReservedWidth - CurrentThumbWidth) / 2)),
        IndustrialSpacing.Xs,
        CurrentThumbWidth,
        Math.Max(0, _owner.ClientSize.Height - (IndustrialSpacing.Xs * 2)));

    private Rectangle GetThumbHitBounds(Rectangle thumb)
    {
        Rectangle hitTrack = GetHitTrackBounds();
        return thumb.IsEmpty
            ? Rectangle.Empty
            : new Rectangle(hitTrack.Left, thumb.Top, hitTrack.Width, thumb.Height);
    }

    private Rectangle GetThumbBounds()
    {
        int maximum = MaximumOffset;
        Rectangle track = GetTrackBounds();
        if (maximum <= 0 || track.Height <= 0)
        {
            return Rectangle.Empty;
        }

        int contentHeight = GetContentHeight();
        int height = Math.Clamp(
            (int)Math.Round(track.Height * (_owner.ClientSize.Height / (double)contentHeight)),
            MinimumThumbHeight,
            track.Height);
        int travel = Math.Max(0, track.Height - height);
        int top = track.Top + (int)Math.Round(CurrentOffset / (double)maximum * travel);
        return new Rectangle(track.Left, top, track.Width, height);
    }

    private void SetOffset(int value)
    {
        _owner.AutoScrollPosition = new Point(0, Math.Clamp(value, 0, MaximumOffset));
        HideNativeScrollbars();
        _owner.Invalidate();
    }

    [DllImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool ShowScrollBar(IntPtr window, int bar, bool show);
}
