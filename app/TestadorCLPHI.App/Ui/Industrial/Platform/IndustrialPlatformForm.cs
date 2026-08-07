using TestadorCLPHI.App.Industrial.Platform.Integration;
using TestadorCLPHI.App.Industrial.Platform.Simulation;

namespace TestadorCLPHI.App.Ui.Industrial.Platform;

internal sealed class IndustrialPlatformForm : Form
{
    private readonly Panel _content = new() { Dock = DockStyle.Fill, Padding = new(16) };
    private readonly Button _testerButton = PlatformUi.Button("TESTADOR", "modeTesterButton");
    private readonly Button _simulatorButton = PlatformUi.Button("SIMULADOR", "modeSimulatorButton");
    private IndustrialPlatformSession? _session;
    private IndustrialTesterControl? _tester;
    private IndustrialSimulatorControl? _simulator;

    internal IndustrialPlatformForm()
    {
        Text = "Testador CLP - Plataforma industrial offline";
        Name = "industrialPlatformForm";
        StartPosition = FormStartPosition.CenterScreen;
        MinimumSize = new Size(1080, 720);
        ClientSize = new Size(1360, 840);
        BackColor = PlatformUi.Background;
        ForeColor = PlatformUi.Text;
        Font = new Font("Segoe UI", 9F);

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
            Height = 72,
            Padding = new Padding(18, 12, 18, 10),
            ColumnCount = 3,
            BackColor = PlatformUi.Header
        };
        header.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        header.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 150F));
        header.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 150F));

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
        header.Controls.Add(brand, 0, 0);
        header.Controls.Add(_testerButton, 1, 0);
        header.Controls.Add(_simulatorButton, 2, 0);
        return header;
    }

    private void ShowWelcome()
    {
        TableLayoutPanel welcome = new()
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            RowCount = 2,
            Padding = new Padding(80)
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

internal static class PlatformUi
{
    internal static readonly Color Background = Color.FromArgb(18, 24, 32);
    internal static readonly Color Header = Color.FromArgb(24, 32, 42);
    internal static readonly Color Surface = Color.FromArgb(31, 41, 52);
    internal static readonly Color Field = Color.FromArgb(22, 30, 39);
    internal static readonly Color ButtonSurface = Color.FromArgb(46, 58, 71);
    internal static readonly Color Accent = Color.FromArgb(0, 122, 138);
    internal static readonly Color Success = Color.FromArgb(58, 166, 102);
    internal static readonly Color Warning = Color.FromArgb(225, 163, 46);
    internal static readonly Color Danger = Color.FromArgb(210, 73, 73);
    internal static readonly Color Text = Color.FromArgb(229, 235, 241);
    internal static readonly Color Muted = Color.FromArgb(160, 174, 190);

    internal static Button Button(string text, string name, bool primary = false) => new()
    {
        Text = text,
        Name = name,
        Height = 36,
        AutoSize = false,
        FlatStyle = FlatStyle.Flat,
        BackColor = primary ? Accent : ButtonSurface,
        ForeColor = Text,
        Font = new Font("Segoe UI Semibold", 9F),
        Cursor = Cursors.Hand
    };

    internal static Label Label(string text, bool heading = false) => new()
    {
        Text = text,
        AutoSize = true,
        ForeColor = heading ? Text : Muted,
        Font = heading ? new Font("Segoe UI Semibold", 11F) : new Font("Segoe UI", 9F),
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
