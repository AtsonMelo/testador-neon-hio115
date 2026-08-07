using System.Drawing.Drawing2D;
using TestadorCLPHI.App.Industrial.Platform.Process;

namespace TestadorCLPHI.App.Ui.Industrial.Platform;

internal sealed class PivotProcessControl : Control
{
    private readonly Pen _structurePen = new(Color.FromArgb(126, 146, 164), 3F);
    private readonly Pen _mutedPen = new(PlatformUi.ButtonSurface, 2F);
    private readonly Pen _directionPen = new(PlatformUi.Accent, 3F);
    private readonly Pen _waterPen = new(Color.FromArgb(54, 151, 210), 2F) { DashStyle = DashStyle.Dash };
    private readonly SolidBrush _surfaceBrush = new(PlatformUi.Surface);
    private readonly SolidBrush _fieldBrush = new(PlatformUi.Field);
    private readonly SolidBrush _accentBrush = new(PlatformUi.Accent);
    private readonly SolidBrush _successBrush = new(PlatformUi.Success);
    private readonly SolidBrush _warningBrush = new(PlatformUi.Warning);
    private readonly SolidBrush _dangerBrush = new(PlatformUi.Danger);
    private readonly SolidBrush _mutedBrush = new(PlatformUi.Muted);
    private readonly SolidBrush _waterBrush = new(Color.FromArgb(54, 151, 210));
    private readonly Font _titleFont = new("Segoe UI Semibold", 11F);
    private readonly Font _labelFont = new("Segoe UI Semibold", 8.5F);
    private readonly Font _smallFont = new("Segoe UI", 8F);
    private PivotProcessState? _state;
    private string _stateSignature = string.Empty;

    internal PivotProcessControl()
    {
        Name = "pivotProcessControl";
        AccessibleName = "Representação visual do Pivô Central simulado";
        AccessibleRole = AccessibleRole.Graphic;
        BackColor = PlatformUi.Surface;
        ForeColor = PlatformUi.Text;
        MinimumSize = new Size(420, 240);
        DoubleBuffered = true;
        ResizeRedraw = true;
        SetStyle(
            ControlStyles.AllPaintingInWmPaint
            | ControlStyles.OptimizedDoubleBuffer
            | ControlStyles.UserPaint,
            true);
    }

    internal int TowerCount => _state?.Towers.Count ?? 0;
    internal int StateRevision { get; private set; }
    internal bool UsesContinuousAnimation => false;
    internal bool IsDoubleBuffered => GetStyle(ControlStyles.OptimizedDoubleBuffer);

    internal void UpdateState(PivotProcessState state)
    {
        ArgumentNullException.ThrowIfNull(state);
        string signature = BuildSignature(state);
        if (string.Equals(signature, _stateSignature, StringComparison.Ordinal))
        {
            return;
        }

        _state = state;
        _stateSignature = signature;
        StateRevision++;
        AccessibleDescription = BuildAccessibleDescription(state);
        Invalidate();
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);
        Graphics graphics = e.Graphics;
        graphics.SmoothingMode = SmoothingMode.AntiAlias;
        graphics.FillRectangle(_surfaceBrush, ClientRectangle);
        if (_state is null || ClientSize.Width < 120 || ClientSize.Height < 120)
        {
            TextRenderer.DrawText(
                graphics,
                "Aguardando estado do processo",
                _titleFont,
                ClientRectangle,
                PlatformUi.Muted,
                TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
            return;
        }

        DrawHeader(graphics, _state);
        Rectangle processArea = new(18, 50, ClientSize.Width - 36, Math.Max(120, ClientSize.Height - 112));
        DrawPivot(graphics, processArea, _state);
        DrawPosition(graphics, _state);
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _structurePen.Dispose();
            _mutedPen.Dispose();
            _directionPen.Dispose();
            _waterPen.Dispose();
            _surfaceBrush.Dispose();
            _fieldBrush.Dispose();
            _accentBrush.Dispose();
            _successBrush.Dispose();
            _warningBrush.Dispose();
            _dangerBrush.Dispose();
            _mutedBrush.Dispose();
            _waterBrush.Dispose();
            _titleFont.Dispose();
            _labelFont.Dispose();
            _smallFont.Dispose();
        }

        base.Dispose(disposing);
    }

    private void DrawHeader(Graphics graphics, PivotProcessState state)
    {
        Rectangle titleBounds = new(18, 12, ClientSize.Width - 210, 30);
        TextRenderer.DrawText(
            graphics,
            $"PIVÔ CENTRAL  •  {state.OverallState}",
            _titleFont,
            titleBounds,
            state.OutputsBlocked ? PlatformUi.Danger : PlatformUi.Text,
            TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis);
        Rectangle positionBounds = new(ClientSize.Width - 190, 12, 172, 30);
        TextRenderer.DrawText(
            graphics,
            $"POSIÇÃO  {state.PositionPercent:0.#}%",
            _labelFont,
            positionBounds,
            PlatformUi.Text,
            TextFormatFlags.Right | TextFormatFlags.VerticalCenter);
    }

    private void DrawPivot(Graphics graphics, Rectangle area, PivotProcessState state)
    {
        int centerX = area.Left + Math.Min(72, Math.Max(46, area.Width / 9));
        int axisY = area.Top + Math.Max(56, area.Height / 2 - 8);
        int endX = area.Right - 24;
        int lineStartX = centerX + 23;
        graphics.FillEllipse(_fieldBrush, centerX - 24, axisY - 24, 48, 48);
        graphics.DrawEllipse(_structurePen, centerX - 24, axisY - 24, 48, 48);
        graphics.FillEllipse(state.PumpActive ? _successBrush : _mutedBrush, centerX - 8, axisY - 8, 16, 16);
        TextRenderer.DrawText(
            graphics,
            "CENTRO",
            _labelFont,
            new Rectangle(centerX - 38, axisY + 28, 76, 22),
            PlatformUi.Text,
            TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
        TextRenderer.DrawText(
            graphics,
            state.PumpActive ? "BOMBA ON" : "BOMBA OFF",
            _smallFont,
            new Rectangle(centerX - 44, axisY + 48, 88, 20),
            state.PumpActive ? PlatformUi.Success : PlatformUi.Muted,
            TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);

        graphics.DrawLine(_structurePen, lineStartX, axisY, endX, axisY);
        if (state.WaterActive)
        {
            graphics.DrawLine(_waterPen, lineStartX, axisY + 8, endX, axisY + 8);
            DrawWaterDrops(graphics, lineStartX, endX, axisY + 14);
        }

        int spanWidth = Math.Max(1, endX - lineStartX);
        for (int index = 0; index < state.Towers.Count; index++)
        {
            int towerX = lineStartX + (int)Math.Round(spanWidth * ((index + 1D) / state.Towers.Count));
            DrawTower(graphics, towerX, axisY, state.Towers[index]);
        }

        DrawDirection(graphics, lineStartX, endX, area.Top + 20, state.Direction);
    }

    private void DrawTower(Graphics graphics, int x, int axisY, PivotTowerState tower)
    {
        SolidBrush statusBrush = tower.Status switch
        {
            PivotTowerStatus.Ok => _successBrush,
            PivotTowerStatus.Moving => _accentBrush,
            PivotTowerStatus.Misaligned => _warningBrush,
            PivotTowerStatus.Fault => _dangerBrush,
            _ => _mutedBrush
        };
        graphics.DrawLine(_mutedPen, x, axisY + 2, x - 9, axisY + 35);
        graphics.DrawLine(_mutedPen, x, axisY + 2, x + 9, axisY + 35);
        graphics.DrawLine(_mutedPen, x - 13, axisY + 35, x + 13, axisY + 35);
        graphics.FillEllipse(statusBrush, x - 8, axisY - 8, 16, 16);
        Rectangle labelBounds = new(x - 42, axisY + 39, 84, 20);
        TextRenderer.DrawText(
            graphics,
            $"T{tower.Number}  {FormatTowerStatus(tower.Status)}",
            _smallFont,
            labelBounds,
            PlatformUi.Text,
            TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis);
    }

    private void DrawDirection(
        Graphics graphics,
        int lineStartX,
        int endX,
        int y,
        PivotMovementDirection direction)
    {
        string label = direction switch
        {
            PivotMovementDirection.Forward => "SENTIDO: FRENTE",
            PivotMovementDirection.Reverse => "SENTIDO: REVERSO",
            _ => "SENTIDO: PARADO"
        };
        TextRenderer.DrawText(
            graphics,
            label,
            _labelFont,
            new Rectangle(lineStartX, y - 18, Math.Max(120, endX - lineStartX), 20),
            direction == PivotMovementDirection.Stopped ? PlatformUi.Muted : PlatformUi.Accent,
            TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
        if (direction == PivotMovementDirection.Stopped)
        {
            return;
        }

        int middle = (lineStartX + endX) / 2;
        int arrowStart = direction == PivotMovementDirection.Forward ? middle - 34 : middle + 34;
        int arrowEnd = direction == PivotMovementDirection.Forward ? middle + 34 : middle - 34;
        graphics.DrawLine(_directionPen, arrowStart, y + 8, arrowEnd, y + 8);
        int sign = direction == PivotMovementDirection.Forward ? 1 : -1;
        graphics.DrawLine(_directionPen, arrowEnd, y + 8, arrowEnd - (10 * sign), y);
        graphics.DrawLine(_directionPen, arrowEnd, y + 8, arrowEnd - (10 * sign), y + 16);
    }

    private void DrawWaterDrops(Graphics graphics, int startX, int endX, int y)
    {
        int width = Math.Max(1, endX - startX);
        for (int index = 1; index <= 5; index++)
        {
            int x = startX + (width * index / 6);
            graphics.FillEllipse(_waterBrush, x - 3, y, 6, 9);
        }
    }

    private void DrawPosition(Graphics graphics, PivotProcessState state)
    {
        Rectangle track = new(18, ClientSize.Height - 38, ClientSize.Width - 36, 14);
        graphics.FillRectangle(_fieldBrush, track);
        int fillWidth = (int)Math.Round(track.Width * (state.PositionPercent / 100D));
        if (fillWidth > 0)
        {
            graphics.FillRectangle(_accentBrush, track.Left, track.Top, fillWidth, track.Height);
        }

        graphics.DrawRectangle(_mutedPen, track);
        TextRenderer.DrawText(
            graphics,
            "0%",
            _smallFont,
            new Rectangle(track.Left, track.Bottom + 1, 40, 18),
            PlatformUi.Muted,
            TextFormatFlags.Left);
        TextRenderer.DrawText(
            graphics,
            "100%",
            _smallFont,
            new Rectangle(track.Right - 44, track.Bottom + 1, 44, 18),
            PlatformUi.Muted,
            TextFormatFlags.Right);
    }

    private static string FormatTowerStatus(PivotTowerStatus status) => status switch
    {
        PivotTowerStatus.Ok => "OK",
        PivotTowerStatus.Moving => "MOVING",
        PivotTowerStatus.Misaligned => "MISALIGNED",
        PivotTowerStatus.Fault => "FAULT",
        _ => "UNKNOWN"
    };

    private static string BuildSignature(PivotProcessState state) =>
        $"{state.OverallState}|{state.PositionPercent:R}|{state.Direction}|{state.PumpActive}|"
        + $"{state.WaterActive}|{state.EmergencyActive}|{state.OutputsBlocked}|"
        + string.Join(',', state.Towers.Select(tower => $"{tower.Number}:{tower.Status}"));

    private static string BuildAccessibleDescription(PivotProcessState state) =>
        $"Estado {state.OverallState}; posição {state.PositionPercent:0.#} por cento; "
        + $"sentido {state.Direction}; bomba {(state.PumpActive ? "ligada" : "desligada")}; "
        + string.Join(", ", state.Towers.Select(tower =>
            $"torre {tower.Number} {FormatTowerStatus(tower.Status)}"));
}
