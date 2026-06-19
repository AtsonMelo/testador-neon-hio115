using TestadorCLPHI.App.Hardware;

namespace TestadorCLPHI.App.Ui.Industrial.Layout3;

internal sealed class Layout3HostControl : UserControl
{
    private const int MinimumContentWidth = 1000;
    private const int MinimumContentHeight = 580;

    private readonly Layout3HostState _state;
    private readonly ILayout3CommandGuard _commandGuard;
    private readonly HardwareCatalog _hardwareCatalog;
    private readonly Panel _viewport;

    private Layout3ThemePalette _palette;
    private Layout3PreviewTheme _theme;
    private TableLayoutPanel _content;
    private TextBox _localLogTextBox;

    /// <summary>Raised when the operator switches the read-only host theme.</summary>
    public event Action<Layout3PreviewTheme>? ThemeChanged;

    public Layout3HostControl(
        HardwareCatalog hardwareCatalog,
        Layout3HostState state,
        ILayout3CommandGuard commandGuard,
        Layout3PreviewTheme theme)
    {
        _state = state ?? throw new ArgumentNullException(nameof(state));
        _commandGuard = commandGuard ?? throw new ArgumentNullException(nameof(commandGuard));
        _hardwareCatalog = hardwareCatalog ?? HardwareCatalog.Empty;
        _theme = theme;
        _palette = Layout3ThemePalette.For(theme);

        Dock = DockStyle.Fill;
        BackColor = _palette.Background;
        AutoScaleMode = AutoScaleMode.Dpi;

        _localLogTextBox = CreateLocalLogTextBox();
        _content = CreateContent(_hardwareCatalog);
        _viewport = new Panel
        {
            Dock = DockStyle.Fill,
            AutoScroll = true,
            BackColor = _palette.Background
        };
        _viewport.Controls.Add(_content);
        Controls.Add(_viewport);

        Resize += (_, _) => ResizeContentToViewport();
        ResizeContentToViewport();
    }

    /// <summary>
    /// Rebuilds the read-only host with a new theme palette. No hardware access is
    /// performed; only colors and visual surfaces are refreshed. The local log text
    /// is preserved across the rebuild.
    /// </summary>
    private void ApplyTheme(Layout3PreviewTheme theme)
    {
        if (theme == _theme)
        {
            return;
        }

        _theme = theme;
        _palette = Layout3ThemePalette.For(theme);
        string preservedLog = _localLogTextBox.Text;

        SuspendLayout();
        try
        {
            BackColor = _palette.Background;
            _viewport.BackColor = _palette.Background;
            _viewport.Controls.Remove(_content);
            _content.Dispose();

            _localLogTextBox = CreateLocalLogTextBox(preservedLog);
            _content = CreateContent(_hardwareCatalog);
            _viewport.Controls.Add(_content);
            ResizeContentToViewport();
        }
        finally
        {
            ResumeLayout(performLayout: true);
        }

        ThemeChanged?.Invoke(theme);
    }

    private TableLayoutPanel CreateContent(HardwareCatalog hardwareCatalog)
    {
        TableLayoutPanel content = new()
        {
            ColumnCount = 1,
            RowCount = 3,
            BackColor = _palette.Background,
            Padding = new Padding(12),
            Margin = Padding.Empty
        };
        content.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        content.RowStyles.Add(new RowStyle(SizeType.Absolute, 76F));
        content.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
        content.RowStyles.Add(new RowStyle(SizeType.Absolute, 136F));

        content.Controls.Add(CreateTopBar(), 0, 0);
        content.Controls.Add(CreateMainArea(hardwareCatalog), 0, 1);
        content.Controls.Add(CreateTerminalArea(), 0, 2);
        return content;
    }

    private Control CreateTopBar()
    {
        Panel panel = CreateSurfacePanel();
        panel.Padding = new Padding(14, 8, 12, 8);

        TableLayoutPanel layout = new()
        {
            Dock = DockStyle.Fill,
            ColumnCount = 3,
            RowCount = 1,
            BackColor = _palette.Surface
        };
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
        // Title stays on the left; the status indicators sit centered in the middle
        // region and the theme selector is anchored to the right.
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33F));
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 17F));

        layout.Controls.Add(CreateIdentity(), 0, 0);
        layout.Controls.Add(CreateIndicatorStrip(), 1, 0);
        layout.Controls.Add(CreateThemeSelector(), 2, 0);

        panel.Controls.Add(layout);
        return panel;
    }

    private Control CreateIndicatorStrip()
    {
        TableLayoutPanel strip = new()
        {
            Dock = DockStyle.Fill,
            ColumnCount = 3,
            RowCount = 1,
            BackColor = _palette.Surface,
            Margin = Padding.Empty
        };
        strip.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
        strip.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.34F));
        strip.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.33F));
        strip.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.33F));

        strip.Controls.Add(CreateBadge(
            "COMUNICACAO\r\n" + _state.CommunicationStatus.ToUpperInvariant(),
            _palette.Text,
            _palette.Field), 0, 0);
        strip.Controls.Add(CreateBadge(
            _state.Mode.DisplayName + "\r\n" + _state.Mode.SafetyMessage,
            _palette.Warning,
            _palette.WarningBackground), 1, 0);
        strip.Controls.Add(CreateBadge(
            $"{_state.PhysicalCommandsExecuted} COMANDOS FISICOS\r\nHOST LOCAL INATIVO",
            _palette.Emergency,
            _palette.EmergencyBackground), 2, 0);
        return strip;
    }

    private Control CreateThemeSelector()
    {
        TableLayoutPanel container = new()
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 2,
            BackColor = _palette.Surface,
            Margin = new Padding(8, 3, 0, 3),
            Padding = new Padding(6, 2, 4, 2)
        };
        container.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        container.RowStyles.Add(new RowStyle(SizeType.Percent, 40F));
        container.RowStyles.Add(new RowStyle(SizeType.Percent, 60F));

        Label caption = CreateLabel("TEMA VISUAL", 7F, FontStyle.Bold, _palette.MutedText);
        caption.BackColor = _palette.Surface;
        container.Controls.Add(caption, 0, 0);

        ComboBox combo = new()
        {
            Dock = DockStyle.Fill,
            DropDownStyle = ComboBoxStyle.DropDownList,
            FlatStyle = FlatStyle.Flat,
            BackColor = _palette.Field,
            ForeColor = _palette.Text,
            Font = new Font("Segoe UI", 8F, FontStyle.Bold),
            Margin = new Padding(0, 2, 0, 2)
        };
        combo.Items.Add(new ThemeOption(Layout3PreviewTheme.Dark, "ESCURO"));
        combo.Items.Add(new ThemeOption(Layout3PreviewTheme.Light, "CLARO"));
        combo.Items.Add(new ThemeOption(Layout3PreviewTheme.Automatic, "AUTOMATICO"));
        combo.SelectedIndex = _theme switch
        {
            Layout3PreviewTheme.Light => 1,
            Layout3PreviewTheme.Automatic => 2,
            _ => 0
        };
        combo.SelectedIndexChanged += (_, _) =>
        {
            // Defer the rebuild: the handler is raised from the combo that is about
            // to be disposed while the visual surfaces are recreated for the new theme.
            if (combo.SelectedItem is ThemeOption option && option.Theme != _theme)
            {
                BeginInvoke(new Action(() => ApplyTheme(option.Theme)));
            }
        };
        container.Controls.Add(combo, 0, 1);
        return container;
    }

    private sealed record ThemeOption(Layout3PreviewTheme Theme, string DisplayName)
    {
        public override string ToString() => DisplayName;
    }

    private Control CreateIdentity()
    {
        TableLayoutPanel identity = new()
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            RowCount = 1,
            BackColor = _palette.Surface,
            Margin = Padding.Empty
        };
        identity.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 4F));
        identity.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        identity.Controls.Add(new Panel
        {
            Dock = DockStyle.Fill,
            BackColor = _palette.AccentBlue,
            Margin = new Padding(0, 5, 0, 5)
        }, 0, 0);

        Label title = CreateLabel(
            "TESTADOR CLP HI\r\nLAYOUT 3  /  HOST OPERACIONAL READ-ONLY",
            11F,
            FontStyle.Bold,
            _palette.Text);
        title.Padding = new Padding(12, 0, 0, 0);
        identity.Controls.Add(title, 1, 0);
        return identity;
    }

    private Control CreateBadge(string text, Color color, Color background)
    {
        Label badge = CreateLabel(text, 8.2F, FontStyle.Bold, color, ContentAlignment.MiddleCenter);
        badge.BackColor = background;
        badge.Margin = new Padding(5, 3, 5, 3);
        return badge;
    }

    private Control CreateMainArea(HardwareCatalog hardwareCatalog)
    {
        TableLayoutPanel main = new()
        {
            Dock = DockStyle.Fill,
            ColumnCount = 3,
            RowCount = 1,
            Margin = new Padding(0, 8, 0, 8),
            BackColor = _palette.Background
        };
        main.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 26F));
        main.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 44F));
        main.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 30F));

        Control connection = CreateConnectionArea();
        connection.Margin = new Padding(0, 0, 6, 0);
        main.Controls.Add(connection, 0, 0);

        main.Controls.Add(new Layout3IoPanelControl(_palette)
        {
            Dock = DockStyle.Fill,
            Margin = new Padding(2, 0, 6, 0)
        }, 1, 0);

        Layout3HostProfileSelectionControl profileSelection = new(
            hardwareCatalog,
            _palette,
            "READ-ONLY / 0 COMANDOS FISICOS")
        {
            Dock = DockStyle.Fill,
            Margin = new Padding(2, 0, 0, 0)
        };
        profileSelection.SelectionLogged += AppendLocalLog;
        main.Controls.Add(profileSelection, 2, 0);

        return main;
    }

    private Control CreateConnectionArea()
    {
        Panel panel = CreateSurfacePanel();
        panel.Padding = new Padding(14);

        TableLayoutPanel layout = new()
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 8,
            BackColor = _palette.Surface
        };
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 48F));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 52F));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 46F));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 46F));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 46F));
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 42F));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 42F));

        layout.Controls.Add(CreateSectionHeader(), 0, 0);
        layout.Controls.Add(CreateStatusField("COMUNICACAO", _state.CommunicationStatus), 0, 1);
        layout.Controls.Add(CreateStatusField("ENTRADAS", _state.Io.InputStatus), 0, 2);
        layout.Controls.Add(CreateStatusField("SAIDAS", _state.Io.OutputStatus), 0, 3);
        layout.Controls.Add(CreateStatusField(
            "CATALOGO",
            _state.Hardware.LocalCatalogAvailable ? "Local carregado" : "Indisponivel"), 0, 4);

        Label note = CreateLabel(
            "Este host nao abre canais de comunicacao. O estado inicial e local, desconectado e inativo.",
            8.5F,
            FontStyle.Regular,
            _palette.MutedText,
            ContentAlignment.MiddleLeft);
        note.Padding = new Padding(3, 8, 3, 8);
        layout.Controls.Add(note, 0, 5);

        Button intentButton = CreateLocalButton("REGISTRAR INTENCAO BLOQUEADA");
        intentButton.Click += (_, _) => RegisterBlockedIntent();
        layout.Controls.Add(intentButton, 0, 6);

        Label guard = CreateLabel(
            "GUARDA READ-ONLY ATIVA",
            8F,
            FontStyle.Bold,
            _palette.Warning,
            ContentAlignment.MiddleCenter);
        guard.BackColor = _palette.WarningBackground;
        guard.Margin = new Padding(0, 5, 0, 0);
        layout.Controls.Add(guard, 0, 7);

        panel.Controls.Add(layout);
        return panel;
    }

    private Control CreateSectionHeader()
    {
        TableLayoutPanel header = new()
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            RowCount = 2,
            BackColor = _palette.Surface
        };
        header.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 34F));
        header.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        header.RowStyles.Add(new RowStyle(SizeType.Percent, 58F));
        header.RowStyles.Add(new RowStyle(SizeType.Percent, 42F));

        Label index = CreateLabel("01", 8F, FontStyle.Bold, _palette.AccentBlue, ContentAlignment.MiddleCenter);
        index.BackColor = _palette.Field;
        index.Margin = new Padding(0, 4, 7, 4);
        header.Controls.Add(index, 0, 0);
        header.SetRowSpan(index, 2);
        header.Controls.Add(CreateLabel("HOST / ESTADO LOCAL", 10F, FontStyle.Bold, _palette.Text), 1, 0);
        header.Controls.Add(CreateLabel("Inicio seguro sem hardware", 8F, FontStyle.Regular, _palette.MutedText), 1, 1);
        return header;
    }

    private Control CreateStatusField(string title, string value)
    {
        TableLayoutPanel field = new()
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 2,
            BackColor = _palette.Field,
            Margin = new Padding(0, 3, 0, 3),
            Padding = new Padding(10, 2, 10, 2)
        };
        field.RowStyles.Add(new RowStyle(SizeType.Percent, 42F));
        field.RowStyles.Add(new RowStyle(SizeType.Percent, 58F));
        Label titleLabel = CreateLabel(title, 7F, FontStyle.Bold, _palette.MutedText);
        Label valueLabel = CreateLabel(value.ToUpperInvariant(), 8.5F, FontStyle.Bold, _palette.Text);
        titleLabel.BackColor = _palette.Field;
        valueLabel.BackColor = _palette.Field;
        field.Controls.Add(titleLabel, 0, 0);
        field.Controls.Add(valueLabel, 0, 1);
        return field;
    }

    private Control CreateTerminalArea()
    {
        Panel panel = CreateSurfacePanel();
        panel.Padding = new Padding(12, 6, 12, 8);

        TableLayoutPanel layout = new()
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 2,
            BackColor = _palette.Surface
        };
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 28F));
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));

        TableLayoutPanel header = new()
        {
            Dock = DockStyle.Fill,
            ColumnCount = 3,
            RowCount = 1,
            BackColor = _palette.Surface
        };
        header.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 150F));
        header.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        header.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 100F));
        header.Controls.Add(CreateLabel("LOG LOCAL DO HOST", 9F, FontStyle.Bold, _palette.Text), 0, 0);
        header.Controls.Add(CreateLabel(
            "MODO READ-ONLY  |  SEM ESCRITA FISICA  |  0 COMANDOS FISICOS",
            8.5F,
            FontStyle.Bold,
            _palette.Warning,
            ContentAlignment.MiddleCenter), 1, 0);

        Button clearButton = CreateLocalButton("LIMPAR LOG");
        clearButton.Click += (_, _) => _localLogTextBox.Clear();
        header.Controls.Add(clearButton, 2, 0);

        layout.Controls.Add(header, 0, 0);
        layout.Controls.Add(_localLogTextBox, 0, 1);
        panel.Controls.Add(layout);
        return panel;
    }

    private TextBox CreateLocalLogTextBox(string? preservedText = null)
    {
        string initialLog = preservedText ?? string.Join(
            "\r\n",
            _state.LocalEvents.Select(message => $"[LOCAL] {message}"));

        return new TextBox
        {
            Dock = DockStyle.Fill,
            Multiline = true,
            ReadOnly = true,
            ScrollBars = ScrollBars.Vertical,
            BackColor = _palette.TerminalBackground,
            ForeColor = _palette.TerminalText,
            BorderStyle = BorderStyle.None,
            Font = new Font("Consolas", 8.5F),
            Text = initialLog
        };
    }

    private void RegisterBlockedIntent()
    {
        Layout3CommandIntent intent = Layout3CommandIntent.Create(
            action: "Solicitar conexao",
            target: "Host Layout 3",
            origin: "Botao local de auditoria");
        Layout3CommandDecision decision = _commandGuard.Evaluate(intent);

        string result = decision.Allowed
            ? "Decisao inesperada da guarda ignorada; este host nao possui executor fisico."
            : decision.Message;
        AppendLocalLog(
            $"[INTENCAO {intent.CorrelationId:N}] {intent.Action} / {intent.Target}. {result}");
    }

    private void AppendLocalLog(string message)
    {
        if (_localLogTextBox.TextLength > 0)
        {
            _localLogTextBox.AppendText("\r\n");
        }
        _localLogTextBox.AppendText($"[LOCAL {DateTime.Now:HH:mm:ss}] {message}");
    }

    private Panel CreateSurfacePanel()
    {
        return new Panel
        {
            Dock = DockStyle.Fill,
            BackColor = _palette.Surface,
            BorderStyle = BorderStyle.FixedSingle
        };
    }

    private Label CreateLabel(
        string text,
        float size,
        FontStyle style,
        Color color,
        ContentAlignment alignment = ContentAlignment.MiddleLeft)
    {
        return new Label
        {
            Text = text,
            Dock = DockStyle.Fill,
            Font = new Font("Segoe UI", size, style),
            ForeColor = color,
            BackColor = _palette.Surface,
            TextAlign = alignment,
            AutoEllipsis = true
        };
    }

    private Button CreateLocalButton(string text)
    {
        Button button = new()
        {
            Text = text,
            Dock = DockStyle.Fill,
            FlatStyle = FlatStyle.Flat,
            ForeColor = _palette.Text,
            BackColor = _palette.Raised,
            Font = new Font("Segoe UI", 7.5F, FontStyle.Bold),
            Margin = new Padding(0, 3, 0, 3),
            Cursor = Cursors.Hand,
            UseVisualStyleBackColor = false
        };
        button.FlatAppearance.BorderColor = _palette.Divider;
        button.FlatAppearance.BorderSize = 1;
        button.FlatAppearance.MouseOverBackColor = _palette.ButtonHover;
        return button;
    }

    private void ResizeContentToViewport()
    {
        Size desiredSize = new(
            Math.Max(ClientSize.Width, MinimumContentWidth),
            Math.Max(ClientSize.Height, MinimumContentHeight));

        if (_content.Size == desiredSize && _content.Location == Point.Empty)
        {
            return;
        }

        _content.Bounds = new Rectangle(Point.Empty, desiredSize);
    }
}
