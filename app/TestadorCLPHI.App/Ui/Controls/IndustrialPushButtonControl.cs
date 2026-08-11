using System.ComponentModel;
using System.Drawing.Drawing2D;
using TestadorCLPHI.App.Ui.Theme;

namespace TestadorCLPHI.App.Ui.Controls;

public sealed class IndustrialPushButtonControl : Control
{
    private bool _active;
    private bool _hovered;
    private bool _pressed;
    private string _title = "DO00";
    private string _description = "DO00 -> DI00 + DI04";
    private string? _buttonImagePath;

    public IndustrialPushButtonControl()
    {
        SetStyle(
            ControlStyles.AllPaintingInWmPaint |
            ControlStyles.ApplyThemingImplicitly |
            ControlStyles.OptimizedDoubleBuffer |
            ControlStyles.ResizeRedraw |
            ControlStyles.Selectable |
            ControlStyles.UserPaint,
            true);
        Size = new Size(148, 108);
        MinimumSize = new Size(132, 92);
        Font = IndustrialTypography.BodyStrong();
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
            _title = string.IsNullOrWhiteSpace(value) ? "DO00" : value.Trim();
            UpdateAccessibility();
            Invalidate();
        }
    }

    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public string Description
    {
        get => _description;
        set
        {
            _description = value?.Trim() ?? string.Empty;
            UpdateAccessibility();
            Invalidate();
        }
    }

    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public bool IsActive
    {
        get => _active;
        set
        {
            if (_active == value)
            {
                return;
            }

            _active = value;
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

        Rectangle title = new(5, 6, Math.Max(1, Width - 10), 20);
        TextRenderer.DrawText(
            graphics,
            $"{_title}{(_active ? " • ATIVO" : string.Empty)}",
            Font,
            title,
            Enabled ? IndustrialTheme.Palette.TextPrimary : IndustrialTheme.Palette.Disabled,
            TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis);
        Rectangle button = IndustrialControlDrawing.CenteredSquare(
            ClientRectangle,
            _pressed ? 31 : 29,
            28,
            50);
        Image? image = IndustrialAssetCache.Get(_buttonImagePath);
        if (image is null)
        {
            IndustrialControlDrawing.DrawPushButton(
                graphics,
                button,
                IndustrialTheme.Palette.Accent,
                _active,
                _hovered,
                _pressed,
                Enabled);
        }
        else
        {
            graphics.DrawImage(image, button);
        }

        Rectangle description = new(6, Height - 24, Math.Max(1, Width - 12), 18);
        TextRenderer.DrawText(
            graphics,
            _description,
            Font,
            description,
            IndustrialTheme.Palette.TextSecondary,
            TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis);
        IndustrialControlDrawing.DrawFocus(graphics, card, Focused);
    }

    private void UpdateAccessibility()
    {
        AccessibleName = $"Botoeira {_title}";
        AccessibleDescription = $"{_description}; estado {(_active ? "ativo" : "inativo")}";
    }
}
