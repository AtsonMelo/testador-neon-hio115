using System.Drawing.Drawing2D;
using TestadorCLPHI.App.Ui.Theme;

namespace TestadorCLPHI.App.Ui.Controls;

internal static class IndustrialControlDrawing
{
    internal static GraphicsPath RoundedRectangle(Rectangle rectangle, int radius)
    {
        GraphicsPath path = new();
        int diameter = Math.Min(Math.Min(rectangle.Width, rectangle.Height), radius * 2);
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

    internal static void DrawCard(
        Graphics graphics,
        Rectangle rectangle,
        bool hovered,
        bool pressed,
        bool enabled)
    {
        IndustrialPalette palette = IndustrialTheme.Palette;
        Color fill = !enabled
            ? palette.Surface
            : pressed ? palette.Surface : hovered ? palette.SurfaceInteractive : palette.SurfaceElevated;
        using GraphicsPath path = RoundedRectangle(rectangle, 10);
        using SolidBrush brush = new(fill);
        using Pen pen = new(enabled ? palette.Border : palette.Disabled, 1F);
        graphics.FillPath(brush, path);
        graphics.DrawPath(pen, path);
    }

    internal static void DrawLed(Graphics graphics, Rectangle rectangle, bool isOn, bool enabled)
    {
        IndustrialPalette palette = IndustrialTheme.Palette;
        Color baseColor = !enabled
            ? palette.Disabled
            : isOn ? palette.Success : palette.Offline;
        Color darkColor = Scale(baseColor, isOn ? 0.58F : 0.62F);
        Color lightColor = Scale(baseColor, isOn ? 1.18F : 1.05F);
        Rectangle shadow = rectangle;
        shadow.Offset(0, 2);
        using SolidBrush shadowBrush = new(Color.FromArgb(95, palette.Background));
        graphics.FillEllipse(shadowBrush, shadow);
        using LinearGradientBrush fill = new(
            rectangle,
            lightColor,
            darkColor,
            LinearGradientMode.ForwardDiagonal);
        using Pen border = new(isOn ? palette.Success : palette.BorderStrong, isOn ? 2F : 1F);
        graphics.FillEllipse(fill, rectangle);
        graphics.DrawEllipse(border, rectangle);

        Rectangle highlight = new(
            rectangle.X + rectangle.Width / 5,
            rectangle.Y + rectangle.Height / 6,
            Math.Max(3, rectangle.Width / 3),
            Math.Max(3, rectangle.Height / 4));
        using SolidBrush highlightBrush = new(Color.FromArgb(isOn ? 115 : 60, palette.TextPrimary));
        graphics.FillEllipse(highlightBrush, highlight);
    }

    internal static void DrawPushButton(
        Graphics graphics,
        Rectangle rectangle,
        Color buttonColor,
        bool active,
        bool hovered,
        bool pressed,
        bool enabled)
    {
        IndustrialPalette palette = IndustrialTheme.Palette;
        Color face = enabled ? buttonColor : palette.Disabled;
        if (hovered && enabled)
        {
            face = Scale(face, 1.10F);
        }

        Rectangle rim = rectangle;
        rim.Inflate(4, 4);
        Rectangle shadow = rectangle;
        shadow.Offset(0, pressed ? 2 : 4);
        using SolidBrush shadowBrush = new(Color.FromArgb(105, palette.Background));
        using SolidBrush rimBrush = new(palette.SurfaceInteractive);
        using Pen rimPen = new(active ? palette.Focus : palette.BorderStrong, active ? 2F : 1F);
        graphics.FillEllipse(shadowBrush, shadow);
        graphics.FillEllipse(rimBrush, rim);
        graphics.DrawEllipse(rimPen, rim);

        using LinearGradientBrush faceBrush = new(
            rectangle,
            Scale(face, 1.16F),
            Scale(face, 0.58F),
            LinearGradientMode.ForwardDiagonal);
        using Pen facePen = new(Scale(face, 0.45F), 2F);
        graphics.FillEllipse(faceBrush, rectangle);
        graphics.DrawEllipse(facePen, rectangle);
    }

    internal static void DrawEmergencyStop(
        Graphics graphics,
        Rectangle rectangle,
        bool hovered,
        bool pressed,
        bool enabled)
    {
        IndustrialPalette palette = IndustrialTheme.Palette;
        Rectangle plate = rectangle;
        plate.Inflate(5, 5);
        using SolidBrush plateBrush = new(enabled ? palette.Warning : palette.Disabled);
        using Pen platePen = new(palette.BorderStrong, 2F);
        graphics.FillEllipse(plateBrush, plate);
        graphics.DrawEllipse(platePen, plate);
        DrawPushButton(
            graphics,
            rectangle,
            palette.Danger,
            active: true,
            hovered: hovered,
            pressed: pressed,
            enabled: enabled);
    }

    internal static void DrawFocus(Graphics graphics, Rectangle rectangle, bool focused)
    {
        if (!focused)
        {
            return;
        }

        using Pen pen = new(IndustrialTheme.Palette.Focus, 2F)
        {
            DashStyle = DashStyle.Dot
        };
        rectangle.Inflate(-3, -3);
        graphics.DrawRectangle(pen, rectangle);
    }

    internal static Rectangle CenteredSquare(
        Rectangle clientRectangle,
        int top,
        int bottomReserve,
        int maximum)
    {
        int available = Math.Max(12, clientRectangle.Height - top - bottomReserve);
        int diameter = Math.Min(maximum, Math.Min(clientRectangle.Width - 24, available));
        diameter = Math.Max(12, diameter);
        return new Rectangle(
            clientRectangle.Left + ((clientRectangle.Width - diameter) / 2),
            clientRectangle.Top + top,
            diameter,
            diameter);
    }

    internal static Color Scale(Color color, float factor) => Color.FromArgb(
        color.A,
        Math.Clamp((int)(color.R * factor), 0, 255),
        Math.Clamp((int)(color.G * factor), 0, 255),
        Math.Clamp((int)(color.B * factor), 0, 255));
}
