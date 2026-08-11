using TestadorCLPHI.App.Industrial.Platform.Integration;
using TestadorCLPHI.App.Industrial.Platform.Simulation;
using TestadorCLPHI.App.Ui.Theme;

namespace TestadorCLPHI.App.Ui.Industrial.Platform;

internal sealed class IndustrialPlatformForm : Form
{
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
        RowCount = 1
    };
    private readonly Panel _content = new()
    {
        Dock = DockStyle.Fill,
        Padding = new Padding(IndustrialSpacing.Md)
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
        _themeSelector.Items.AddRange(["Escuro", "Claro", "Windows"]);
        _themeSelector.SelectedIndex = 0;
        _themeSelector.SelectedIndexChanged += ThemeSelectorChanged;
        ClientSizeChanged += (_, _) => UpdateResponsiveLayout();
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
        TableLayoutPanel header = new()
        {
            Dock = DockStyle.Fill,
            Padding = new Padding(IndustrialSpacing.Lg, IndustrialSpacing.Md, IndustrialSpacing.Lg, IndustrialSpacing.Sm),
            ColumnCount = 6,
            RowCount = 1,
            Name = "industrialHeader"
        };
        header.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        header.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 230F));
        header.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 150F));
        header.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 170F));
        header.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 188F));
        header.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 112F));

        TableLayoutPanel identity = new()
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 2,
            Margin = Padding.Empty
        };
        identity.RowStyles.Add(new RowStyle(SizeType.Percent, 58F));
        identity.RowStyles.Add(new RowStyle(SizeType.Percent, 42F));
        Label brand = new()
        {
            Text = "TESTADOR INDUSTRIAL HI",
            AutoSize = false,
            Dock = DockStyle.Fill,
            TextAlign = ContentAlignment.BottomLeft,
            Font = IndustrialTypography.Title(),
            AccessibleName = "Testador Industrial HI"
        };
        Label context = new()
        {
            Text = "PLATAFORMA OFFLINE • TRANSPORTE EM MEMÓRIA",
            AutoSize = false,
            Dock = DockStyle.Fill,
            TextAlign = ContentAlignment.TopLeft,
            Font = IndustrialTypography.Caption()
        };
        identity.Controls.Add(brand, 0, 0);
        identity.Controls.Add(context, 0, 1);
        _offlineStatus.Anchor = AnchorStyles.None;
        _modeStatus.Anchor = AnchorStyles.None;
        _safetyStatus.Anchor = AnchorStyles.None;
        header.Controls.Add(identity, 0, 0);
        header.Controls.Add(_equipmentStatus, 1, 0);
        header.Controls.Add(_offlineStatus, 2, 0);
        header.Controls.Add(_modeStatus, 3, 0);
        header.Controls.Add(_safetyStatus, 4, 0);
        header.Controls.Add(_themeSelector, 5, 0);
        return header;
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
        TableLayoutPanel status = new()
        {
            Dock = DockStyle.Fill,
            ColumnCount = 5,
            RowCount = 1,
            Padding = new Padding(IndustrialSpacing.Md, 0, IndustrialSpacing.Md, 0),
            Name = "industrialStatusBar"
        };
        status.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 29F));
        status.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25F));
        status.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 14F));
        status.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 20F));
        status.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 12F));
        status.Controls.Add(_profileFooter, 0, 0);
        status.Controls.Add(_transportFooter, 1, 0);
        status.Controls.Add(_addressFooter, 2, 0);
        status.Controls.Add(_safetyFooter, 3, 0);
        status.Controls.Add(_versionFooter, 4, 0);
        return status;
    }

    private void ShowWelcome()
    {
        TableLayoutPanel welcome = new()
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            RowCount = 2,
            Padding = new Padding(IndustrialSpacing.Xxl)
        };
        welcome.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
        welcome.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
        welcome.RowStyles.Add(new RowStyle(SizeType.Percent, 35F));
        welcome.RowStyles.Add(new RowStyle(SizeType.Percent, 65F));
        Label title = new()
        {
            Text = "Escolha o modo",
            Dock = DockStyle.Fill,
            TextAlign = ContentAlignment.BottomCenter,
            Font = IndustrialTypography.Display()
        };
        welcome.SetColumnSpan(title, 2);
        Button tester = PlatformUi.Button("TESTADOR", "welcomeTesterButton", primary: true);
        Button simulator = PlatformUi.Button("SIMULADOR", "welcomeSimulatorButton", primary: true);
        tester.Margin = new Padding(24);
        simulator.Margin = new Padding(24);
        tester.Dock = DockStyle.Top;
        simulator.Dock = DockStyle.Top;
        tester.Height = 88;
        simulator.Height = 88;
        tester.Click += (_, _) => ShowTester();
        simulator.Click += (_, _) => ShowSimulator();
        welcome.Controls.Add(title, 0, 0);
        welcome.Controls.Add(tester, 0, 1);
        welcome.Controls.Add(simulator, 1, 1);
        _welcome = welcome;
        _content.Controls.Add(welcome);
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
        IndustrialPlatformSession session = new(profileId);
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
        RefreshGlobalStatus();
        IndustrialTheme.ApplyTitleBar(this);
        Invalidate(true);
    }

    private void UpdateResponsiveLayout()
    {
        float logicalWidth = ClientSize.Width * 96F / Math.Max(DeviceDpi, 96);
        bool compact = logicalWidth < 1180F;
        if (_compactNavigation == compact && _body.ColumnStyles[0].Width > 0)
        {
            return;
        }

        _compactNavigation = compact;
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
    }

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
            Padding = new Padding(8, 4, 8, 4),
            Margin = new Padding(4),
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
        Margin = new Padding(3, 6, 3, 4)
    };

    internal static GroupBox Group(string text) => new()
    {
        Text = text,
        ForeColor = Text,
        BackColor = Surface,
        Padding = new Padding(10),
        Margin = new Padding(5)
    };

    internal static void StyleField(Control control)
    {
        control.BackColor = Field;
        control.ForeColor = Text;
    }
}
