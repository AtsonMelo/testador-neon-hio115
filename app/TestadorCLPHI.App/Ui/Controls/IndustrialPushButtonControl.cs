using System.Drawing.Drawing2D;

namespace TestadorCLPHI.App.Ui.Controls;

public sealed class IndustrialPushButtonControl : Control
{
    private Image? _buttonImage;

    public IndustrialPushButtonControl()
    {
        SetStyle(
            ControlStyles.AllPaintingInWmPaint |
            ControlStyles.OptimizedDoubleBuffer |
            ControlStyles.ResizeRedraw |
            ControlStyles.UserPaint,
            true);

        BackColor = Color.FromArgb(42, 52, 64);
        ForeColor = Color.FromArgb(226, 232, 240);
        Size = new Size(132, 92);
    }

    public string Title { get; set; } = "D000";

    public string Description { get; set; } = "D000 -> DI00 + DI04";

    public string? ButtonImagePath { get; set; }

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);

        Graphics graphics = e.Graphics;
        graphics.SmoothingMode = SmoothingMode.HighQuality;
        graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
        graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;
        graphics.Clear(BackColor);

        using Font titleFont = new(Font.FontFamily, 9.0f, FontStyle.Bold);
        using Font descFont = new(Font.FontFamily, 7.2f, FontStyle.Regular);
        using Brush titleBrush = new SolidBrush(Color.White);
        using Brush descBrush = new SolidBrush(Color.FromArgb(205, 225, 235, 245));
        using Pen borderPen = new(Color.FromArgb(72, 96, 116, 136));
        using Brush cardBrush = new SolidBrush(Color.FromArgb(32, 42, 52, 64));

        Rectangle cardRect = new(0, 0, Width - 1, Height - 1);
        graphics.FillRectangle(cardBrush, cardRect);
        graphics.DrawRectangle(borderPen, cardRect);

        using StringFormat center = new()
        {
            Alignment = StringAlignment.Center,
            LineAlignment = StringAlignment.Center
        };

        graphics.DrawString(Title, titleFont, titleBrush, new RectangleF(0, 5, Width, 16), center);

        Image? image = GetImage();
        int imageSize = Math.Min(52, Math.Max(36, Height - 44));
        Rectangle imageRect = new((Width - imageSize) / 2, 24, imageSize, imageSize);

        if (image is not null)
        {
            graphics.DrawImage(image, imageRect);
        }

        graphics.DrawString(Description, descFont, descBrush, new RectangleF(0, Height - 17, Width, 14), center);
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _buttonImage?.Dispose();
            _buttonImage = null;
        }

        base.Dispose(disposing);
    }

    private Image? GetImage()
    {
        if (_buttonImage is not null)
        {
            return _buttonImage;
        }

        if (string.IsNullOrWhiteSpace(ButtonImagePath) || !File.Exists(ButtonImagePath))
        {
            return null;
        }

        using FileStream stream = File.OpenRead(ButtonImagePath);
        using Image image = Image.FromStream(stream);
        _buttonImage = new Bitmap(image);

        return _buttonImage;
    }
}
