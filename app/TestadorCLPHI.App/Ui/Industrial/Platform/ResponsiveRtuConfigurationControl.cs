using System.Drawing.Drawing2D;
using TestadorCLPHI.App.Ui.Controls;
using TestadorCLPHI.App.Ui.Theme;

namespace TestadorCLPHI.App.Ui.Industrial.Platform;

internal sealed class ResponsiveRtuConfigurationControl : UserControl
{
    private const int WideBreakpoint = 860;
    private const int CompactBreakpoint = 520;
    private const int TitleRowHeight = 28;
    private const int FieldRowHeight = 46;
    private const int ActionRowHeight = IndustrialSpacing.InteractiveHeight;
    private const int StatusRowHeight = 22;
    private readonly IReadOnlyList<RtuFieldDefinition> _fields;
    private readonly IReadOnlyList<Button> _actions;
    private readonly Control _status;
    private readonly TableLayoutPanel _root;
    private readonly TableLayoutPanel _fieldGrid;
    private readonly TableLayoutPanel _actionGrid;
    private readonly Label _title;
    private readonly Label _subtitle;
    private readonly Panel _titleDivider;
    private Region? _roundedRegion;
    private int _fieldColumns;
    private int _actionColumns;

    internal ResponsiveRtuConfigurationControl(
        IReadOnlyList<RtuFieldDefinition> fields,
        IReadOnlyList<Button> actions,
        Control status)
    {
        _fields = fields ?? throw new ArgumentNullException(nameof(fields));
        _actions = actions ?? throw new ArgumentNullException(nameof(actions));
        _status = status ?? throw new ArgumentNullException(nameof(status));
        Name = "rtuConfigurationPanel";
        AccessibleName = "Parâmetros RTU de referência offline";
        AccessibleRole = AccessibleRole.Grouping;
        AutoScaleMode = AutoScaleMode.Dpi;
        DoubleBuffered = true;
        Padding = new Padding(
            IndustrialSpacing.Sm,
            IndustrialSpacing.Xs,
            IndustrialSpacing.Sm,
            IndustrialSpacing.Xs);

        _root = new TableLayoutPanel
        {
            Dock = DockStyle.Top,
            AutoSize = true,
            AutoSizeMode = AutoSizeMode.GrowAndShrink,
            ColumnCount = 1,
            RowCount = 4,
            Margin = Padding.Empty,
            Padding = Padding.Empty,
            Name = "rtuConfigurationLayout"
        };
        _root.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        _root.RowStyles.Add(new RowStyle(SizeType.Absolute, TitleRowHeight));
        _root.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        _root.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        _root.RowStyles.Add(new RowStyle(SizeType.Absolute, StatusRowHeight));

        _title = new Label
        {
            Name = "rtuConfigurationTitle",
            Text = "Parâmetros RTU",
            Dock = DockStyle.Fill,
            Margin = Padding.Empty,
            TextAlign = ContentAlignment.BottomLeft,
            Font = IndustrialTypography.SectionTitle(),
            AccessibleName = "Parâmetros RTU"
        };
        _subtitle = new Label
        {
            Name = "rtuConfigurationSubtitle",
            Text = "Referência offline",
            Dock = DockStyle.Fill,
            Margin = Padding.Empty,
            TextAlign = ContentAlignment.TopLeft,
            Font = IndustrialTypography.Status(),
            AccessibleName = "Referência offline"
        };
        _titleDivider = new Panel
        {
            Name = "rtuConfigurationTitleDivider",
            Dock = DockStyle.Fill,
            Margin = Padding.Empty
        };
        TableLayoutPanel header = new()
        {
            Name = "rtuConfigurationSectionHeader",
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 3,
            Margin = Padding.Empty,
            Padding = Padding.Empty
        };
        header.RowStyles.Add(new RowStyle(SizeType.Absolute, 16F));
        header.RowStyles.Add(new RowStyle(SizeType.Absolute, 11F));
        header.RowStyles.Add(new RowStyle(SizeType.Absolute, 1F));
        header.Controls.Add(_title, 0, 0);
        header.Controls.Add(_subtitle, 0, 1);
        header.Controls.Add(_titleDivider, 0, 2);
        _fieldGrid = new TableLayoutPanel
        {
            Dock = DockStyle.Top,
            AutoSize = true,
            AutoSizeMode = AutoSizeMode.GrowAndShrink,
            Margin = Padding.Empty,
            Padding = Padding.Empty,
            Name = "rtuFieldGrid"
        };
        _actionGrid = new TableLayoutPanel
        {
            Dock = DockStyle.Top,
            AutoSize = true,
            AutoSizeMode = AutoSizeMode.GrowAndShrink,
            Margin = Padding.Empty,
            Padding = Padding.Empty,
            Name = "rtuActionGrid",
            AccessibleName = "Ações do Testador"
        };
        _status.Dock = DockStyle.Fill;
        _status.Margin = new Padding(0, IndustrialSpacing.Xs, 0, 0);

        _root.Controls.Add(header, 0, 0);
        _root.Controls.Add(_fieldGrid, 0, 1);
        _root.Controls.Add(_actionGrid, 0, 2);
        _root.Controls.Add(_status, 0, 3);
        Controls.Add(_root);
        UpdateRoundedRegion();
        ApplyResponsiveLayout(force: true);
        ApplyTheme();
    }

    internal event EventHandler? PreferredLayoutHeightChanged;

    internal int FieldColumnCount => _fieldColumns;
    internal int ActionColumnCount => _actionColumns;
    internal bool UsesHorizontalScroll => false;
    internal IReadOnlyList<string> FieldLabels => _fields.Select(item => item.Label).ToArray();

    internal int PreferredLayoutHeight
    {
        get
        {
            int fieldRows = DivideRoundUp(_fields.Count, Math.Max(1, _fieldColumns));
            int actionRows = DivideRoundUp(_actions.Count, Math.Max(1, _actionColumns));
            return Padding.Vertical
                + TitleRowHeight
                + (fieldRows * FieldRowHeight)
                + (actionRows * ActionRowHeight)
                + _fieldGrid.Margin.Vertical
                + _actionGrid.Margin.Vertical
                + StatusRowHeight;
        }
    }

    internal void ApplyTheme()
    {
        IndustrialPalette palette = IndustrialTheme.Palette;
        BackColor = palette.SurfaceElevated;
        ForeColor = palette.TextPrimary;
        _root.BackColor = palette.SurfaceElevated;
        _fieldGrid.BackColor = palette.SurfaceElevated;
        _actionGrid.BackColor = palette.SurfaceElevated;
        _title.ForeColor = palette.TextPrimary;
        _subtitle.ForeColor = palette.TextMuted;
        _titleDivider.BackColor = palette.Border;
        foreach (Control child in EnumerateControls(this))
        {
            if (child is Label label && label.Tag is not PlatformStatusTone)
            {
                label.ForeColor = ReferenceEquals(label, _title)
                    ? palette.TextPrimary
                    : ReferenceEquals(label, _subtitle)
                        ? palette.TextMuted
                        : palette.TextSecondary;
            }
        }

        Invalidate();
    }

    protected override void OnSizeChanged(EventArgs e)
    {
        base.OnSizeChanged(e);
        UpdateRoundedRegion();
        ApplyResponsiveLayout(force: false);
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);
        Rectangle border = ClientRectangle;
        border.Width = Math.Max(0, border.Width - 1);
        border.Height = Math.Max(0, border.Height - 1);
        SmoothingMode previous = e.Graphics.SmoothingMode;
        e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
        using GraphicsPath path = IndustrialControlDrawing.RoundedRectangle(border, 8);
        using Pen pen = new(IndustrialTheme.Palette.Border, IndustrialSpacing.BorderWidth);
        e.Graphics.DrawPath(pen, path);
        e.Graphics.SmoothingMode = previous;
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            Region = null;
            _roundedRegion?.Dispose();
            _roundedRegion = null;
        }

        base.Dispose(disposing);
    }

    private void ApplyResponsiveLayout(bool force)
    {
        int logicalWidth = DeviceDpi > 0
            ? (int)Math.Round(ClientSize.Width * 96D / DeviceDpi)
            : ClientSize.Width;
        int fieldColumns = logicalWidth >= WideBreakpoint
            ? 5
            : logicalWidth >= CompactBreakpoint ? 2 : 1;
        int actionColumns = logicalWidth >= WideBreakpoint
            ? 5
            : logicalWidth >= CompactBreakpoint ? 2 : 1;
        if (!force && fieldColumns == _fieldColumns && actionColumns == _actionColumns)
        {
            return;
        }

        _fieldColumns = fieldColumns;
        _actionColumns = actionColumns;
        RebuildFields();
        RebuildActions();
        PreferredLayoutHeightChanged?.Invoke(this, EventArgs.Empty);
    }

    private void RebuildFields()
    {
        _fieldGrid.SuspendLayout();
        _fieldGrid.Controls.Clear();
        ConfigureGrid(
            _fieldGrid,
            _fieldColumns,
            DivideRoundUp(_fields.Count, _fieldColumns),
            FieldRowHeight);
        for (int index = 0; index < _fields.Count; index++)
        {
            RtuFieldDefinition definition = _fields[index];
            TableLayoutPanel fieldContainer = new()
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 2,
                Margin = new Padding(IndustrialSpacing.Xs, 0, IndustrialSpacing.Xs, IndustrialSpacing.Xs)
            };
            fieldContainer.RowStyles.Add(new RowStyle(SizeType.Absolute, 18F));
            fieldContainer.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            Label caption = PlatformUi.Label(definition.Label);
            caption.Font = IndustrialTypography.FieldLabel();
            caption.Dock = DockStyle.Fill;
            caption.AutoEllipsis = true;
            caption.Margin = Padding.Empty;
            definition.Editor.Dock = DockStyle.Fill;
            definition.Editor.Margin = Padding.Empty;
            definition.Editor.AccessibleName = definition.Label;
            PlatformUi.StyleField(definition.Editor);
            fieldContainer.Controls.Add(caption, 0, 0);
            fieldContainer.Controls.Add(definition.Editor, 0, 1);
            _fieldGrid.Controls.Add(fieldContainer, index % _fieldColumns, index / _fieldColumns);
        }

        _fieldGrid.ResumeLayout(performLayout: true);
    }

    private void RebuildActions()
    {
        _actionGrid.SuspendLayout();
        _actionGrid.Controls.Clear();
        ConfigureGrid(
            _actionGrid,
            _actionColumns,
            DivideRoundUp(_actions.Count, _actionColumns),
            ActionRowHeight);
        for (int index = 0; index < _actions.Count; index++)
        {
            Button button = _actions[index];
            button.Dock = DockStyle.Fill;
            button.Margin = new Padding(IndustrialSpacing.Xs, 0, IndustrialSpacing.Xs, 0);
            _actionGrid.Controls.Add(button, index % _actionColumns, index / _actionColumns);
        }

        _actionGrid.ResumeLayout(performLayout: true);
    }

    private static void ConfigureGrid(
        TableLayoutPanel grid,
        int columns,
        int rows,
        int rowHeight)
    {
        grid.ColumnCount = columns;
        grid.RowCount = rows;
        grid.ColumnStyles.Clear();
        grid.RowStyles.Clear();
        for (int column = 0; column < columns; column++)
        {
            grid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F / columns));
        }

        for (int row = 0; row < rows; row++)
        {
            grid.RowStyles.Add(new RowStyle(SizeType.Absolute, rowHeight));
        }
    }

    private static int DivideRoundUp(int value, int divisor) => (value + divisor - 1) / divisor;

    private void UpdateRoundedRegion()
    {
        if (ClientSize.Width <= 0 || ClientSize.Height <= 0)
        {
            return;
        }

        using GraphicsPath path = IndustrialControlDrawing.RoundedRectangle(ClientRectangle, 8);
        Region replacement = new(path);
        Region? previous = _roundedRegion;
        _roundedRegion = replacement;
        Region = replacement;
        previous?.Dispose();
    }

    private static IEnumerable<Control> EnumerateControls(Control root)
    {
        foreach (Control child in root.Controls)
        {
            yield return child;
            foreach (Control descendant in EnumerateControls(child))
            {
                yield return descendant;
            }
        }
    }
}

internal readonly record struct RtuFieldDefinition(string Label, Control Editor);
