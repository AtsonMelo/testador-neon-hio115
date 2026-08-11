using TestadorCLPHI.App.Ui.Theme;

namespace TestadorCLPHI.App.Ui.Industrial.Platform;

internal sealed class ResponsiveRtuConfigurationControl : UserControl
{
    private const int WideBreakpoint = 860;
    private const int CompactBreakpoint = 520;
    private const int FieldRowHeight = 50;
    private const int ActionRowHeight = 38;
    private readonly IReadOnlyList<RtuFieldDefinition> _fields;
    private readonly IReadOnlyList<Button> _actions;
    private readonly Control _status;
    private readonly TableLayoutPanel _root;
    private readonly TableLayoutPanel _fieldGrid;
    private readonly TableLayoutPanel _actionGrid;
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
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 4,
            Margin = Padding.Empty,
            Padding = Padding.Empty,
            Name = "rtuConfigurationLayout"
        };
        _root.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        _root.RowStyles.Add(new RowStyle(SizeType.Absolute, 18F));
        _root.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
        _root.RowStyles.Add(new RowStyle(SizeType.Absolute, ActionRowHeight));
        _root.RowStyles.Add(new RowStyle(SizeType.Absolute, 24F));

        Label title = new()
        {
            Text = "PARÂMETROS RTU  •  REFERÊNCIA OFFLINE",
            Dock = DockStyle.Fill,
            Margin = Padding.Empty,
            TextAlign = ContentAlignment.MiddleLeft,
            Font = IndustrialTypography.CaptionStrong(),
            AccessibleName = "Parâmetros RTU; referência offline"
        };
        _fieldGrid = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            Margin = new Padding(0, IndustrialSpacing.Xs, 0, IndustrialSpacing.Xs),
            Padding = Padding.Empty,
            Name = "rtuFieldGrid"
        };
        _actionGrid = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            Margin = Padding.Empty,
            Padding = Padding.Empty,
            Name = "rtuActionGrid",
            AccessibleName = "Ações do Testador"
        };
        _status.Dock = DockStyle.Fill;
        _status.Margin = new Padding(0, IndustrialSpacing.Xs, 0, 0);

        _root.Controls.Add(title, 0, 0);
        _root.Controls.Add(_fieldGrid, 0, 1);
        _root.Controls.Add(_actionGrid, 0, 2);
        _root.Controls.Add(_status, 0, 3);
        Controls.Add(_root);
        ApplyResponsiveLayout(force: true);
        ApplyTheme();
    }

    internal event EventHandler? PreferredLayoutHeightChanged;

    internal int FieldColumnCount => _fieldColumns;
    internal int ActionColumnCount => _actionColumns;
    internal bool UsesHorizontalScroll => false;

    internal int PreferredLayoutHeight
    {
        get
        {
            int fieldRows = DivideRoundUp(_fields.Count, Math.Max(1, _fieldColumns));
            int actionRows = DivideRoundUp(_actions.Count, Math.Max(1, _actionColumns));
            return Padding.Vertical
                + 18
                + (fieldRows * FieldRowHeight)
                + (actionRows * ActionRowHeight)
                + 24;
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
        foreach (Control child in EnumerateControls(this))
        {
            if (child is Label label && label.Tag is not PlatformStatusTone)
            {
                label.ForeColor = ReferenceEquals(label, _root.GetControlFromPosition(0, 0))
                    ? palette.TextPrimary
                    : palette.TextSecondary;
            }
        }

        Invalidate();
    }

    protected override void OnSizeChanged(EventArgs e)
    {
        base.OnSizeChanged(e);
        ApplyResponsiveLayout(force: false);
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);
        Rectangle border = ClientRectangle;
        border.Width = Math.Max(0, border.Width - 1);
        border.Height = Math.Max(0, border.Height - 1);
        using Pen pen = new(IndustrialTheme.Palette.Border, IndustrialSpacing.BorderWidth);
        e.Graphics.DrawRectangle(pen, border);
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
        _root.RowStyles[1].Height = DivideRoundUp(_fields.Count, _fieldColumns) * FieldRowHeight;
        _root.RowStyles[2].Height = DivideRoundUp(_actions.Count, _actionColumns) * ActionRowHeight;
        PreferredLayoutHeightChanged?.Invoke(this, EventArgs.Empty);
    }

    private void RebuildFields()
    {
        _fieldGrid.SuspendLayout();
        _fieldGrid.Controls.Clear();
        ConfigureGrid(_fieldGrid, _fieldColumns, DivideRoundUp(_fields.Count, _fieldColumns));
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
        ConfigureGrid(_actionGrid, _actionColumns, DivideRoundUp(_actions.Count, _actionColumns));
        for (int index = 0; index < _actions.Count; index++)
        {
            Button button = _actions[index];
            button.Dock = DockStyle.Fill;
            button.Margin = new Padding(IndustrialSpacing.Xs, 0, IndustrialSpacing.Xs, 0);
            _actionGrid.Controls.Add(button, index % _actionColumns, index / _actionColumns);
        }

        _actionGrid.ResumeLayout(performLayout: true);
    }

    private static void ConfigureGrid(TableLayoutPanel grid, int columns, int rows)
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
            grid.RowStyles.Add(new RowStyle(SizeType.Percent, 100F / rows));
        }
    }

    private static int DivideRoundUp(int value, int divisor) => (value + divisor - 1) / divisor;

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
