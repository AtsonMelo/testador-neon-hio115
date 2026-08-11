using System.ComponentModel;
using System.Drawing.Drawing2D;
using TestadorCLPHI.App.Ui.Theme;

namespace TestadorCLPHI.App.Ui.Controls;

public sealed class EmergencyStopButtonControl : Control
{
    private bool _pressed;
    private bool _hovered;
    private string _title = "PARADA DE EMERGÊNCIA";
    private string? _buttonImagePath;

    public EmergencyStopButtonControl()
    {
        SetStyle(
            ControlStyles.AllPaintingInWmPaint |
            ControlStyles.ApplyThemingImplicitly |
            ControlStyles.OptimizedDoubleBuffer |
            ControlStyles.ResizeRedraw |
            ControlStyles.Selectable |
            ControlStyles.UserPaint,
            true);
        Size = new Size(128, 112);
        MinimumSize = new Size(96, 92);
        Font = IndustrialTypography.CaptionStrong();
        Cursor = Cursors.Hand;
        TabStop = true;
        AccessibleRole = AccessibleRole.PushButton;
        UpdateAccessibility();
    }

    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public string Title
    {
        get => _title;
        set
        {
            _title = string.IsNullOrWhiteSpace(value) ? "PARADA DE EMERGÊNCIA" : value.Trim();
            UpdateAccessibility();
            Invalidate();
        }
    }

    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public string? ButtonImagePath
    {
        get => _buttonImagePath;
        set
        {
            _buttonImagePath = value;
            Invalidate();
        }
    }

    protected override bool IsInputKey(Keys keyData) =>
        keyData is Keys.Space or Keys.Enter || base.IsInputKey(keyData);

    protected override void OnMouseEnter(EventArgs e)
    {
        base.OnMouseEnter(e);
        _hovered = true;
        Invalidate();
    }

    protected override void OnMouseLeave(EventArgs e)
    {
        base.OnMouseLeave(e);
        _hovered = false;
        _pressed = false;
        Invalidate();
    }

    protected override void OnMouseDown(MouseEventArgs e)
    {
        base.OnMouseDown(e);
        if (Enabled && e.Button == MouseButtons.Left)
        {
            Focus();
            _pressed = true;
            Invalidate();
        }
    }

    protected override void OnMouseUp(MouseEventArgs e)
    {
        base.OnMouseUp(e);
        _pressed = false;
        Invalidate();
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        base.OnKeyDown(e);
        if (Enabled && e.KeyCode is Keys.Space or Keys.Enter)
        {
            _pressed = true;
            e.Handled = true;
            Invalidate();
        }
    }

    protected override void OnKeyUp(KeyEventArgs e)
    {
        base.OnKeyUp(e);
        if (_pressed && e.KeyCode is Keys.Space or Keys.Enter)
        {
            _pressed = false;
            e.Handled = true;
            Invalidate();
            OnClick(EventArgs.Empty);
        }
    }

    protected override void OnGotFocus(EventArgs e)
    {
        base.OnGotFocus(e);
        Invalidate();
    }

    protected override void OnLostFocus(EventArgs e)
    {
        base.OnLostFocus(e);
        _pressed = false;
        Invalidate();
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);
        Graphics graphics = e.Graphics;
        graphics.SmoothingMode = SmoothingMode.AntiAlias;
        graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
        graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;
        graphics.Clear(IndustrialTheme.Palette.Background);
        Rectangle card = ClientRectangle;
        card.Inflate(-1, -1);
        IndustrialControlDrawing.DrawCard(graphics, card, _hovered, _pressed, Enabled);
        Rectangle title = new(5, 4, Math.Max(1, Width - 10), 30);
        TextRenderer.DrawText(
            graphics,
            _title,
            Font,
            title,
            Enabled ? IndustrialTheme.Palette.Danger : IndustrialTheme.Palette.Disabled,
            TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.WordBreak);
        Rectangle button = IndustrialControlDrawing.CenteredSquare(
            ClientRectangle,
            _pressed ? 39 : 37,
            16,
            56);
        Image? image = IndustrialAssetCache.Get(_buttonImagePath);
        if (image is null)
        {
            IndustrialControlDrawing.DrawEmergencyStop(
                graphics,
                button,
                _hovered,
                _pressed,
                Enabled);
        }
        else
        {
            graphics.DrawImage(image, button);
        }

        IndustrialControlDrawing.DrawFocus(graphics, card, Focused);
    }

    private void UpdateAccessibility()
    {
        AccessibleName = _title;
        AccessibleDescription = "Botoeira de parada de emergência; ação crítica";
    }
}
