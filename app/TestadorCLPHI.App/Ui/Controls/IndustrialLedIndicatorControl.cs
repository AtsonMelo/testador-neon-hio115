using System.ComponentModel;
using System.Drawing.Drawing2D;
using TestadorCLPHI.App.Ui.Theme;

namespace TestadorCLPHI.App.Ui.Controls;

public sealed class IndustrialLedIndicatorControl : Control
{
    private bool _isOn;
    private string _labelText = "DI";
    private string? _onImagePath;
    private string? _offImagePath;

    public IndustrialLedIndicatorControl()
    {
        SetStyle(
            ControlStyles.AllPaintingInWmPaint |
            ControlStyles.ApplyThemingImplicitly |
            ControlStyles.OptimizedDoubleBuffer |
            ControlStyles.ResizeRedraw |
            ControlStyles.UserPaint,
            true);
        Size = new Size(72, 80);
        MinimumSize = new Size(64, 72);
        Margin = new Padding(0, 0, 14, 0);
        Font = IndustrialTypography.CaptionStrong();
        TabStop = false;
        AccessibleRole = AccessibleRole.Graphic;
        UpdateAccessibility();
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

    public void LoadImages(string onImagePath, string offImagePath)
    {
        _onImagePath = onImagePath;
        _offImagePath = offImagePath;
        _ = IndustrialAssetCache.Get(onImagePath);
        _ = IndustrialAssetCache.Get(offImagePath);
        Invalidate();
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);
        Graphics graphics = e.Graphics;
        graphics.SmoothingMode = SmoothingMode.AntiAlias;
        graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
        graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;
        graphics.Clear(IndustrialTheme.Palette.Surface);

        Rectangle led = IndustrialControlDrawing.CenteredSquare(ClientRectangle, 3, 24, 50);
        Image? image = IndustrialAssetCache.Get(_isOn ? _onImagePath : _offImagePath);
        if (image is null)
        {
            IndustrialControlDrawing.DrawLed(graphics, led, _isOn, Enabled);
        }
        else
        {
            graphics.DrawImage(image, led);
        }

        Rectangle label = new(2, Height - 20, Math.Max(1, Width - 4), 18);
        TextRenderer.DrawText(
            graphics,
            $"{_labelText} {(_isOn ? "ON" : "OFF")}",
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
