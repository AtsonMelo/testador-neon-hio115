using TestadorCLPHI.App.Industrial.Platform.Integration;
using TestadorCLPHI.App.Industrial.Platform.Simulation;
using TestadorCLPHI.App.Ui.Theme;

namespace TestadorCLPHI.App.Ui.Industrial.Platform;

internal sealed class IndustrialPlatformForm : Form
{
    private readonly Panel _content = new() { Dock = DockStyle.Fill, Padding = new(12) };
    private readonly Button _testerButton = PlatformUi.Button("TESTADOR", "modeTesterButton");
    private readonly Button _simulatorButton = PlatformUi.Button("SIMULADOR", "modeSimulatorButton");
    private readonly Label _offlineStatus = PlatformUi.StatusChip(
        "OFFLINE • EM MEMÓRIA",
        PlatformStatusTone.Offline,
        "platformOfflineStatus");
    private readonly Label _modeStatus = PlatformUi.StatusChip(
        "ESCOLHA UM MODO",
        PlatformStatusTone.Disabled,
        "platformModeStatus");
    private IndustrialPlatformSession? _session;
    private IndustrialTesterControl? _tester;
    private IndustrialSimulatorControl? _simulator;

    internal IndustrialPlatformForm()
    {
        Text = "Testador CLP - Plataforma industrial offline";
        Name = "industrialPlatformForm";
        StartPosition = FormStartPosition.CenterScreen;
        MinimumSize = new Size(1024, 680);
        ClientSize = new Size(1366, 768);
        BackColor = PlatformUi.Background;
        ForeColor = PlatformUi.Text;
        Font = new Font("Segoe UI", 9F);
        AutoScaleMode = AutoScaleMode.Dpi;

        Controls.Add(_content);
        Controls.Add(BuildHeader());
        _testerButton.Click += (_, _) => ShowTester();
        _simulatorButton.Click += (_, _) => ShowSimulator();
        ShowWelcome();
    }

    internal Button TesterModeButton => _testerButton;
    internal Button SimulatorModeButton => _simulatorButton;
    internal IndustrialPlatformSession Session => _session ??= new IndustrialPlatformSession("pivo-central");

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

            _session?.Dispose();
        }

        base.Dispose(disposing);
    }

    private Control BuildHeader()
    {
        TableLayoutPanel header = new()
        {
            Dock = DockStyle.Top,
            Height = 68,
            Padding = new Padding(16, 10, 16, 9),
            ColumnCount = 5,
            BackColor = PlatformUi.Header
        };
        header.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        header.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 190F));
        header.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 170F));
        header.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 138F));
        header.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 138F));

        Label brand = new()
        {
            Text = "TESTADOR CLP",
            AutoSize = false,
            Dock = DockStyle.Fill,
            TextAlign = ContentAlignment.MiddleLeft,
            Font = new Font("Segoe UI Semibold", 15F),
            ForeColor = PlatformUi.Text
        };
        _testerButton.Dock = DockStyle.Fill;
        _simulatorButton.Dock = DockStyle.Fill;
        _testerButton.Margin = new Padding(4);
        _simulatorButton.Margin = new Padding(4);
        _offlineStatus.Anchor = AnchorStyles.None;
        _modeStatus.Anchor = AnchorStyles.None;
        header.Controls.Add(brand, 0, 0);
        header.Controls.Add(_offlineStatus, 1, 0);
        header.Controls.Add(_modeStatus, 2, 0);
        header.Controls.Add(_testerButton, 3, 0);
        header.Controls.Add(_simulatorButton, 4, 0);
        return header;
    }

    private void ShowWelcome()
    {
        TableLayoutPanel welcome = new()
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            RowCount = 2,
            Padding = new Padding(48)
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
            Font = new Font("Segoe UI Semibold", 20F),
            ForeColor = PlatformUi.Text
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
        _content.Controls.Add(welcome);
    }

    private void ShowControl(Control control, Button activeButton)
    {
        _content.SuspendLayout();
        _content.Controls.Clear();
        control.Dock = DockStyle.Fill;
        _content.Controls.Add(control);
        _testerButton.BackColor = ReferenceEquals(activeButton, _testerButton) ? PlatformUi.Accent : PlatformUi.ButtonSurface;
        _simulatorButton.BackColor = ReferenceEquals(activeButton, _simulatorButton) ? PlatformUi.Accent : PlatformUi.ButtonSurface;
        bool testerActive = ReferenceEquals(activeButton, _testerButton);
        _modeStatus.Text = testerActive ? "● MODO: TESTADOR" : "● MODO: SIMULADOR";
        _modeStatus.ForeColor = PlatformUi.Text;
        _modeStatus.BackColor = PlatformUi.Accent;
        _modeStatus.AccessibleDescription = testerActive
            ? "Modo Testador ativo"
            : "Modo Simulador ativo";
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

        _session?.Dispose();
        _session = new IndustrialPlatformSession(profileId);
        _tester = null;
        _simulator = new IndustrialSimulatorControl(_session);
        _simulator.ProfileRequested += ChangeProfile;
        ShowControl(_simulator, _simulatorButton);
    }
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

    internal static Label StatusChip(
        string text,
        PlatformStatusTone tone,
        string? name = null)
    {
        (string symbol, Color background, Color foreground) = tone switch
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
        return new Label
        {
            Text = $"{symbol} {text}",
            Name = name ?? string.Empty,
            AutoSize = true,
            Padding = new Padding(8, 4, 8, 4),
            Margin = new Padding(4),
            BorderStyle = BorderStyle.FixedSingle,
            BackColor = background,
            ForeColor = foreground,
            Font = IndustrialTypography.CaptionStrong(),
            TextAlign = ContentAlignment.MiddleCenter,
            AccessibleName = text
        };
    }

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
