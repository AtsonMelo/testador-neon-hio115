using System.Drawing.Drawing2D;

namespace TestadorCLPHI.App.Ui.Controls;

public sealed class AssetPushButtonControl : Control
{
    private bool _isActive;
    private bool _isPressed;
    private string _labelText = "DO";
    private Color _buttonColor = Color.FromArgb(190, 48, 48);
    private Image? _buttonImage;
    private string? _buttonImagePath;

    public AssetPushButtonControl()
    {
        SetStyle(
            ControlStyles.AllPaintingInWmPaint |
            ControlStyles.OptimizedDoubleBuffer |
            ControlStyles.ResizeRedraw |
            ControlStyles.UserPaint,
            true);

        Size = new Size(104, 108);
        MinimumSize = new Size(82, 92);
        BackColor = Color.FromArgb(20, 24, 31);
        ForeColor = Color.FromArgb(226, 232, 240);
        Cursor = Cursors.Hand;
    }

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
            Invalidate();
        }
    }

    public string LabelText
    {
        get => _labelText;
        set
        {
            _labelText = string.IsNullOrWhiteSpace(value) ? "DO" : value.Trim();
            Invalidate();
        }
    }

    public Color ButtonColor
    {
        get => _buttonColor;
        set
        {
            _buttonColor = value;
            Invalidate();
        }
    }

    public string? ButtonImagePath
    {
        get => _buttonImagePath;
        set
        {
            _buttonImagePath = value;
            ReplaceImage(ref _buttonImage, value);
            Invalidate();
        }
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _buttonImage?.Dispose();
        }

        base.Dispose(disposing);
    }

    protected override void OnMouseDown(MouseEventArgs e)
    {
        base.OnMouseDown(e);

        if (e.Button != MouseButtons.Left)
        {
            return;
        }

        _isPressed = true;
        Invalidate();
    }

    protected override void OnMouseUp(MouseEventArgs e)
    {
        base.OnMouseUp(e);

        if (!_isPressed)
        {
            return;
        }

        _isPressed = false;
        Invalidate();
    }

    protected override void OnMouseLeave(EventArgs e)
    {
        base.OnMouseLeave(e);

        if (!_isPressed)
        {
            return;
        }

        _isPressed = false;
        Invalidate();
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

        Rectangle buttonRect = GetButtonRectangle();

        Image? image = _buttonImage;
        if (image is not null)
        {
            graphics.DrawImage(image, buttonRect);
        }
        else
        {
            DrawFallbackButton(graphics, buttonRect);
        }

        DrawLabel(graphics);
    }

    private Rectangle GetButtonRectangle()
    {
        int diameter = Math.Min(54, Math.Max(40, Math.Min(Width - 28, Height - 48)));
        int x = (Width - diameter) / 2;
        int y = 13;

        if (_isPressed)
        {
            y += 2;
        }

        return new Rectangle(x, y, diameter, diameter);
    }

    private void DrawFallbackButton(Graphics graphics, Rectangle buttonRect)
    {
        Color baseColor = _buttonColor;
        Color darkColor = ScaleColor(baseColor, 0.58F);
        Color lightColor = ScaleColor(baseColor, 1.35F);
        Color rimColor = _isActive
            ? Color.FromArgb(240, 220, 235, 255)
            : Color.FromArgb(95, 105, 118);

        Rectangle shadowRect = buttonRect;
        shadowRect.Offset(0, _isPressed ? 3 : 5);

        using SolidBrush shadowBrush = new(Color.FromArgb(_isPressed ? 115 : 150, Color.Black));
        graphics.FillEllipse(shadowBrush, shadowRect);

        Rectangle rimRect = buttonRect;
        rimRect.Inflate(4, 4);

        using LinearGradientBrush rimBrush = new(
            rimRect,
            Color.FromArgb(130, 142, 158),
            Color.FromArgb(40, 46, 56),
            LinearGradientMode.Vertical);

        using Pen rimPen = new(rimColor, _isActive ? 2 : 1);

        graphics.FillEllipse(rimBrush, rimRect);
        graphics.DrawEllipse(rimPen, rimRect);

        using LinearGradientBrush buttonBrush = new(
            buttonRect,
            lightColor,
            darkColor,
            LinearGradientMode.ForwardDiagonal);

        using Pen buttonPen = new(Color.FromArgb(45, Color.Black), 2);

        graphics.FillEllipse(buttonBrush, buttonRect);
        graphics.DrawEllipse(buttonPen, buttonRect);

        Rectangle topHighlight = new(
            buttonRect.X + buttonRect.Width / 5,
            buttonRect.Y + buttonRect.Height / 7,
            buttonRect.Width * 3 / 5,
            buttonRect.Height / 3);

        using SolidBrush highlightBrush = new(Color.FromArgb(_isPressed ? 45 : 95, Color.White));
        graphics.FillEllipse(highlightBrush, topHighlight);

        if (_isActive)
        {
            Rectangle glowRect = buttonRect;
            glowRect.Inflate(7, 7);

            using GraphicsPath glowPath = new();
            glowPath.AddEllipse(glowRect);

            using PathGradientBrush glowBrush = new(glowPath)
            {
                CenterColor = Color.FromArgb(75, lightColor),
                SurroundColors = new[] { Color.FromArgb(0, lightColor) }
            };

            graphics.FillPath(glowBrush, glowPath);
        }
    }

    private void DrawLabel(Graphics graphics)
    {
        Rectangle labelRect = new(6, Height - 34, Width - 12, 24);

        using Font labelFont = new(Font.FontFamily, 9F, FontStyle.Bold);
        Color labelColor = Enabled
            ? Color.FromArgb(226, 232, 240)
            : Color.FromArgb(120, 128, 140);

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

    private static Color ScaleColor(Color color, float factor)
    {
        int red = Math.Clamp((int)(color.R * factor), 0, 255);
        int green = Math.Clamp((int)(color.G * factor), 0, 255);
        int blue = Math.Clamp((int)(color.B * factor), 0, 255);

        return Color.FromArgb(color.A, red, green, blue);
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
