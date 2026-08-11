using System.ComponentModel;
using System.Drawing.Drawing2D;
using TestadorCLPHI.App.Ui.Theme;

namespace TestadorCLPHI.App.Ui.Controls;

public sealed class AssetPushButtonControl : Control
{
    private bool _isActive;
    private bool _isPressed;
    private bool _isHovered;
    private string _labelText = "DO";
    private Color _buttonColor;
    private string? _buttonImagePath;

    public AssetPushButtonControl()
    {
        SetStyle(
            ControlStyles.AllPaintingInWmPaint |
            ControlStyles.ApplyThemingImplicitly |
            ControlStyles.OptimizedDoubleBuffer |
            ControlStyles.ResizeRedraw |
            ControlStyles.Selectable |
            ControlStyles.UserPaint,
            true);
        _buttonColor = IndustrialTheme.Palette.Danger;
        Size = new Size(104, 108);
        MinimumSize = new Size(82, 92);
        Font = IndustrialTypography.BodyStrong();
        Cursor = Cursors.Hand;
        TabStop = true;
        AccessibleRole = AccessibleRole.PushButton;
        UpdateAccessibility();
    }

    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public bool IsActive
    {
        get => _isActive;
        set
        {
            if (_isActive == value)
            {
                return;
            }

            _isActive = value;
            UpdateAccessibility();
            Invalidate();
        }
    }

    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public string LabelText
    {
        get => _labelText;
        set
        {
            _labelText = string.IsNullOrWhiteSpace(value) ? "DO" : value.Trim();
            UpdateAccessibility();
            Invalidate();
        }
    }

    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public Color ButtonColor
    {
        get => _buttonColor;
        set
        {
            _buttonColor = value;
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
        _isHovered = true;
        Invalidate();
    }

    protected override void OnMouseLeave(EventArgs e)
    {
        base.OnMouseLeave(e);
        _isHovered = false;
        _isPressed = false;
        Invalidate();
    }

    protected override void OnMouseDown(MouseEventArgs e)
    {
        base.OnMouseDown(e);
        if (Enabled && e.Button == MouseButtons.Left)
        {
            Focus();
            _isPressed = true;
            Invalidate();
        }
    }

    protected override void OnMouseUp(MouseEventArgs e)
    {
        base.OnMouseUp(e);
        if (_isPressed)
        {
            _isPressed = false;
            Invalidate();
        }
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        base.OnKeyDown(e);
        if (Enabled && e.KeyCode is Keys.Space or Keys.Enter)
        {
            _isPressed = true;
            e.Handled = true;
            Invalidate();
        }
    }

    protected override void OnKeyUp(KeyEventArgs e)
    {
        base.OnKeyUp(e);
        if (_isPressed && e.KeyCode is Keys.Space or Keys.Enter)
        {
            _isPressed = false;
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
        _isPressed = false;
        Invalidate();
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);
        Graphics graphics = e.Graphics;
        graphics.SmoothingMode = SmoothingMode.AntiAlias;
        graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;
        graphics.Clear(IndustrialTheme.Palette.Background);
        Rectangle card = ClientRectangle;
        card.Inflate(-1, -1);
        IndustrialControlDrawing.DrawCard(graphics, card, _isHovered, _isPressed, Enabled);
        Rectangle button = IndustrialControlDrawing.CenteredSquare(
            ClientRectangle,
            _isPressed ? 15 : 13,
            38,
            54);
        Image? image = IndustrialAssetCache.Get(_buttonImagePath);
        if (image is null)
        {
            IndustrialControlDrawing.DrawPushButton(
                graphics,
                button,
                _buttonColor,
                _isActive,
                _isHovered,
                _isPressed,
                Enabled);
        }
        else
        {
            graphics.DrawImage(image, button);
        }

        Rectangle label = new(6, Height - 34, Math.Max(1, Width - 12), 24);
        TextRenderer.DrawText(
            graphics,
            $"{_labelText}{(_isActive ? " • ATIVO" : string.Empty)}",
            Font,
            label,
            Enabled ? IndustrialTheme.Palette.TextPrimary : IndustrialTheme.Palette.Disabled,
            TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis);
        IndustrialControlDrawing.DrawFocus(graphics, card, Focused);
    }

    private void UpdateAccessibility()
    {
        AccessibleName = $"Botoeira {_labelText}";
        AccessibleDescription = $"Comando {_labelText}; estado {(_isActive ? "ativo" : "inativo")}";
    }
}
