using TestadorCLPHI.App.Industrial.Platform.Integration;
using TestadorCLPHI.App.Industrial.Platform.Simulation;

namespace TestadorCLPHI.App.Ui.Industrial.Platform;

internal sealed class IndustrialSimulatorControl : UserControl
{
    private readonly IndustrialPlatformSession _session;
    private readonly ComboBox _profiles = new() { DropDownStyle = ComboBoxStyle.DropDownList, Width = 220 };
    private readonly ComboBox _scenarios = new() { DropDownStyle = ComboBoxStyle.DropDownList, Width = 220 };
    private readonly Label _state = PlatformUi.Label(string.Empty, heading: true);
    private readonly Label _alarms = PlatformUi.Label(string.Empty);
    private readonly Label _counters = PlatformUi.Label(string.Empty);
    private readonly FlowLayoutPanel _signals = new() { Dock = DockStyle.Fill, AutoScroll = true, FlowDirection = FlowDirection.TopDown, WrapContents = false };
    private bool _initializing;

    internal IndustrialSimulatorControl(IndustrialPlatformSession session)
    {
        _session = session ?? throw new ArgumentNullException(nameof(session));
        Name = "industrialSimulatorControl";
        BackColor = PlatformUi.Background;
        ForeColor = PlatformUi.Text;
        _initializing = true;
        foreach (string id in SimulationProfileLoader.AvailableProfileIds())
        {
            _profiles.Items.Add(id);
        }

        _profiles.SelectedItem = _session.Profile.Id;
        foreach (SimulationScenario scenario in _session.Profile.Scenarios)
        {
            _scenarios.Items.Add(new ScenarioItem(scenario.Id!, scenario.DisplayName!));
        }

        if (_scenarios.Items.Count > 0)
        {
            _scenarios.SelectedIndex = 0;
        }

        Controls.Add(BuildLayout());
        BuildSignals();
        RefreshSnapshot();
        _session.Changed += SessionChanged;
        _profiles.SelectedIndexChanged += ProfileChanged;
        _initializing = false;
    }

    internal event EventHandler<string>? ProfileRequested;
    internal bool HasPhysicalTransport => false;

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _session.Changed -= SessionChanged;
            _profiles.SelectedIndexChanged -= ProfileChanged;
        }

        base.Dispose(disposing);
    }

    private Control BuildLayout()
    {
        TableLayoutPanel root = new()
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            RowCount = 1,
            BackColor = PlatformUi.Background
        };
        root.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 300F));
        root.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        root.Controls.Add(BuildSidebar(), 0, 0);
        root.Controls.Add(BuildProcessPanel(), 1, 0);
        return root;
    }

    private Control BuildSidebar()
    {
        FlowLayoutPanel sidebar = new()
        {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.TopDown,
            WrapContents = false,
            Padding = new Padding(12),
            BackColor = PlatformUi.Surface
        };
        PlatformUi.StyleField(_profiles);
        PlatformUi.StyleField(_scenarios);
        sidebar.Controls.Add(PlatformUi.Label("Perfil", heading: true));
        sidebar.Controls.Add(_profiles);
        sidebar.Controls.Add(PlatformUi.Label("Cenario", heading: true));
        sidebar.Controls.Add(_scenarios);
        Button apply = PlatformUi.Button("Aplicar cenario", "applyScenarioButton", primary: true);
        Button reset = PlatformUi.Button("Resetar", "resetScenarioButton");
        apply.Width = reset.Width = 220;
        apply.Margin = new Padding(3, 12, 3, 3);
        apply.Click += (_, _) => ApplyScenario();
        reset.Click += (_, _) => _session.ResetSimulation();
        sidebar.Controls.Add(apply);
        sidebar.Controls.Add(reset);
        sidebar.Controls.Add(PlatformUi.Label("Estado", heading: true));
        _state.Width = 240;
        sidebar.Controls.Add(_state);
        sidebar.Controls.Add(PlatformUi.Label("Alarmes", heading: true));
        _alarms.Width = 240;
        _alarms.Height = 80;
        _alarms.AutoSize = false;
        sidebar.Controls.Add(_alarms);
        _counters.Width = 250;
        _counters.Height = 80;
        _counters.AutoSize = false;
        sidebar.Controls.Add(_counters);
        return sidebar;
    }

    private Control BuildProcessPanel()
    {
        TableLayoutPanel panel = new() { Dock = DockStyle.Fill, RowCount = 2, ColumnCount = 1, Padding = new Padding(14) };
        panel.RowStyles.Add(new RowStyle(SizeType.Absolute, 56F));
        panel.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
        Label header = PlatformUi.Label(_session.Profile.DisplayName ?? _session.Profile.Id!, heading: true);
        header.Font = new Font("Segoe UI Semibold", 17F);
        header.Dock = DockStyle.Fill;
        _signals.BackColor = PlatformUi.Background;
        panel.Controls.Add(header, 0, 0);
        panel.Controls.Add(_signals, 0, 1);
        return panel;
    }

    private void BuildSignals()
    {
        _signals.Controls.Clear();
        foreach (SimulationSignalDefinition definition in _session.Profile.Signals)
        {
            _signals.Controls.Add(definition.Kind switch
            {
                SimulationSignalKind.DigitalInput => BuildDigitalSignal(definition),
                SimulationSignalKind.AnalogInput => BuildAnalogSignal(definition),
                SimulationSignalKind.VirtualOutput => BuildOutputSignal(definition),
                _ => throw new ArgumentOutOfRangeException()
            });
        }
    }

    private Control BuildDigitalSignal(SimulationSignalDefinition definition)
    {
        CheckBox value = new()
        {
            Text = definition.Label,
            Name = $"simulation{definition.Id}Input",
            Checked = _session.Simulation.GetValue(definition.Id!) != 0,
            Width = 500,
            AutoSize = false,
            Height = 30,
            ForeColor = PlatformUi.Text
        };
        value.CheckedChanged += (_, _) => _session.SetDigitalInput(definition.Id!, value.Checked);
        return SignalSurface(value);
    }

    private Control BuildAnalogSignal(SimulationSignalDefinition definition)
    {
        FlowLayoutPanel row = new() { Dock = DockStyle.Fill, WrapContents = false };
        Label label = PlatformUi.Label(definition.Label ?? definition.Id!, heading: true);
        label.Width = 220;
        NumericUpDown value = new()
        {
            Name = $"simulation{definition.Id}Input",
            Minimum = (decimal)definition.Minimum,
            Maximum = (decimal)definition.Maximum,
            DecimalPlaces = 2,
            Increment = 1,
            Value = (decimal)_session.Simulation.GetValue(definition.Id!),
            Width = 160
        };
        PlatformUi.StyleField(value);
        value.ValueChanged += (_, _) => _session.SetAnalogInput(definition.Id!, (double)value.Value);
        row.Controls.Add(label);
        row.Controls.Add(value);
        row.Controls.Add(PlatformUi.Label(string.IsNullOrWhiteSpace(definition.Unit) ? "raw" : definition.Unit!));
        return SignalSurface(row);
    }

    private Control BuildOutputSignal(SimulationSignalDefinition definition)
    {
        FlowLayoutPanel row = new() { Dock = DockStyle.Fill, WrapContents = false };
        Label label = PlatformUi.Label(definition.Label ?? definition.Id!, heading: true);
        label.Width = 220;
        Label value = PlatformUi.Label(_session.Simulation.GetValue(definition.Id!) != 0 ? "ON" : "OFF");
        value.Name = $"simulation{definition.Id}Output";
        value.Width = 160;
        value.ForeColor = PlatformUi.Success;
        row.Controls.Add(label);
        row.Controls.Add(value);
        return SignalSurface(row);
    }

    private static Control SignalSurface(Control content)
    {
        Panel surface = new()
        {
            Width = 760,
            Height = 48,
            BackColor = PlatformUi.Surface,
            Padding = new Padding(12, 7, 12, 7),
            Margin = new Padding(3, 3, 3, 7)
        };
        content.Dock = DockStyle.Fill;
        surface.Controls.Add(content);
        return surface;
    }

    private void ApplyScenario()
    {
        if (_scenarios.SelectedItem is ScenarioItem scenario)
        {
            _session.ApplyScenario(scenario.Id);
            BuildSignals();
        }
    }

    private void ProfileChanged(object? sender, EventArgs e)
    {
        if (!_initializing && _profiles.SelectedItem is string profileId
            && !string.Equals(profileId, _session.Profile.Id, StringComparison.OrdinalIgnoreCase))
        {
            ProfileRequested?.Invoke(this, profileId);
        }
    }

    private void SessionChanged(object? sender, EventArgs e) => RefreshSnapshot();

    private void RefreshSnapshot()
    {
        SimulationSnapshot snapshot = _session.Simulation.Snapshot;
        _state.Text = snapshot.State;
        _state.ForeColor = snapshot.OutputsBlocked ? PlatformUi.Danger : PlatformUi.Success;
        _alarms.Text = snapshot.ActiveAlarms.Count == 0 ? "Nenhum" : string.Join(Environment.NewLine, snapshot.ActiveAlarms);
        _counters.Text = $"Operacoes simuladas: {_session.Simulation.Counters.SimulatedOperations}\r\n"
            + $"Comandos simulados: {_session.Simulation.Counters.SimulatedCommands}\r\n"
            + "Operacoes fisicas: 0";

        foreach (SimulationSignalDefinition definition in _session.Profile.Signals.Where(signal =>
                     signal.Kind == SimulationSignalKind.VirtualOutput))
        {
            Control[] controls = Controls.Find($"simulation{definition.Id}Output", searchAllChildren: true);
            if (controls.FirstOrDefault() is Label value)
            {
                value.Text = _session.Simulation.GetValue(definition.Id!) != 0 ? "ON" : "OFF";
            }
        }
    }

    private sealed record ScenarioItem(string Id, string DisplayName)
    {
        public override string ToString() => DisplayName;
    }
}
