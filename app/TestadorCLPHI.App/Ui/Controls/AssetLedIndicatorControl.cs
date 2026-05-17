using System.Drawing.Drawing2D;

namespace TestadorCLPHI.App.Ui.Controls;

public sealed class AssetLedIndicatorControl : Control
{
    private bool _isOn;
    private string _labelText = "DI";
    private Image? _onImage;
    private Image? _offImage;
    private string? _onImagePath;
    private string? _offImagePath;

    public AssetLedIndicatorControl()
    {
        SetStyle(
            ControlStyles.AllPaintingInWmPaint |
            ControlStyles.OptimizedDoubleBuffer |
            ControlStyles.ResizeRedraw |
            ControlStyles.UserPaint,
            true);

        Size = new Size(92, 92);
        MinimumSize = new Size(72, 82);
        BackColor = Color.FromArgb(20, 24, 31);
        ForeColor = Color.FromArgb(226, 232, 240);
        Cursor = Cursors.Default;
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

    public string LabelText
    {
        get => _labelText;
        set
        {
            _labelText = string.IsNullOrWhiteSpace(value) ? "DI" : value.Trim();
            Invalidate();
        }
    }

    public string? OnImagePath
    {
        get => _onImagePath;
        set
        {
            _onImagePath = value;
            ReplaceImage(ref _onImage, value);
            Invalidate();
        }
    }

    public string? OffImagePath
    {
        get => _offImagePath;
        set
        {
            _offImagePath = value;
            ReplaceImage(ref _offImage, value);
            Invalidate();
        }
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _onImage?.Dispose();
            _offImage?.Dispose();
        }

        base.Dispose(disposing);
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);

        Graphics graphics = e.Graphics;
        graphics.SmoothingMode = SmoothingMode.AntiAlias;
        graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;

        Rectangle outerRect = new(1, 1, Width - 3, Height - 3);

        using GraphicsPath outerPath = CreateRoundedRectangle(outerRect, 14);
        using SolidBrush outerBrush = new(Color.FromArgb(28, 34, 43));
        using Pen outerPen = new(Color.FromArgb(65, 76, 92), 1);

        graphics.FillPath(outerBrush, outerPath);
        graphics.DrawPath(outerPen, outerPath);

        Rectangle ledRect = GetLedRectangle();

        Image? image = _isOn ? _onImage : _offImage;
        if (image is not null)
        {
            graphics.DrawImage(image, ledRect);
        }
        else
        {
            DrawFallbackLed(graphics, ledRect);
        }

        DrawLabel(graphics);
    }

    private Rectangle GetLedRectangle()
    {
        int diameter = Math.Min(42, Math.Max(30, Math.Min(Width - 28, Height - 42)));
        int x = (Width - diameter) / 2;
        int y = 12;

        return new Rectangle(x, y, diameter, diameter);
    }

    private void DrawFallbackLed(Graphics graphics, Rectangle ledRect)
    {
        Color baseColor = _isOn
            ? Color.FromArgb(42, 185, 100)
            : Color.FromArgb(82, 88, 96);

        Color highlightColor = _isOn
            ? Color.FromArgb(140, 255, 185)
            : Color.FromArgb(145, 150, 160);

        Color shadowColor = _isOn
            ? Color.FromArgb(10, 95, 50)
            : Color.FromArgb(42, 46, 54);

        Rectangle shadowRect = ledRect;
        shadowRect.Offset(0, 3);

        using SolidBrush shadowBrush = new(Color.FromArgb(115, Color.Black));
        graphics.FillEllipse(shadowBrush, shadowRect);

        using LinearGradientBrush ledBrush = new(
            ledRect,
            highlightColor,
            baseColor,
            LinearGradientMode.ForwardDiagonal);

        using Pen borderPen = new(shadowColor, 2);

        graphics.FillEllipse(ledBrush, ledRect);
        graphics.DrawEllipse(borderPen, ledRect);

        Rectangle innerHighlight = new(
            ledRect.X + ledRect.Width / 5,
            ledRect.Y + ledRect.Height / 6,
            ledRect.Width / 3,
            ledRect.Height / 4);

        using SolidBrush highlightBrush = new(Color.FromArgb(_isOn ? 140 : 70, Color.White));
        graphics.FillEllipse(highlightBrush, innerHighlight);

        if (_isOn)
        {
            Rectangle glowRect = ledRect;
            glowRect.Inflate(5, 5);

            using GraphicsPath glowPath = new();
            glowPath.AddEllipse(glowRect);

            using PathGradientBrush glowBrush = new(glowPath)
            {
                CenterColor = Color.FromArgb(90, 58, 225, 125),
                SurroundColors = new[] { Color.FromArgb(0, 58, 225, 125) }
            };

            graphics.FillPath(glowBrush, glowPath);
        }
    }

    private void DrawLabel(Graphics graphics)
    {
        Rectangle labelRect = new(6, Height - 30, Width - 12, 22);

        using Font labelFont = new(Font.FontFamily, 9F, FontStyle.Bold);
        Color labelColor = _isOn
            ? Color.FromArgb(190, 255, 215)
            : Color.FromArgb(180, 188, 200);

        TextRenderer.DrawText(
            graphics,
            _labelText,
            labelFont,
            labelRect,
            labelColor,
            TextFormatFlags.HorizontalCenter |
            TextFormatFlags.VerticalCenter |
            TextFormatFlags.EndEllipsis);
    }

    private static void ReplaceImage(ref Image? currentImage, string? imagePath)
    {
        currentImage?.Dispose();
        currentImage = null;

        if (string.IsNullOrWhiteSpace(imagePath) || !File.Exists(imagePath))
        {
            return;
        }

        using Image loadedImage = Image.FromFile(imagePath);
        currentImage = new Bitmap(loadedImage);
    }

    private static GraphicsPath CreateRoundedRectangle(Rectangle rectangle, int radius)
    {
        GraphicsPath path = new();

        int diameter = radius * 2;

        if (diameter <= 0)
        {
            path.AddRectangle(rectangle);
            return path;
        }

        Rectangle arc = new(rectangle.Location, new Size(diameter, diameter));

        path.AddArc(arc, 180, 90);

        arc.X = rectangle.Right - diameter;
        path.AddArc(arc, 270, 90);

        arc.Y = rectangle.Bottom - diameter;
        path.AddArc(arc, 0, 90);

        arc.X = rectangle.Left;
        path.AddArc(arc, 90, 90);

        path.CloseFigure();
        return path;
    }
}
