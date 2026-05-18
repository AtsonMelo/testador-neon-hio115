using System.Drawing.Drawing2D;

namespace TestadorCLPHI.App.Ui.Controls;

public sealed class EmergencyStopButtonControl : Control
{
    private Image? _image;

    public EmergencyStopButtonControl()
    {
        SetStyle(
            ControlStyles.AllPaintingInWmPaint |
            ControlStyles.OptimizedDoubleBuffer |
            ControlStyles.ResizeRedraw |
            ControlStyles.UserPaint,
            true);

        Width = 96;
        Height = 92;
        BackColor = Color.FromArgb(31, 31, 31);
        ForeColor = Color.White;
        Cursor = Cursors.Hand;
    }

    public string Title { get; set; } = "STOP";

    public string? ButtonImagePath { get; set; }

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);

        Graphics graphics = e.Graphics;
        graphics.SmoothingMode = SmoothingMode.HighQuality;
        graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
        graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;
        graphics.CompositingQuality = CompositingQuality.HighQuality;

        graphics.Clear(BackColor);

        using StringFormat center = new()
        {
            Alignment = StringAlignment.Center,
            LineAlignment = StringAlignment.Center
        };

        using Font titleFont = new(Font.FontFamily, 9.5F, FontStyle.Bold);
        using Brush titleBrush = new SolidBrush(ForeColor);

        graphics.DrawString(Title, titleFont, titleBrush, new RectangleF(0, 0, Width, 18), center);

        Image? image = GetImage();
        if (image is not null)
        {
            int imageSize = Math.Min(54, Math.Max(44, Height - 34));
            Rectangle imageRect = new((Width - imageSize) / 2, 22, imageSize, imageSize);
            graphics.DrawImage(image, imageRect);
        }
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _image?.Dispose();
            _image = null;
        }

        base.Dispose(disposing);
    }

    private Image? GetImage()
    {
        if (_image is not null)
        {
            return _image;
        }

        if (string.IsNullOrWhiteSpace(ButtonImagePath) || !File.Exists(ButtonImagePath))
        {
            return null;
        }

        using FileStream stream = File.OpenRead(ButtonImagePath);
        using Image image = Image.FromStream(stream);
        _image = new Bitmap(image);

        return _image;
    }
}
