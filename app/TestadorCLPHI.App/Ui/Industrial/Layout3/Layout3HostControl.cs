using TestadorCLPHI.App.Hardware;

namespace TestadorCLPHI.App.Ui.Industrial.Layout3;

internal sealed class Layout3HostControl : UserControl
{
    private const int MinimumContentWidth = 860;
    private const int MinimumContentHeight = 930;
    private const int CompactHeaderBreakpoint = 1080;

    private readonly Layout3HostState _state;
    private readonly ILayout3CommandGuard _commandGuard;
    private readonly ILayout3ReadBridge _readBridge;
    private readonly ILayout3ReadBridgeActivationGate _activationGate;
    private readonly HardwareCatalog _hardwareCatalog;
    private readonly Panel _viewport;

    private Layout3ThemePalette _palette;
    private Layout3PreviewTheme _theme;
    private Layout3CommunicationState _communicationState;
    private Layout3ReadBridgeSnapshot _readBridgeSnapshot;
    private Layout3ReadBridgeActivationDecision _activationDecision;
    private TableLayoutPanel _content;
    private TextBox _localLogTextBox;
    private Label? _communicationBadge;
    private Layout3HostCommunicationPanelControl? _communicationPanel;
    private Layout3ReadBridgePanelControl? _readBridgePanel;
    private Layout3ReadBridgeActivationGatePanelControl? _activationPanel;

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
        _communicationState = state.Communication;
        _readBridge = new Layout3DisabledReadBridge();
        _readBridgeSnapshot = _readBridge.CreateSnapshot(state.Profile);
        _activationGate = new Layout3BlockedReadBridgeActivationGate();
        _activationDecision = _activationGate.Evaluate(state.Profile);

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
            RowCount = 5,
            BackColor = _palette.Background,
            Padding = new Padding(12),
            Margin = Padding.Empty
        };
        content.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        content.RowStyles.Add(new RowStyle(SizeType.Absolute, 104F));
        content.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
        content.RowStyles.Add(new RowStyle(SizeType.Absolute, 162F));
        content.RowStyles.Add(new RowStyle(SizeType.Absolute, 122F));
        content.RowStyles.Add(new RowStyle(SizeType.Absolute, 136F));

        content.Controls.Add(CreateTopBar(), 0, 0);
        content.Controls.Add(CreateMainArea(hardwareCatalog), 0, 1);
        content.Controls.Add(CreateReadBridgeArea(), 0, 2);
        content.Controls.Add(CreateActivationGateArea(), 0, 3);
        content.Controls.Add(CreateTerminalArea(), 0, 4);
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

        Control identity = CreateIdentity();
        Control indicators = CreateIndicatorStrip();
        Control themeSelector = CreateThemeSelector();
        layout.Controls.Add(identity, 0, 0);
        layout.Controls.Add(indicators, 1, 0);
        layout.Controls.Add(themeSelector, 2, 0);

        bool? compactLayout = null;
        void UpdateLayout()
        {
            bool compact = layout.ClientSize.Width < CompactHeaderBreakpoint;
            if (compactLayout == compact)
            {
                return;
            }

            compactLayout = compact;
            ConfigureTopBarLayout(layout, identity, indicators, themeSelector, compact);
        }

        layout.ClientSizeChanged += (_, _) => UpdateLayout();
        UpdateLayout();

        panel.Controls.Add(layout);
        return panel;
    }

    private static void ConfigureTopBarLayout(
        TableLayoutPanel layout,
        Control identity,
        Control indicators,
        Control themeSelector,
        bool compact)
    {
        layout.SuspendLayout();
        try
        {
            layout.ColumnStyles.Clear();
            layout.RowStyles.Clear();

            if (compact)
            {
                layout.ColumnCount = 2;
                layout.RowCount = 2;
                layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
                layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 150F));
                layout.RowStyles.Add(new RowStyle(SizeType.Percent, 52F));
                layout.RowStyles.Add(new RowStyle(SizeType.Percent, 48F));

                layout.SetCellPosition(identity, new TableLayoutPanelCellPosition(0, 0));
                layout.SetCellPosition(themeSelector, new TableLayoutPanelCellPosition(1, 0));
                layout.SetCellPosition(indicators, new TableLayoutPanelCellPosition(0, 1));
                layout.SetColumnSpan(indicators, 2);
            }
            else
            {
                layout.ColumnCount = 3;
                layout.RowCount = 1;
                layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 280F));
                layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
                layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 160F));
                layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));

                layout.SetColumnSpan(indicators, 1);
                layout.SetCellPosition(identity, new TableLayoutPanelCellPosition(0, 0));
                layout.SetCellPosition(indicators, new TableLayoutPanelCellPosition(1, 0));
                layout.SetCellPosition(themeSelector, new TableLayoutPanelCellPosition(2, 0));
            }
        }
        finally
        {
            layout.ResumeLayout(performLayout: true);
        }
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

        _communicationBadge = CreateBadge(
            "COMUNICACAO\r\n" + _communicationState.StatusDisplayName.ToUpperInvariant(),
            _palette.Text,
            _palette.Field);
        strip.Controls.Add(_communicationBadge, 0, 0);
        strip.Controls.Add(CreateBadge(
            _state.Mode.DisplayName + "\r\n" + _state.Mode.SafetyMessage,
            _palette.Warning,
            _palette.WarningBackground), 1, 0);
        strip.Controls.Add(CreateBadge(
            $"{_state.PhysicalCommandsExecuted} COMANDOS FISICOS\r\nSEM CONEXAO FISICA",
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
        identity.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
        Panel accent = new()
        {
            Dock = DockStyle.Fill,
            BackColor = _palette.AccentBlue,
            Margin = new Padding(0, 5, 0, 5)
        };
        identity.Controls.Add(accent, 0, 0);

        Label title = CreateLabel(
            "TESTADOR CLP HI",
            11.5F,
            FontStyle.Bold,
            _palette.Text);
        title.Padding = new Padding(12, 0, 4, 0);
        identity.Controls.Add(title, 1, 0);
        return identity;
    }

    private Label CreateBadge(string text, Color color, Color background)
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

        _communicationPanel = new Layout3HostCommunicationPanelControl(_palette, _communicationState)
        {
            Dock = DockStyle.Fill
        };
        _communicationPanel.BlockedIntentRequested += RegisterBlockedIntent;
        Control connection = _communicationPanel;
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
        profileSelection.SelectionChanged += RecalculateCommunicationState;
        profileSelection.SelectionLogged += AppendLocalLog;
        SynchronizeCommunicationState(profileSelection.CurrentSelection, writeLog: false);
        main.Controls.Add(profileSelection, 2, 0);

        return main;
    }

    private Control CreateReadBridgeArea()
    {
        _readBridgePanel = new Layout3ReadBridgePanelControl(_palette, _readBridgeSnapshot)
        {
            Dock = DockStyle.Fill,
            Margin = new Padding(0, 0, 0, 8)
        };
        return _readBridgePanel;
    }

    private Control CreateActivationGateArea()
    {
        _activationPanel = new Layout3ReadBridgeActivationGatePanelControl(_palette, _activationDecision)
        {
            Dock = DockStyle.Fill,
            Margin = new Padding(0, 0, 0, 8)
        };
        return _activationPanel;
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
        header.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 176F));
        header.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        header.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 100F));
        header.Controls.Add(CreateLabel("06  LOG LOCAL DO HOST", 9F, FontStyle.Bold, _palette.Text), 0, 0);
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
        _communicationState = _communicationState.AsBlocked(
            "Intencao bloqueada pela guarda; nenhuma conexao criada.");
        RefreshCommunicationDisplay();
        AppendLocalLog(
            $"[INTENCAO {intent.CorrelationId:N}] {intent.Action} / {intent.Target}. {result}");
    }

    private void RecalculateCommunicationState(Layout3ProfileSelection selection)
    {
        SynchronizeCommunicationState(selection, writeLog: true);
    }

    private void SynchronizeCommunicationState(Layout3ProfileSelection selection, bool writeLog)
    {
        _communicationState = Layout3CommunicationState.FromSelection(selection);
        // O bridge permanece um no-op: o snapshot apenas recebe o perfil como
        // contexto local; nenhuma conexao e aberta e nenhuma leitura e feita.
        _readBridgeSnapshot = _readBridge.CreateSnapshot(selection);
        // O gate permanece bloqueado: a decisao apenas recebe o perfil como
        // contexto local; nenhum perfil libera ativacao, conexao ou leitura.
        _activationDecision = _activationGate.Evaluate(selection);
        RefreshCommunicationDisplay();
        if (writeLog)
        {
            AppendLocalLog(
                $"Estado de comunicacao recalculado: {_communicationState.StatusDisplayName}; " +
                $"origem {_communicationState.Origin}; perfil {_communicationState.ProfileProtocol}. " +
                "Nenhuma conexao fisica foi criada; tentativas reais: 0; comandos fisicos: 0.");
            AppendLocalLog(
                $"Bridge de leitura: {_readBridgeSnapshot.StatusDisplayName} " +
                $"({_readBridgeSnapshot.Mode}); conexao ativa: {_readBridgeSnapshot.ConnectionActiveDisplay}; " +
                $"leituras reais: {_readBridgeSnapshot.RealReads}; escritas reais: {_readBridgeSnapshot.RealWrites}; " +
                $"comandos fisicos: {_readBridgeSnapshot.PhysicalCommands}.");
            AppendLocalLog(
                $"Gate de ativacao: {_activationDecision.StatusDisplayName}; " +
                $"ativacao liberada: {_activationDecision.ActivationAllowedDisplay}; " +
                $"conexao ativa: {_activationDecision.ConnectionActiveDisplay}; " +
                $"requisitos pendentes: {_activationDecision.PendingRequirements}; " +
                $"requisitos atendidos: {_activationDecision.SatisfiedRequirements}.");
        }
    }

    private void RefreshCommunicationDisplay()
    {
        _communicationPanel?.UpdateState(_communicationState);
        _readBridgePanel?.UpdateSnapshot(_readBridgeSnapshot);
        _activationPanel?.UpdateDecision(_activationDecision);
        if (_communicationBadge is not null)
        {
            _communicationBadge.Text =
                "COMUNICACAO\r\n" + _communicationState.StatusDisplayName.ToUpperInvariant();
        }
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
