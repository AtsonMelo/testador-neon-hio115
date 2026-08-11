using TestadorCLPHI.App.Industrial.Platform.Integration;
using TestadorCLPHI.App.Industrial.Platform.Simulation;
using TestadorCLPHI.App.Ui.Theme;

namespace TestadorCLPHI.App.Ui.Industrial.Platform;

internal sealed class IndustrialPlatformForm : Form
{
    private readonly TableLayoutPanel _header = new()
    {
        Dock = DockStyle.Fill,
        ColumnCount = 1,
        RowCount = 2,
        Margin = Padding.Empty,
        Name = "industrialHeader"
    };
    private readonly TableLayoutPanel _shell = new()
    {
        Dock = DockStyle.Fill,
        ColumnCount = 1,
        RowCount = 3
    };
    private readonly TableLayoutPanel _body = new()
    {
        Dock = DockStyle.Fill,
        ColumnCount = 2,
        RowCount = 1,
        Margin = Padding.Empty
    };
    private readonly Panel _content = new()
    {
        Dock = DockStyle.Fill,
        Padding = new Padding(IndustrialSpacing.Md)
    };
    private readonly TableLayoutPanel _statusBar = new()
    {
        Dock = DockStyle.Fill,
        ColumnCount = 5,
        RowCount = 1,
        Margin = Padding.Empty,
        Name = "industrialStatusBar"
    };
    private readonly FlowLayoutPanel _sidebar = new()
    {
        Dock = DockStyle.Fill,
        FlowDirection = FlowDirection.TopDown,
        WrapContents = false,
        Padding = new Padding(IndustrialSpacing.Md),
        AutoScroll = true
    };
    private readonly Button _testerButton = PlatformUi.Button("TESTADOR", "modeTesterButton");
    private readonly Button _simulatorButton = PlatformUi.Button("SIMULADOR", "modeSimulatorButton");
    private readonly Label _offlineStatus = PlatformUi.StatusChip(
        "OFFLINE • EM MEMÓRIA",
        PlatformStatusTone.Offline,
        "platformOfflineStatus");
    private readonly Label _modeStatus = PlatformUi.StatusChip(
        "MODO NÃO SELECIONADO",
        PlatformStatusTone.Disabled,
        "platformModeStatus");
    private readonly Label _safetyStatus = PlatformUi.StatusChip(
        "SAFETYCHAIN • AGUARDANDO",
        PlatformStatusTone.Disabled,
        "platformSafetyStatus");
    private readonly Label _equipmentStatus = new()
    {
        AutoSize = false,
        Dock = DockStyle.Fill,
        TextAlign = ContentAlignment.MiddleLeft,
        AutoEllipsis = true,
        AccessibleName = "Equipamento e perfil ativos"
    };
    private readonly Label _brandTitle = new()
    {
        Name = "industrialBrand",
        Text = "TESTADOR INDUSTRIAL HI",
        AutoSize = false,
        Dock = DockStyle.Fill,
        Margin = Padding.Empty,
        TextAlign = ContentAlignment.MiddleLeft,
        Font = IndustrialTypography.Title(),
        AccessibleName = "Testador Industrial HI"
    };
    private readonly Label _brandContext = new()
    {
        Name = "industrialContext",
        Text = "PLATAFORMA OFFLINE • TRANSPORTE EM MEMÓRIA",
        AutoSize = false,
        Dock = DockStyle.Fill,
        Margin = Padding.Empty,
        TextAlign = ContentAlignment.MiddleLeft,
        Font = IndustrialTypography.Caption()
    };
    private readonly Label _navCaption = new()
    {
        Text = "NAVEGAÇÃO",
        AutoSize = false,
        Height = 30,
        TextAlign = ContentAlignment.MiddleLeft
    };
    private readonly Label _sidebarMode = PlatformUi.StatusChip(
        "OFFLINE",
        PlatformStatusTone.Offline,
        "sidebarOfflineStatus");
    private readonly ComboBox _themeSelector = new()
    {
        Name = "industrialThemeSelector",
        Dock = DockStyle.Fill,
        DropDownStyle = ComboBoxStyle.DropDownList,
        AccessibleName = "Tema da interface",
        AccessibleDescription = "Seleciona tema escuro, claro ou do Windows",
        TabStop = true
    };
    private readonly ToolTip _toolTip = new();
    private readonly Label _profileFooter = BuildFooterLabel("Perfil: aguardando");
    private readonly Label _transportFooter = BuildFooterLabel("Transporte: em memória");
    private readonly Label _addressFooter = BuildFooterLabel("Endereço: 1");
    private readonly Label _safetyFooter = BuildFooterLabel("Física: bloqueada");
    private readonly Label _versionFooter = BuildFooterLabel(".NET 10 • UI2");
    private IndustrialPlatformSession? _session;
    private IndustrialTesterControl? _tester;
    private IndustrialSimulatorControl? _simulator;
    private Control? _welcome;
    private TableLayoutPanel? _welcomeLayout;
    private Control? _welcomeHeader;
    private Control? _welcomeTesterCard;
    private Control? _welcomeSimulatorCard;
    private Label? _welcomeTesterStatus;
    private Label? _welcomeSimulatorStatus;
    private Button? _activeButton;
    private bool _compactNavigation;

    internal IndustrialPlatformForm()
    {
        Text = "Testador Industrial HI - Plataforma industrial offline";
        Name = "industrialPlatformForm";
        StartPosition = FormStartPosition.CenterScreen;
        MinimumSize = new Size(1024, 680);
        ClientSize = new Size(1366, 768);
        Font = IndustrialTypography.Body();
        AutoScaleMode = AutoScaleMode.Dpi;
        KeyPreview = true;

        _shell.RowStyles.Add(new RowStyle(SizeType.Absolute, IndustrialSpacing.HeaderHeight));
        _shell.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
        _shell.RowStyles.Add(new RowStyle(SizeType.Absolute, IndustrialSpacing.StatusBarHeight));
        _body.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, IndustrialSpacing.SidebarWidth));
        _body.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        _body.Controls.Add(BuildSidebar(), 0, 0);
        _body.Controls.Add(_content, 1, 0);
        _shell.Controls.Add(BuildHeader(), 0, 0);
        _shell.Controls.Add(_body, 0, 1);
        _shell.Controls.Add(BuildStatusBar(), 0, 2);
        Controls.Add(_shell);

        _testerButton.Click += (_, _) => ShowTester();
        _simulatorButton.Click += (_, _) => ShowSimulator();
        _testerButton.TabIndex = 0;
        _testerButton.AccessibleDescription = "Abre leitura, diagnóstico e resultados no ambiente offline";
        _simulatorButton.TabIndex = 1;
        _simulatorButton.AccessibleDescription = "Abre cenários e sinais simulados em memória";
        _themeSelector.TabIndex = 2;
        _themeSelector.Items.AddRange(["Escuro", "Claro", "Windows"]);
        _themeSelector.SelectedIndex = 0;
        _themeSelector.SelectedIndexChanged += ThemeSelectorChanged;
        ClientSizeChanged += (_, _) => UpdateResponsiveLayout();
        DpiChanged += (_, _) => UpdateResponsiveLayout();
        IndustrialTheme.ThemeChanged += IndustrialThemeChanged;
        ShowWelcome();
        ApplyTheme();
        UpdateResponsiveLayout();
    }

    internal Button TesterModeButton => _testerButton;
    internal Button SimulatorModeButton => _simulatorButton;
    internal IndustrialPlatformSession Session => _session ??= CreateSession("pivo-central");
    internal IndustrialTesterControl? TesterInstance => _tester;
    internal IndustrialSimulatorControl? SimulatorInstance => _simulator;
    internal bool IsCompactNavigation => _compactNavigation;
    internal IndustrialThemeMode ThemeMode => IndustrialTheme.Mode;
    internal Control HeaderRegion => _header;
    internal Control BodyRegion => _body;
    internal Control ContentRegion => _content;
    internal Control FooterRegion => _statusBar;
    internal Control ThemeSelector => _themeSelector;
    internal Label BrandTitle => _brandTitle;
    internal Label BrandContext => _brandContext;
    internal IReadOnlyList<Label> CriticalHeaderStatuses =>
        [_offlineStatus, _modeStatus, _safetyStatus];

    internal void SetTheme(IndustrialThemeMode mode)
    {
        int index = mode switch
        {
            IndustrialThemeMode.Dark => 0,
            IndustrialThemeMode.Light => 1,
            IndustrialThemeMode.System => 2,
            _ => 0
        };
        if (_themeSelector.SelectedIndex != index)
        {
            _themeSelector.SelectedIndex = index;
        }
        else
        {
            IndustrialTheme.SetMode(mode);
            ApplyTheme();
        }
    }

    internal void ShowTester()
    {
        _tester ??= new IndustrialTesterControl(Session);
        ShowControl(_tester, _testerButton);
    }

    internal void ShowSimulator()
    {
        if (_simulator is null)
        {
            _simulator = new IndustrialSimulatorControl(Session);
            _simulator.ProfileRequested += ChangeProfile;
        }

        ShowControl(_simulator, _simulatorButton);
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            if (_simulator is not null)
            {
                _simulator.ProfileRequested -= ChangeProfile;
            }

            IndustrialTheme.ThemeChanged -= IndustrialThemeChanged;
            if (_session is not null)
            {
                _session.Changed -= SessionChanged;
            }

            _session?.Dispose();
            _toolTip.Dispose();
        }

        base.Dispose(disposing);
    }

    private Control BuildHeader()
    {
        _header.Padding = new Padding(
            IndustrialSpacing.Lg,
            IndustrialSpacing.Xs,
            IndustrialSpacing.Lg,
            IndustrialSpacing.Xs);
        _header.RowStyles.Add(new RowStyle(SizeType.Percent, 64F));
        _header.RowStyles.Add(new RowStyle(SizeType.Percent, 36F));

        TableLayoutPanel primary = new()
        {
            Dock = DockStyle.Fill,
            ColumnCount = 3,
            RowCount = 1,
            Margin = Padding.Empty,
            Name = "industrialHeaderPrimary"
        };
        primary.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 67F));
        primary.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33F));
        primary.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 124F));

        TableLayoutPanel identity = new()
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 2,
            Margin = Padding.Empty
        };
        identity.RowStyles.Add(new RowStyle(SizeType.Percent, 70F));
        identity.RowStyles.Add(new RowStyle(SizeType.Percent, 30F));
        identity.Controls.Add(_brandTitle, 0, 0);
        identity.Controls.Add(_brandContext, 0, 1);
        foreach (Label status in new[] { _offlineStatus, _modeStatus, _safetyStatus })
        {
            status.AutoSize = false;
            status.Dock = DockStyle.Fill;
            status.Margin = new Padding(IndustrialSpacing.Xs, IndustrialSpacing.Xs, IndustrialSpacing.Xs, 0);
            status.AutoEllipsis = true;
        }
        _equipmentStatus.Margin = new Padding(IndustrialSpacing.Md, 0, IndustrialSpacing.Md, 0);
        _equipmentStatus.Font = IndustrialTypography.CaptionStrong();
        _themeSelector.Margin = new Padding(IndustrialSpacing.Sm, IndustrialSpacing.Xs, 0, IndustrialSpacing.Xs);
        primary.Controls.Add(identity, 0, 0);
        primary.Controls.Add(_equipmentStatus, 1, 0);
        primary.Controls.Add(_themeSelector, 2, 0);

        TableLayoutPanel statuses = new()
        {
            Dock = DockStyle.Fill,
            ColumnCount = 3,
            RowCount = 1,
            Margin = Padding.Empty,
            Name = "industrialHeaderStatuses"
        };
        statuses.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.33F));
        statuses.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.33F));
        statuses.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.34F));
        statuses.Controls.Add(_offlineStatus, 0, 0);
        statuses.Controls.Add(_modeStatus, 1, 0);
        statuses.Controls.Add(_safetyStatus, 2, 0);

        _header.Controls.Add(primary, 0, 0);
        _header.Controls.Add(statuses, 0, 1);
        return _header;
    }

    private Control BuildSidebar()
    {
        _sidebar.Name = "industrialSidebar";
        _navCaption.Width = IndustrialSpacing.SidebarWidth - (IndustrialSpacing.Md * 2);
        _sidebar.Controls.Add(_navCaption);
        foreach (Button button in new[] { _testerButton, _simulatorButton })
        {
            button.Width = IndustrialSpacing.SidebarWidth - (IndustrialSpacing.Md * 2);
            button.Height = IndustrialSpacing.CriticalInteractiveHeight;
            button.Margin = new Padding(0, IndustrialSpacing.Xs, 0, IndustrialSpacing.Xs);
            button.TextAlign = ContentAlignment.MiddleLeft;
            button.Padding = new Padding(IndustrialSpacing.Md, 0, 0, 0);
            _sidebar.Controls.Add(button);
        }

        _sidebarMode.Width = IndustrialSpacing.SidebarWidth - (IndustrialSpacing.Md * 2);
        _sidebarMode.AutoSize = false;
        _sidebarMode.Height = 34;
        _sidebarMode.Margin = new Padding(0, IndustrialSpacing.Xl, 0, 0);
        _sidebar.Controls.Add(_sidebarMode);
        _toolTip.SetToolTip(_testerButton, "Abrir o Testador: leitura, diagnóstico e resultados offline");
        _toolTip.SetToolTip(_simulatorButton, "Abrir o Simulador: cenários e sinais em memória");
        _toolTip.SetToolTip(_sidebarMode, "Nenhuma conexão física está ativa");
        return _sidebar;
    }

    private Control BuildStatusBar()
    {
        _statusBar.Padding = new Padding(IndustrialSpacing.Md, 0, IndustrialSpacing.Md, 0);
        _statusBar.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 29F));
        _statusBar.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25F));
        _statusBar.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 14F));
        _statusBar.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 20F));
        _statusBar.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 12F));
        _statusBar.Controls.Add(_profileFooter, 0, 0);
        _statusBar.Controls.Add(_transportFooter, 1, 0);
        _statusBar.Controls.Add(_addressFooter, 2, 0);
        _statusBar.Controls.Add(_safetyFooter, 3, 0);
        _statusBar.Controls.Add(_versionFooter, 4, 0);
        return _statusBar;
    }

    private void ShowWelcome()
    {
        _welcomeLayout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            RowCount = 2,
            Padding = new Padding(IndustrialSpacing.Xl),
            Name = "industrialWelcome"
        };
        _welcomeHeader = BuildWelcomeHeader();
        _welcomeTesterCard = BuildWelcomeCard(
            "TESTADOR",
            "Diagnóstico, identificação e testes supervisionados usando somente o transporte em memória.",
            "LEITURA • DIAGNÓSTICO",
            "ABRIR TESTADOR",
            "welcomeTesterButton",
            ShowTester,
            out _welcomeTesterStatus);
        _welcomeSimulatorCard = BuildWelcomeCard(
            "SIMULADOR",
            "Simulação offline do processo, das torres, da SafetyChain e dos sinais industriais.",
            "SIMULAÇÃO OFFLINE",
            "ABRIR SIMULADOR",
            "welcomeSimulatorButton",
            ShowSimulator,
            out _welcomeSimulatorStatus);
        _welcomeLayout.Controls.Add(_welcomeHeader, 0, 0);
        _welcomeLayout.Controls.Add(_welcomeTesterCard, 0, 1);
        _welcomeLayout.Controls.Add(_welcomeSimulatorCard, 1, 1);
        _welcomeLayout.SetColumnSpan(_welcomeHeader, 2);
        _welcome = _welcomeLayout;
        _content.Controls.Add(_welcomeLayout);
        UpdateWelcomeLayout(ClientSize.Width * 96F / Math.Max(DeviceDpi, 96));
    }

    private static Control BuildWelcomeHeader()
    {
        TableLayoutPanel header = new()
        {
            Dock = DockStyle.Fill,
            AutoSize = true,
            AutoSizeMode = AutoSizeMode.GrowAndShrink,
            ColumnCount = 1,
            RowCount = 2,
            Margin = new Padding(IndustrialSpacing.Sm)
        };
        Label title = PlatformUi.PageTitle("SELECIONE O MODO DE OPERAÇÃO", "Seleção do modo industrial");
        Label context = PlatformUi.Label(
            "Ambiente estritamente offline. Nenhuma comunicação física está habilitada.");
        context.AutoSize = true;
        context.AccessibleName = "Estado físico bloqueado";
        header.Controls.Add(title, 0, 0);
        header.Controls.Add(context, 0, 1);
        return header;
    }

    private static Control BuildWelcomeCard(
        string titleText,
        string descriptionText,
        string statusText,
        string actionText,
        string actionName,
        Action action,
        out Label status)
    {
        Panel card = PlatformUi.Card($"welcome{titleText}Card");
        card.Dock = DockStyle.Fill;
        card.AccessibleName = $"Modo {titleText}";
        TableLayoutPanel layout = new()
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 4,
            Margin = Padding.Empty
        };
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        Label title = PlatformUi.PageTitle(titleText, $"Modo {titleText}");
        Label description = PlatformUi.Label(descriptionText);
        description.Dock = DockStyle.Fill;
        description.AutoSize = false;
        description.TextAlign = ContentAlignment.TopLeft;
        description.Padding = new Padding(0, IndustrialSpacing.Md, 0, IndustrialSpacing.Md);
        status = PlatformUi.StatusChip(statusText, PlatformStatusTone.Offline, $"welcome{titleText}Status");
        status.Dock = DockStyle.Top;
        status.AutoSize = false;
        status.Height = IndustrialSpacing.FieldHeight;
        Button button = PlatformUi.Button(actionText, actionName, primary: true);
        button.Dock = DockStyle.Top;
        button.Height = IndustrialSpacing.CriticalInteractiveHeight;
        button.Margin = new Padding(0, IndustrialSpacing.Md, 0, 0);
        button.AccessibleDescription = $"Abre o modo {titleText} sem habilitar hardware físico";
        button.Click += (_, _) => action();
        layout.Controls.Add(title, 0, 0);
        layout.Controls.Add(description, 0, 1);
        layout.Controls.Add(status, 0, 2);
        layout.Controls.Add(button, 0, 3);
        card.Controls.Add(layout);
        return card;
    }

    private void ShowControl(Control control, Button activeButton)
    {
        _content.SuspendLayout();
        if (control.Parent is null)
        {
            control.Dock = DockStyle.Fill;
            _content.Controls.Add(control);
        }

        foreach (Control child in _content.Controls)
        {
            child.Visible = ReferenceEquals(child, control);
        }

        control.Dock = DockStyle.Fill;
        control.BringToFront();
        _activeButton = activeButton;
        PlatformUi.StyleButton(_testerButton, selected: ReferenceEquals(activeButton, _testerButton));
        PlatformUi.StyleButton(_simulatorButton, selected: ReferenceEquals(activeButton, _simulatorButton));
        bool testerActive = ReferenceEquals(activeButton, _testerButton);
        PlatformUi.UpdateStatusChip(
            _modeStatus,
            testerActive ? "MODO: TESTADOR" : "MODO: SIMULADOR",
            testerActive ? PlatformStatusTone.Active : PlatformStatusTone.Simulated);
        _modeStatus.AccessibleDescription = testerActive
            ? "Modo Testador ativo"
            : "Modo Simulador ativo";
        RefreshGlobalStatus();
        _content.ResumeLayout();
    }

    private void ChangeProfile(object? sender, string profileId)
    {
        _tester?.Dispose();
        if (_simulator is not null)
        {
            _simulator.ProfileRequested -= ChangeProfile;
            _simulator.Dispose();
        }

        if (_session is not null)
        {
            _session.Changed -= SessionChanged;
            _session.Dispose();
        }

        _session = CreateSession(profileId);
        _tester = null;
        _simulator = new IndustrialSimulatorControl(_session);
        _simulator.ProfileRequested += ChangeProfile;
        ShowControl(_simulator, _simulatorButton);
    }

    private IndustrialPlatformSession CreateSession(string profileId)
    {
        IndustrialPlatformSession session = new(SimulationProfileLoader.Load(profileId));
        session.Changed += SessionChanged;
        RefreshGlobalStatus(session);
        return session;
    }

    private void SessionChanged(object? sender, EventArgs e)
    {
        if (InvokeRequired)
        {
            BeginInvoke(RefreshGlobalStatus);
            return;
        }

        RefreshGlobalStatus();
    }

    private void RefreshGlobalStatus() => RefreshGlobalStatus(_session);

    private void RefreshGlobalStatus(IndustrialPlatformSession? session)
    {
        if (session is null)
        {
            _equipmentStatus.Text = "HIO115 • perfil aguardando";
            _profileFooter.Text = "Perfil: aguardando";
            PlatformUi.UpdateStatusChip(
                _safetyStatus,
                "SAFETYCHAIN • AGUARDANDO",
                PlatformStatusTone.Disabled);
            return;
        }

        _equipmentStatus.Text = $"{session.DeviceProfile.DisplayName}\r\n{session.Profile.DisplayName}";
        _profileFooter.Text = $"Perfil: {session.Profile.DisplayName}";
        SimulationSignalDefinition? safety = session.FindSignal("SafetyChain");
        if (safety is null)
        {
            PlatformUi.UpdateStatusChip(
                _safetyStatus,
                "SAFETYCHAIN • N/A",
                PlatformStatusTone.Disabled);
            return;
        }

        bool closed = session.Simulation.GetValue("SafetyChain") >= 0.5D;
        PlatformUi.UpdateStatusChip(
            _safetyStatus,
            closed ? "SEGURANÇA OK • READ-ONLY" : "CADEIA ABERTA • READ-ONLY",
            closed ? PlatformStatusTone.Normal : PlatformStatusTone.Fault);
        _safetyStatus.AccessibleDescription = closed
            ? "SafetyChain derivada fechada e segura"
            : "SafetyChain derivada aberta; saídas bloqueadas";
    }

    private void ThemeSelectorChanged(object? sender, EventArgs e)
    {
        IndustrialThemeMode mode = _themeSelector.SelectedIndex switch
        {
            1 => IndustrialThemeMode.Light,
            2 => IndustrialThemeMode.System,
            _ => IndustrialThemeMode.Dark
        };
        if (IndustrialTheme.Mode == mode)
        {
            ApplyTheme();
            return;
        }

        IndustrialTheme.SetMode(mode);
    }

    private void IndustrialThemeChanged(object? sender, EventArgs e) => ApplyTheme();

    private void ApplyTheme()
    {
        IndustrialPalette palette = IndustrialTheme.Palette;
        BackColor = palette.Background;
        ForeColor = palette.TextPrimary;
        _shell.BackColor = palette.Background;
        _body.BackColor = palette.Background;
        _content.BackColor = palette.Background;
        _sidebar.BackColor = palette.Surface;
        foreach (Control child in _shell.Controls)
        {
            if (child.Name is "industrialHeader" or "industrialStatusBar")
            {
                child.BackColor = palette.Surface;
            }
        }

        _navCaption.ForeColor = palette.TextMuted;
        _equipmentStatus.ForeColor = palette.TextSecondary;
        _themeSelector.BackColor = palette.Field;
        _themeSelector.ForeColor = palette.TextPrimary;
        foreach (Label footer in new[]
                 {
                     _profileFooter,
                     _transportFooter,
                     _addressFooter,
                     _safetyFooter,
                     _versionFooter
                 })
        {
            footer.ForeColor = palette.TextSecondary;
            footer.BackColor = palette.Surface;
        }

        PlatformUi.UpdateStatusChip(_offlineStatus, "OFFLINE • EM MEMÓRIA", PlatformStatusTone.Offline);
        PlatformUi.UpdateStatusChip(_sidebarMode, _compactNavigation ? "OFF" : "OFFLINE • FÍSICA BLOQUEADA", PlatformStatusTone.Offline);
        PlatformUi.UpdateStatusChip(
            _modeStatus,
            ReferenceEquals(_activeButton, _testerButton)
                ? "MODO: TESTADOR"
                : ReferenceEquals(_activeButton, _simulatorButton)
                    ? "MODO: SIMULADOR"
                    : "MODO NÃO SELECIONADO",
            ReferenceEquals(_activeButton, _testerButton)
                ? PlatformStatusTone.Active
                : ReferenceEquals(_activeButton, _simulatorButton)
                    ? PlatformStatusTone.Simulated
                    : PlatformStatusTone.Disabled);
        PlatformUi.StyleButton(_testerButton, selected: ReferenceEquals(_activeButton, _testerButton));
        PlatformUi.StyleButton(_simulatorButton, selected: ReferenceEquals(_activeButton, _simulatorButton));
        _tester?.ApplyTheme();
        _simulator?.ApplyTheme();
        ApplyWelcomeTheme();
        RefreshGlobalStatus();
        IndustrialTheme.ApplyTitleBar(this);
        Invalidate(true);
    }

    private void UpdateResponsiveLayout()
    {
        float logicalWidth = ClientSize.Width * 96F / Math.Max(DeviceDpi, 96);
        bool compact = logicalWidth < 1180F;
        _compactNavigation = compact;
        int dpi = Math.Max(DeviceDpi, 96);
        _brandContext.Visible = !compact;
        _equipmentStatus.Visible = !compact;
        if (_header.Controls.Find("industrialHeaderPrimary", searchAllChildren: true)
                .FirstOrDefault() is TableLayoutPanel primary)
        {
            primary.ColumnStyles[0].Width = compact ? 100F : 67F;
            primary.ColumnStyles[1].Width = compact ? 0F : 33F;
        }

        HeaderLayoutMetrics headerMetrics = CalculateHeaderLayoutMetrics(dpi, compact);
        _header.RowStyles[0].SizeType = SizeType.Absolute;
        _header.RowStyles[0].Height = headerMetrics.PrimaryHeight;
        _header.RowStyles[1].SizeType = SizeType.Absolute;
        _header.RowStyles[1].Height = headerMetrics.StatusHeight;
        _shell.RowStyles[0].Height = headerMetrics.TotalHeight;
        _shell.RowStyles[2].Height = ScaleLogicalMetric(IndustrialSpacing.StatusBarHeight, dpi);
        int width = compact ? IndustrialSpacing.SidebarCompactWidth : IndustrialSpacing.SidebarWidth;
        _body.ColumnStyles[0].Width = width;
        _sidebar.Padding = compact
            ? new Padding(IndustrialSpacing.Sm)
            : new Padding(IndustrialSpacing.Md);
        _navCaption.Visible = !compact;
        ConfigureNavigationButton(_testerButton, compact ? "T" : "TESTADOR", width);
        ConfigureNavigationButton(_simulatorButton, compact ? "S" : "SIMULADOR", width);
        _sidebarMode.Width = Math.Max(40, width - (_sidebar.Padding.Horizontal));
        _sidebarMode.Text = compact ? "OFF" : "■ OFFLINE • FÍSICA BLOQUEADA";

        UpdateWelcomeLayout(logicalWidth);

        _header.PerformLayout();
        _statusBar.PerformLayout();
    }

    private void UpdateWelcomeLayout(float logicalWidth)
    {
        if (_welcomeLayout is null
            || _welcomeHeader is null
            || _welcomeTesterCard is null
            || _welcomeSimulatorCard is null)
        {
            return;
        }

        bool stacked = logicalWidth < 900F;
        _welcomeLayout.SuspendLayout();
        _welcomeLayout.ColumnStyles.Clear();
        _welcomeLayout.RowStyles.Clear();
        _welcomeLayout.ColumnCount = stacked ? 1 : 2;
        _welcomeLayout.RowCount = stacked ? 3 : 2;
        if (stacked)
        {
            _welcomeLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            _welcomeLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            _welcomeLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 50F));
            _welcomeLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 50F));
            _welcomeLayout.SetCellPosition(_welcomeHeader, new TableLayoutPanelCellPosition(0, 0));
            _welcomeLayout.SetColumnSpan(_welcomeHeader, 1);
            _welcomeLayout.SetCellPosition(_welcomeTesterCard, new TableLayoutPanelCellPosition(0, 1));
            _welcomeLayout.SetCellPosition(_welcomeSimulatorCard, new TableLayoutPanelCellPosition(0, 2));
        }
        else
        {
            _welcomeLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            _welcomeLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            _welcomeLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            _welcomeLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            _welcomeLayout.SetCellPosition(_welcomeHeader, new TableLayoutPanelCellPosition(0, 0));
            _welcomeLayout.SetColumnSpan(_welcomeHeader, 2);
            _welcomeLayout.SetCellPosition(_welcomeTesterCard, new TableLayoutPanelCellPosition(0, 1));
            _welcomeLayout.SetCellPosition(_welcomeSimulatorCard, new TableLayoutPanelCellPosition(1, 1));
        }

        _welcomeLayout.ResumeLayout(performLayout: true);
    }

    private void ApplyWelcomeTheme()
    {
        if (_welcome is null)
        {
            return;
        }

        IndustrialPalette palette = IndustrialTheme.Palette;
        _welcome.BackColor = palette.Background;
        foreach (Panel card in EnumerateControls(_welcome).OfType<Panel>()
                     .Where(panel => panel.Name.EndsWith("Card", StringComparison.Ordinal)))
        {
            card.BackColor = palette.SurfaceElevated;
            card.ForeColor = palette.TextPrimary;
        }

        foreach (Label label in EnumerateControls(_welcome).OfType<Label>()
                     .Where(label => label.BorderStyle != BorderStyle.FixedSingle))
        {
            label.ForeColor = label.Font.Bold ? palette.TextPrimary : palette.TextSecondary;
        }

        if (_welcomeTesterStatus is not null)
        {
            PlatformUi.UpdateStatusChip(
                _welcomeTesterStatus,
                "LEITURA • DIAGNÓSTICO",
                PlatformStatusTone.Offline);
        }

        if (_welcomeSimulatorStatus is not null)
        {
            PlatformUi.UpdateStatusChip(
                _welcomeSimulatorStatus,
                "SIMULAÇÃO OFFLINE",
                PlatformStatusTone.Simulated);
        }

        foreach (Button button in EnumerateControls(_welcome).OfType<Button>())
        {
            PlatformUi.StyleButton(button, primary: true);
        }
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

    private HeaderLayoutMetrics CalculateHeaderLayoutMetrics(int dpi, bool compact)
    {
        int verticalPadding = ScaleLogicalMetric(_header.Padding.Vertical, dpi);
        int titleHeight = TextRenderer.MeasureText(
            _brandTitle.Text,
            _brandTitle.Font,
            Size.Empty,
            TextFormatFlags.NoPadding | TextFormatFlags.SingleLine).Height;
        int contextHeight = compact
            ? 0
            : TextRenderer.MeasureText(
                _brandContext.Text,
                _brandContext.Font,
                Size.Empty,
                TextFormatFlags.NoPadding | TextFormatFlags.SingleLine).Height;
        int identityHeight = titleHeight + contextHeight;
        int primaryHeight = Math.Max(
            identityHeight,
            Math.Max(
                _themeSelector.PreferredHeight,
                compact ? 0 : _equipmentStatus.GetPreferredSize(Size.Empty).Height));
        int statusHeight = CriticalHeaderStatuses
            .Select(status => Math.Max(status.GetPreferredSize(Size.Empty).Height, status.MinimumSize.Height))
            .DefaultIfEmpty(ScaleLogicalMetric(IndustrialSpacing.InteractiveHeight, dpi))
            .Max();
        int minimum = ScaleLogicalMetric(
            compact ? IndustrialSpacing.HeaderCompactHeight : IndustrialSpacing.HeaderHeight,
            dpi);
        int total = Math.Max(minimum, primaryHeight + statusHeight + verticalPadding);
        return new HeaderLayoutMetrics(primaryHeight, statusHeight, total);
    }

    internal static int ScaleLogicalMetric(int logicalPixels, int dpi) =>
        Math.Max(1, (int)Math.Ceiling(logicalPixels * Math.Max(dpi, 96) / 96D));

    private sealed record HeaderLayoutMetrics(int PrimaryHeight, int StatusHeight, int TotalHeight);

    private void ConfigureNavigationButton(Button button, string text, int sidebarWidth)
    {
        button.Text = text;
        button.Width = Math.Max(40, sidebarWidth - _sidebar.Padding.Horizontal);
        button.TextAlign = _compactNavigation
            ? ContentAlignment.MiddleCenter
            : ContentAlignment.MiddleLeft;
        button.Padding = _compactNavigation
            ? Padding.Empty
            : new Padding(IndustrialSpacing.Md, 0, 0, 0);
    }

    private static Label BuildFooterLabel(string text) => new()
    {
        Text = text,
        Dock = DockStyle.Fill,
        TextAlign = ContentAlignment.MiddleLeft,
        AutoEllipsis = true,
        Font = IndustrialTypography.Caption(),
        Padding = new Padding(IndustrialSpacing.Xs, 0, IndustrialSpacing.Xs, 0)
    };
}

internal enum PlatformStatusTone
{
    Normal,
    Attention,
    Fault,
    Active,
    Disabled,
    Simulated,
    Offline
}

internal static class PlatformUi
{
    internal static Color Background => IndustrialTheme.Palette.Background;
    internal static Color Header => IndustrialTheme.Palette.Surface;
    internal static Color Surface => IndustrialTheme.Palette.SurfaceElevated;
    internal static Color Field => IndustrialTheme.Palette.Field;
    internal static Color ButtonSurface => IndustrialTheme.Palette.SurfaceInteractive;
    internal static Color Accent => IndustrialTheme.Palette.Accent;
    internal static Color Success => IndustrialTheme.Palette.Success;
    internal static Color Warning => IndustrialTheme.Palette.Warning;
    internal static Color Danger => IndustrialTheme.Palette.Danger;
    internal static Color Text => IndustrialTheme.Palette.TextPrimary;
    internal static Color Muted => IndustrialTheme.Palette.TextSecondary;

    internal static Button Button(string text, string name, bool primary = false)
    {
        Button button = new()
        {
            Text = text,
            Name = name,
            Height = IndustrialSpacing.InteractiveHeight,
            AutoSize = false,
            FlatStyle = FlatStyle.Flat,
            BackColor = primary ? Accent : ButtonSurface,
            ForeColor = Text,
            Font = IndustrialTypography.BodyStrong(),
            Cursor = Cursors.Hand,
            AccessibleName = text,
            TabStop = true
        };
        button.FlatAppearance.BorderSize = IndustrialSpacing.BorderWidth;
        button.FlatAppearance.BorderColor = primary
            ? IndustrialTheme.Palette.AccentHover
            : IndustrialTheme.Palette.BorderStrong;
        button.FlatAppearance.MouseOverBackColor = primary
            ? IndustrialTheme.Palette.AccentHover
            : IndustrialTheme.Palette.SurfaceElevated;
        button.FlatAppearance.MouseDownBackColor = primary
            ? IndustrialTheme.Palette.AccentPressed
            : IndustrialTheme.Palette.Surface;
        return button;
    }

    internal static void StyleButton(Button button, bool primary = false, bool selected = false)
    {
        IndustrialPalette palette = IndustrialTheme.Palette;
        button.FlatStyle = FlatStyle.Flat;
        button.ForeColor = selected ? palette.SelectedText : palette.TextPrimary;
        button.BackColor = selected
            ? palette.SelectedSurface
            : primary ? palette.Accent : palette.SurfaceInteractive;
        button.FlatAppearance.BorderSize = IndustrialSpacing.BorderWidth;
        button.FlatAppearance.BorderColor = selected
            ? palette.Accent
            : primary ? palette.AccentHover : palette.BorderStrong;
        button.FlatAppearance.MouseOverBackColor = primary
            ? palette.AccentHover
            : palette.SurfaceElevated;
        button.FlatAppearance.MouseDownBackColor = primary
            ? palette.AccentPressed
            : palette.Surface;
    }

    internal static Label StatusChip(
        string text,
        PlatformStatusTone tone,
        string? name = null)
    {
        Label label = new()
        {
            Name = name ?? string.Empty,
            AutoSize = true,
            Padding = new Padding(
                IndustrialSpacing.Sm,
                IndustrialSpacing.Xs,
                IndustrialSpacing.Sm,
                IndustrialSpacing.Xs),
            Margin = new Padding(IndustrialSpacing.Xs),
            BorderStyle = BorderStyle.FixedSingle,
            Font = IndustrialTypography.CaptionStrong(),
            TextAlign = ContentAlignment.MiddleCenter,
            AccessibleName = text
        };
        UpdateStatusChip(label, text, tone);
        return label;
    }

    internal static void UpdateStatusChip(Label label, string text, PlatformStatusTone tone)
    {
        (string symbol, Color background, Color foreground) = StatusAppearance(tone);
        label.Text = $"{symbol} {text}";
        label.BackColor = background;
        label.ForeColor = foreground;
        label.AccessibleName = text;
    }

    private static (string Symbol, Color Background, Color Foreground) StatusAppearance(
        PlatformStatusTone tone) => tone switch
        {
            PlatformStatusTone.Normal => ("OK", IndustrialTheme.Palette.SuccessSurface, Success),
            PlatformStatusTone.Attention => ("!", IndustrialTheme.Palette.WarningSurface, Warning),
            PlatformStatusTone.Fault => ("X", IndustrialTheme.Palette.DangerSurface, Danger),
            PlatformStatusTone.Active => (">", IndustrialTheme.Palette.SelectedSurface, IndustrialTheme.Palette.SelectedText),
            PlatformStatusTone.Disabled => ("-", Surface, IndustrialTheme.Palette.Disabled),
            PlatformStatusTone.Simulated => ("S", IndustrialTheme.Palette.SelectedSurface, IndustrialTheme.Palette.SelectedText),
            PlatformStatusTone.Offline => ("■", IndustrialTheme.Palette.OfflineSurface, IndustrialTheme.Palette.Offline),
            _ => ("•", ButtonSurface, Text)
        };

    internal static Label Label(string text, bool heading = false) => new()
    {
        Text = text,
        AutoSize = true,
        ForeColor = heading ? Text : Muted,
        Font = heading ? IndustrialTypography.Section() : IndustrialTypography.Body(),
        Margin = new Padding(
            IndustrialSpacing.Xs,
            IndustrialSpacing.Sm,
            IndustrialSpacing.Xs,
            IndustrialSpacing.Xs)
    };

    internal static GroupBox Group(string text) => new()
    {
        Text = text,
        ForeColor = Text,
        BackColor = Surface,
        Padding = new Padding(IndustrialSpacing.Md),
        Margin = new Padding(IndustrialSpacing.Xs)
    };

    internal static Panel Card(string name)
    {
        IndustrialPalette palette = IndustrialTheme.Palette;
        return new Panel
        {
            Name = name,
            BackColor = palette.SurfaceElevated,
            ForeColor = palette.TextPrimary,
            Padding = new Padding(IndustrialSpacing.Xl),
            Margin = new Padding(IndustrialSpacing.Sm),
            MinimumSize = new Size(IndustrialSpacing.CardMinimumWidth, 0),
            AccessibleRole = AccessibleRole.Grouping
        };
    }

    internal static Label PageTitle(string text, string accessibleName) => new()
    {
        Text = text,
        AutoSize = true,
        Font = IndustrialTypography.Title(),
        ForeColor = IndustrialTheme.Palette.TextPrimary,
        Margin = Padding.Empty,
        AccessibleName = accessibleName
    };

    internal static void StyleTabs(TabControl tabs)
    {
        tabs.Font = IndustrialTypography.BodyStrong();
        tabs.ItemSize = new Size(150, IndustrialSpacing.TabHeight);
        tabs.Padding = new Point(IndustrialSpacing.Md, IndustrialSpacing.Xs);
    }

    internal static void StyleField(Control control)
    {
        control.BackColor = Field;
        control.ForeColor = Text;
    }
}
