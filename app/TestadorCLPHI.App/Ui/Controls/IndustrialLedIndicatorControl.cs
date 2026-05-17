using System.Drawing.Drawing2D;

namespace TestadorCLPHI.App.Ui.Controls;

public sealed class IndustrialLedIndicatorControl : Control
{
    private Image? _onImage;
    private Image? _offImage;
    private bool _isOn;
    private string _labelText = "DI";

    public IndustrialLedIndicatorControl()
    {
        SetStyle(
            ControlStyles.AllPaintingInWmPaint |
            ControlStyles.OptimizedDoubleBuffer |
            ControlStyles.ResizeRedraw |
            ControlStyles.UserPaint,
            true);

        Width = 64;
        Height = 72;
        BackColor = Color.FromArgb(24, 32, 42);
        ForeColor = Color.FromArgb(203, 213, 225);
        Margin = new Padding(0, 0, 14, 0);
    }

    public string LabelText
    {
        get => _labelText;
        set
        {
            _labelText = string.IsNullOrWhiteSpace(value) ? "DI" : value.Trim();
            Invalidate();
        }
    }

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
            Invalidate();
        }
    }

    public void LoadImages(string onImagePath, string offImagePath)
    {
        _onImage?.Dispose();
        _offImage?.Dispose();

        _onImage = File.Exists(onImagePath) ? new Bitmap(onImagePath) : null;
        _offImage = File.Exists(offImagePath) ? new Bitmap(offImagePath) : null;

        Invalidate();
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);

        Graphics graphics = e.Graphics;
        graphics.SmoothingMode = SmoothingMode.HighQuality;
        graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
        graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;
        graphics.CompositingQuality = CompositingQuality.HighQuality;

        graphics.Clear(BackColor);

        Image? image = _isOn ? _onImage : _offImage;

        int imageSize = Math.Min(52, Math.Max(40, Height - 20));
        Rectangle imageRect = new((Width - imageSize) / 2, 0, imageSize, imageSize);

        if (image is not null)
        {
            graphics.DrawImage(image, imageRect);
        }

        using Font labelFont = new(Font.FontFamily, 8.0f, FontStyle.Bold);
        using Brush labelBrush = new SolidBrush(ForeColor);
        using StringFormat center = new()
        {
            Alignment = StringAlignment.Center,
            LineAlignment = StringAlignment.Center
        };

        graphics.DrawString(_labelText, labelFont, labelBrush, new RectangleF(0, Height - 18, Width, 16), center);
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _onImage?.Dispose();
            _offImage?.Dispose();
            _onImage = null;
            _offImage = null;
        }

        base.Dispose(disposing);
    }
}
