using System.ComponentModel;
using System.Drawing.Drawing2D;
using TestadorCLPHI.App.Ui.Theme;

namespace TestadorCLPHI.App.Ui.Controls;

public sealed class AssetLedIndicatorControl : Control
{
    private bool _isOn;
    private string _labelText = "DI";
    private string? _onImagePath;
    private string? _offImagePath;

    public AssetLedIndicatorControl()
    {
        SetStyle(
            ControlStyles.AllPaintingInWmPaint |
            ControlStyles.ApplyThemingImplicitly |
            ControlStyles.OptimizedDoubleBuffer |
            ControlStyles.ResizeRedraw |
            ControlStyles.UserPaint,
            true);
        Size = new Size(92, 92);
        MinimumSize = new Size(72, 82);
        Font = IndustrialTypography.BodyStrong();
        TabStop = false;
        AccessibleRole = AccessibleRole.Graphic;
        UpdateAccessibility();
    }

    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public bool IsOn
    {
        get => _isOn;
        set
        {
            if (_isOn == value)
            {
                return;
            }

            _isOn = value;
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
            _labelText = string.IsNullOrWhiteSpace(value) ? "DI" : value.Trim();
            UpdateAccessibility();
            Invalidate();
        }
    }

    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public string? OnImagePath
    {
        get => _onImagePath;
        set
        {
            _onImagePath = value;
            Invalidate();
        }
    }

    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public string? OffImagePath
    {
        get => _offImagePath;
        set
        {
            _offImagePath = value;
            Invalidate();
        }
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
        IndustrialControlDrawing.DrawCard(
            graphics,
            card,
            hovered: false,
            pressed: false,
            enabled: Enabled);
        Rectangle led = IndustrialControlDrawing.CenteredSquare(ClientRectangle, 12, 34, 44);
        Image? image = IndustrialAssetCache.Get(_isOn ? _onImagePath : _offImagePath);
        if (image is null)
        {
            IndustrialControlDrawing.DrawLed(graphics, led, _isOn, Enabled);
        }
        else
        {
            graphics.DrawImage(image, led);
        }

        Rectangle label = new(6, Height - 30, Math.Max(1, Width - 12), 22);
        TextRenderer.DrawText(
            graphics,
            $"{_labelText} • {(_isOn ? "ON" : "OFF")}",
            Font,
            label,
            Enabled
                ? (_isOn ? IndustrialTheme.Palette.Success : IndustrialTheme.Palette.TextSecondary)
                : IndustrialTheme.Palette.Disabled,
            TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis);
    }

    private void UpdateAccessibility()
    {
        AccessibleName = $"Indicador {_labelText}";
        AccessibleDescription = $"{_labelText}: {(_isOn ? "ligado" : "desligado")}";
    }
}
