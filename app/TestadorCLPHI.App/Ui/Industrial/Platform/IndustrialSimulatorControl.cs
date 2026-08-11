using TestadorCLPHI.App.Industrial.Platform.Integration;
using TestadorCLPHI.App.Industrial.Platform.Process;
using TestadorCLPHI.App.Industrial.Platform.Simulation;
using TestadorCLPHI.App.Ui.Controls;
using TestadorCLPHI.App.Ui.Theme;

namespace TestadorCLPHI.App.Ui.Industrial.Platform;

internal sealed class IndustrialSimulatorControl : UserControl
{
    private readonly IndustrialPlatformSession _session;
    private readonly IndustrialComboBox _profiles = new()
    {
        DropDownStyle = ComboBoxStyle.DropDownList,
        Width = 220,
        AccessibleName = "Perfil de simulação",
        AccessibleDescription = "Seleciona o processo simulado sem habilitar transporte físico"
    };
    private readonly IndustrialComboBox _scenarios = new()
    {
        DropDownStyle = ComboBoxStyle.DropDownList,
        Width = 220,
        AccessibleName = "Cenário de simulação",
        AccessibleDescription = "Seleciona um cenário offline do perfil ativo"
    };
    private readonly Label _state = PlatformUi.StatusChip(
        "AGUARDANDO ESTADO",
        PlatformStatusTone.Disabled,
        "simulationStateStatus");
    private readonly Label _alarms = PlatformUi.Label(string.Empty);
    private readonly Label _counters = PlatformUi.Label(string.Empty);
    private readonly Label _safety = PlatformUi.StatusChip(
        "SAFETYCHAIN • AGUARDANDO",
        PlatformStatusTone.Disabled,
        "simulationSafetyStatus");
    private readonly Label _overviewStatus = PlatformUi.Label(string.Empty, heading: true);
    private readonly IndustrialScrollPanel _signals = new()
    {
        Dock = DockStyle.Fill,
        AutoScroll = true,
        Name = "simulationSignalScrollHost",
        AccessibleName = "Sinais do processo simulados"
    };
    private readonly TableLayoutPanel _signalGroups = new()
    {
        Dock = DockStyle.Top,
        AutoSize = true,
        AutoSizeMode = AutoSizeMode.GrowAndShrink,
        ColumnCount = 1,
        RowCount = 0,
        BackColor = PlatformUi.Background
    };
    private readonly Dictionary<string, Label> _metricValues = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, CheckBox> _digitalEditors = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, NumericUpDown> _analogEditors = new(StringComparer.OrdinalIgnoreCase);
    private readonly List<SignalGroupLayout> _signalLayouts = [];
    private TableLayoutPanel? _rootLayout;
    private FlowLayoutPanel? _configurationSidebar;
    private TableLayoutPanel? _processPanel;
    private TableLayoutPanel? _pivotOverview;
    private TableLayoutPanel? _metricGrid;
    private Label? _profileEvidence;
    private PivotProcessControl? _pivotVisual;
    private bool _initializing;
    private bool _updatingEditors;
    private int _signalStructureBuildCount;

    internal IndustrialSimulatorControl(IndustrialPlatformSession session)
    {
        _session = session ?? throw new ArgumentNullException(nameof(session));
        Name = "industrialSimulatorControl";
        AccessibleName = "Simulador industrial offline";
        AccessibleDescription = "Cenários, sinais editáveis e saídas virtuais executados somente em memória";
        AutoScaleMode = AutoScaleMode.Dpi;
        _signals.BackColor = PlatformUi.Background;
        _signalGroups.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        _signals.Controls.Add(_signalGroups);
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
        ClientSizeChanged += (_, _) => UpdateResponsiveLayout();
        BuildSignals();
        RefreshSnapshot();
        _session.Changed += SessionChanged;
        _profiles.SelectedIndexChanged += ProfileChanged;
        _initializing = false;
        ApplyTheme();
        UpdateResponsiveLayout();
    }

    internal event EventHandler<string>? ProfileRequested;
    internal bool HasPhysicalTransport => false;
    internal bool HasPivotRenderer => _pivotVisual is not null;
    internal int RenderedTowerCount => _pivotVisual?.TowerCount ?? 0;
    internal int PivotStateRevision => _pivotVisual?.StateRevision ?? 0;
    internal bool UsesContinuousAnimation => _pivotVisual?.UsesContinuousAnimation == true;
    internal bool UsesGroupedSignalEditor => _signalGroups.Controls.OfType<GroupBox>().Any();
    internal int SignalStructureBuildCount => _signalStructureBuildCount;
    internal int EditableSignalCount => _digitalEditors.Count + _analogEditors.Count;
    internal int MetricCount => _metricGrid?.Controls.Count ?? 0;
    internal bool MetricsUseHorizontalScroll => _metricGrid?.AutoScroll == true;
    internal bool ConfigurationUsesScroll => _configurationSidebar?.AutoScroll == true;
    internal bool ConfigurationContentFits => _configurationSidebar is not null
        && _configurationSidebar.Controls.Cast<Control>()
            .Where(control => control.Visible)
            .Select(control => control.Bottom + control.Margin.Bottom)
            .DefaultIfEmpty(0)
            .Max() <= _configurationSidebar.ClientSize.Height;
    internal int SignalColumnCount => _signalLayouts.FirstOrDefault()?.Grid.ColumnCount ?? 0;
    internal bool SignalItemsAreCompact => _signalLayouts
        .SelectMany(layout => layout.Items)
        .All(item => item.Control.Height is >= 32 and <= 40);

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
        _rootLayout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            RowCount = 1,
            BackColor = PlatformUi.Background,
            Name = "simulatorRoot"
        };
        _rootLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 286F));
        _rootLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        _rootLayout.Controls.Add(BuildSidebar(), 0, 0);
        _rootLayout.Controls.Add(BuildProcessPanel(), 1, 0);
        return _rootLayout;
    }

    private Control BuildSidebar()
    {
        _configurationSidebar = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            AutoScroll = false,
            FlowDirection = FlowDirection.TopDown,
            WrapContents = false,
            Padding = new Padding(
                IndustrialSpacing.Md,
                IndustrialSpacing.Sm,
                IndustrialSpacing.Sm,
                IndustrialSpacing.Sm),
            BackColor = PlatformUi.Surface,
            Name = "simulatorConfigurationSidebar",
            AccessibleName = "Configuração e estado da simulação"
        };
        FlowLayoutPanel sidebar = _configurationSidebar;
        PlatformUi.StyleField(_profiles);
        PlatformUi.StyleField(_scenarios);
        sidebar.Controls.Add(SidebarLabel("Perfil"));
        sidebar.Controls.Add(_profiles);
        sidebar.Controls.Add(SidebarLabel("Cenário"));
        sidebar.Controls.Add(_scenarios);
        Button apply = PlatformUi.Button("Aplicar cenário", "applyScenarioButton", primary: true);
        Button reset = PlatformUi.Button("Resetar", "resetScenarioButton");
        PlatformUi.SetButtonTone(reset, PlatformButtonTone.Ghost);
        apply.Width = reset.Width = 220;
        apply.Height = reset.Height = IndustrialSpacing.FieldHeight;
        apply.Margin = new Padding(3, IndustrialSpacing.Sm, 3, 2);
        reset.Margin = new Padding(3, 2, 3, 2);
        apply.Click += (_, _) => ApplyScenario();
        reset.Click += (_, _) => ResetSimulation();
        apply.AccessibleDescription = "Aplica o cenário selecionado somente à simulação em memória";
        reset.AccessibleDescription = "Restaura os valores simulados do perfil ativo";
        sidebar.Controls.Add(apply);
        sidebar.Controls.Add(reset);
        Label ready = PlatformUi.StatusChip(
            "SIMULAÇÃO PRONTA • EM MEMÓRIA",
            PlatformStatusTone.Simulated,
            "simulationReadyStatus");
        ready.AutoSize = false;
        ready.Width = 220;
        ready.Height = 30;
        ready.Margin = new Padding(3, IndustrialSpacing.Sm, 3, 2);
        ready.AccessibleDescription = "Simulação pronta; nenhuma comunicação física ativa";
        sidebar.Controls.Add(ready);
        sidebar.Controls.Add(SidebarLabel("Estado"));
        _state.Width = 220;
        _state.Height = 30;
        _state.AutoSize = false;
        _state.AccessibleDescription = "Estado operacional projetado pela simulação";
        sidebar.Controls.Add(_state);
        sidebar.Controls.Add(SidebarLabel("Segurança derivada"));
        _safety.Width = 220;
        _safety.Height = 34;
        _safety.AutoSize = false;
        _safety.AccessibleDescription = "SafetyChain derivada e não editável";
        sidebar.Controls.Add(_safety);
        sidebar.Controls.Add(SidebarLabel("Alarmes"));
        _alarms.Width = 220;
        _alarms.Height = 42;
        _alarms.AutoSize = false;
        _alarms.AccessibleName = "Alarmes simulados";
        sidebar.Controls.Add(_alarms);
        _counters.Width = 220;
        _counters.Height = 64;
        _counters.AutoSize = false;
        _counters.Margin = new Padding(
            IndustrialSpacing.Xs,
            IndustrialSpacing.Sm,
            IndustrialSpacing.Xs,
            IndustrialSpacing.Xs);
        _counters.Font = IndustrialTypography.Technical();
        _counters.AccessibleName = "Contadores da simulação e bloqueio físico";
        sidebar.Controls.Add(_counters);
        return sidebar;
    }

    private static Label SidebarLabel(string text)
    {
        Label label = PlatformUi.Label(text, heading: true);
        label.Margin = new Padding(IndustrialSpacing.Xs, 2, IndustrialSpacing.Xs, 0);
        return label;
    }

    private Control BuildProcessPanel()
    {
        bool isPivot = _session.Profile.Visualization.Type == SimulationVisualizationType.Pivot;
        _processPanel = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            RowCount = 3,
            ColumnCount = 1,
            Padding = new Padding(IndustrialSpacing.Md),
            BackColor = PlatformUi.Background,
            Name = "simulatorProcessPanel"
        };
        TableLayoutPanel panel = _processPanel;
        panel.RowStyles.Add(new RowStyle(SizeType.Absolute, 56F));
        panel.RowStyles.Add(new RowStyle(SizeType.Absolute, isPivot ? 292F : 166F));
        panel.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
        panel.Controls.Add(BuildProcessHeader(), 0, 0);
        panel.Controls.Add(isPivot ? BuildPivotOverview() : BuildWellOverview(), 0, 1);
        panel.Controls.Add(BuildSignalEditor(), 0, 2);
        return panel;
    }

    private Control BuildProcessHeader()
    {
        TableLayoutPanel header = new()
        {
            Dock = DockStyle.Fill,
            ColumnCount = 3,
            RowCount = 1,
            BackColor = PlatformUi.Background
        };
        header.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        header.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        header.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        Label title = PlatformUi.PageTitle(
            $"Simulador industrial • {_session.Profile.DisplayName ?? _session.Profile.Id!}",
            "Simulador industrial e perfil ativo");
        title.Dock = DockStyle.Fill;
        title.TextAlign = ContentAlignment.MiddleLeft;
        Label simulated = PlatformUi.StatusChip("Simulado", PlatformStatusTone.Simulated, "simulatedProcessStatus");
        _profileEvidence = PlatformUi.StatusChip(
            "Perfil de simulação",
            PlatformStatusTone.Offline,
            "simulationProfileEvidenceStatus");
        simulated.Anchor = AnchorStyles.None;
        _profileEvidence.Anchor = AnchorStyles.None;
        header.Controls.Add(title, 0, 0);
        header.Controls.Add(simulated, 1, 0);
        header.Controls.Add(_profileEvidence, 2, 0);
        return header;
    }

    private Control BuildPivotOverview()
    {
        _pivotOverview = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            RowCount = 1,
            BackColor = PlatformUi.Background,
            Margin = new Padding(0, 0, 0, 8)
        };
        _pivotOverview.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        _pivotOverview.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 232F));
        _pivotVisual = new PivotProcessControl { Dock = DockStyle.Fill, Margin = new Padding(0, 0, 8, 0) };
        _pivotOverview.Controls.Add(_pivotVisual, 0, 0);
        _pivotOverview.Controls.Add(BuildMetricPanel(vertical: true), 1, 0);
        return _pivotOverview;
    }

    private Control BuildWellOverview()
    {
        TableLayoutPanel overview = new()
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            RowCount = 1,
            BackColor = PlatformUi.Background,
            Margin = new Padding(0, 0, 0, 8)
        };
        overview.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 260F));
        overview.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        Panel summary = new()
        {
            Dock = DockStyle.Fill,
            BackColor = PlatformUi.Surface,
            Padding = new Padding(16),
            Margin = new Padding(0, 0, 8, 0)
        };
        Label identity = PlatformUi.Label("PROCESSO POÇO", heading: true);
        identity.Dock = DockStyle.Top;
        _overviewStatus.Dock = DockStyle.Fill;
        _overviewStatus.AutoSize = false;
        _overviewStatus.TextAlign = ContentAlignment.MiddleLeft;
        summary.Controls.Add(_overviewStatus);
        summary.Controls.Add(identity);
        overview.Controls.Add(summary, 0, 0);
        overview.Controls.Add(BuildMetricPanel(vertical: false), 1, 0);
        return overview;
    }

    private Control BuildMetricPanel(bool vertical)
    {
        SimulationSignalDefinition[] definitions = _session.Profile.Signals
            .Where(signal => signal.Kind == SimulationSignalKind.AnalogInput)
            .ToArray();
        int columns = vertical ? 1 : Math.Clamp(definitions.Length, 1, 2);
        int rows = Math.Max(1, (int)Math.Ceiling(definitions.Length / (double)columns));
        TableLayoutPanel metrics = new()
        {
            Dock = DockStyle.Fill,
            AutoScroll = false,
            ColumnCount = columns,
            RowCount = rows,
            BackColor = PlatformUi.Background,
            Padding = Padding.Empty,
            Margin = Padding.Empty,
            Name = "simulationMetricsGrid",
            AccessibleName = "Métricas do processo sem rolagem horizontal"
        };
        _metricGrid = metrics;
        for (int column = 0; column < columns; column++)
        {
            metrics.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F / columns));
        }

        for (int row = 0; row < rows; row++)
        {
            metrics.RowStyles.Add(new RowStyle(SizeType.Percent, 100F / rows));
        }

        for (int index = 0; index < definitions.Length; index++)
        {
            SimulationSignalDefinition definition = definitions[index];
            Panel card = PlatformUi.Card($"simulationMetricCard{definition.Id}");
            card.Dock = DockStyle.Fill;
            card.MinimumSize = Size.Empty;
            card.Padding = new Padding(
                IndustrialSpacing.Md,
                IndustrialSpacing.Xs,
                IndustrialSpacing.Md,
                IndustrialSpacing.Xs);
            card.Margin = new Padding(IndustrialSpacing.Xs);
            card.AccessibleName = $"Métrica {definition.Label ?? definition.Id}";
            TableLayoutPanel hierarchy = new()
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 3,
                BackColor = PlatformUi.Surface,
                Margin = Padding.Empty,
                Name = "simulationMetricHierarchy"
            };
            hierarchy.RowStyles.Add(new RowStyle(SizeType.Percent, 30F));
            hierarchy.RowStyles.Add(new RowStyle(SizeType.Percent, 45F));
            hierarchy.RowStyles.Add(new RowStyle(SizeType.Percent, 25F));
            Label label = PlatformUi.Label((definition.Label ?? definition.Id!).ToUpperInvariant());
            label.Dock = DockStyle.Fill;
            label.Margin = Padding.Empty;
            label.AutoEllipsis = true;
            label.TextAlign = ContentAlignment.MiddleLeft;
            Label value = PlatformUi.Label(string.Empty);
            value.Name = $"simulationMetric{definition.Id}";
            value.Dock = DockStyle.Fill;
            value.AutoSize = false;
            value.Margin = Padding.Empty;
            value.Font = IndustrialTypography.Section();
            value.TextAlign = ContentAlignment.MiddleLeft;
            value.AutoEllipsis = true;
            Label context = PlatformUi.Label("simulado");
            context.Dock = DockStyle.Fill;
            context.Margin = Padding.Empty;
            context.Font = IndustrialTypography.Caption();
            context.TextAlign = ContentAlignment.MiddleLeft;
            _metricValues[definition.Id!] = value;
            hierarchy.Controls.Add(label, 0, 0);
            hierarchy.Controls.Add(value, 0, 1);
            hierarchy.Controls.Add(context, 0, 2);
            card.Controls.Add(hierarchy);
            metrics.Controls.Add(card, index % columns, index / columns);
        }

        return metrics;
    }

    private Control BuildSignalEditor()
    {
        GroupBox editor = PlatformUi.Group("Sinais do processo • edição manual e saídas virtuais");
        editor.Dock = DockStyle.Fill;
        editor.Padding = new Padding(10, 12, 10, 10);
        editor.Controls.Add(_signals);
        return editor;
    }

    private void BuildSignals()
    {
        _signalStructureBuildCount++;
        _digitalEditors.Clear();
        _analogEditors.Clear();
        _signalLayouts.Clear();
        _signalGroups.SuspendLayout();
        _signalGroups.Controls.Clear();
        _signalGroups.RowStyles.Clear();
        _signalGroups.RowCount = 0;
        foreach (IGrouping<string, SimulationSignalDefinition> group in _session.Profile.Signals.GroupBy(
                     signal => string.IsNullOrWhiteSpace(signal.Group) ? "Sinais" : signal.Group!,
                     StringComparer.OrdinalIgnoreCase))
        {
            GroupBox section = PlatformUi.Group(group.Key.ToUpperInvariant());
            section.Dock = DockStyle.Top;
            section.AutoSize = true;
            section.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            section.Padding = new Padding(8, 12, 8, 8);
            TableLayoutPanel rows = new()
            {
                Dock = DockStyle.Top,
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                ColumnCount = 2,
                RowCount = 0,
                BackColor = PlatformUi.Surface
            };
            List<SignalLayoutItem> items = [];
            foreach (SimulationSignalDefinition definition in group)
            {
                bool isDerived = definition.Kind == SimulationSignalKind.DigitalInput
                    && _session.Profile.DerivedSignals.Any(derived =>
                        string.Equals(derived.TargetSignalId, definition.Id, StringComparison.OrdinalIgnoreCase));
                Control signal = definition.Kind switch
                {
                    SimulationSignalKind.DigitalInput => BuildDigitalSignal(definition),
                    SimulationSignalKind.AnalogInput => BuildAnalogSignal(definition),
                    SimulationSignalKind.VirtualOutput => BuildOutputSignal(definition),
                    _ => throw new ArgumentOutOfRangeException()
                };
                signal.Name = $"simulation{definition.Id}SignalSurface";
                items.Add(new SignalLayoutItem(signal, isDerived));
            }

            SignalGroupLayout signalLayout = new(group.Key, rows, items);
            _signalLayouts.Add(signalLayout);
            ReflowSignalGrid(signalLayout, PreferredSignalColumnCount());

            section.Controls.Add(rows);
            int sectionRow = _signalGroups.RowCount++;
            _signalGroups.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            _signalGroups.Controls.Add(section, 0, sectionRow);
        }

        _signalGroups.ResumeLayout(performLayout: true);
    }

    private Control BuildDigitalSignal(SimulationSignalDefinition definition)
    {
        bool isDerived = _session.Profile.DerivedSignals.Any(derived =>
            string.Equals(derived.TargetSignalId, definition.Id, StringComparison.OrdinalIgnoreCase));
        if (isDerived)
        {
            return BuildDerivedDigitalSignal(definition);
        }

        CheckBox value = new()
        {
            Text = definition.Label,
            Name = $"simulation{definition.Id}Input",
            Checked = _session.Simulation.GetValue(definition.Id!) != 0,
            AccessibleName = definition.Label ?? definition.Id,
            AutoSize = false,
            Height = 30,
            ForeColor = PlatformUi.Text
        };
        _digitalEditors[definition.Id!] = value;
        value.CheckedChanged += (_, _) =>
        {
            if (!_updatingEditors)
            {
                _session.SetDigitalInput(definition.Id!, value.Checked);
            }
        };
        return SignalSurface(value);
    }

    private Control BuildDerivedDigitalSignal(SimulationSignalDefinition definition)
    {
        FlowLayoutPanel row = new() { Dock = DockStyle.Fill, WrapContents = false };
        Label label = PlatformUi.Label($"{definition.Label ?? definition.Id} • DERIVADO", heading: true);
        label.Width = 230;
        bool active = _session.Simulation.GetValue(definition.Id!) != 0;
        Label value = PlatformUi.Label(active ? "● ON" : "○ OFF");
        value.Name = $"simulation{definition.Id}Derived";
        value.Width = 140;
        value.ForeColor = active ? PlatformUi.Success : PlatformUi.Muted;
        row.Controls.Add(label);
        row.Controls.Add(value);
        return SignalSurface(row);
    }

    private Control BuildAnalogSignal(SimulationSignalDefinition definition)
    {
        TableLayoutPanel row = new()
        {
            Dock = DockStyle.Fill,
            ColumnCount = 3,
            RowCount = 1,
            Margin = Padding.Empty
        };
        row.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        row.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 120F));
        row.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        Label label = PlatformUi.Label(definition.Label ?? definition.Id!, heading: true);
        label.Dock = DockStyle.Fill;
        label.AutoEllipsis = true;
        NumericUpDown value = new()
        {
            Name = $"simulation{definition.Id}Input",
            Minimum = (decimal)definition.Minimum,
            Maximum = (decimal)definition.Maximum,
            DecimalPlaces = 2,
            Increment = 1,
            Value = (decimal)_session.Simulation.GetValue(definition.Id!),
            Dock = DockStyle.Fill,
            AccessibleName = definition.Label ?? definition.Id
        };
        PlatformUi.StyleField(value);
        _analogEditors[definition.Id!] = value;
        value.ValueChanged += (_, _) =>
        {
            if (!_updatingEditors)
            {
                _session.SetAnalogInput(definition.Id!, (double)value.Value);
            }
        };
        Label unit = PlatformUi.Label(string.IsNullOrWhiteSpace(definition.Unit) ? "raw" : definition.Unit!);
        unit.Dock = DockStyle.Fill;
        unit.AutoEllipsis = true;
        row.Controls.Add(label, 0, 0);
        row.Controls.Add(value, 1, 0);
        row.Controls.Add(unit, 2, 0);
        return SignalSurface(row);
    }

    private Control BuildOutputSignal(SimulationSignalDefinition definition)
    {
        TableLayoutPanel row = new() { Dock = DockStyle.Fill, ColumnCount = 2, RowCount = 1, Margin = Padding.Empty };
        row.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 70F));
        row.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 30F));
        Label label = PlatformUi.Label(definition.Label ?? definition.Id!, heading: true);
        label.Dock = DockStyle.Fill;
        label.AutoEllipsis = true;
        bool active = _session.Simulation.GetValue(definition.Id!) != 0;
        Label value = PlatformUi.Label(active ? "● ON" : "○ OFF");
        value.Name = $"simulation{definition.Id}Output";
        value.Dock = DockStyle.Fill;
        value.ForeColor = active ? PlatformUi.Success : PlatformUi.Muted;
        row.Controls.Add(label, 0, 0);
        row.Controls.Add(value, 1, 0);
        return SignalSurface(row);
    }

    private static Control SignalSurface(Control content)
    {
        Panel surface = new()
        {
            Dock = DockStyle.Fill,
            Height = 38,
            BackColor = PlatformUi.Surface,
            Padding = new Padding(IndustrialSpacing.Sm, IndustrialSpacing.Xs, IndustrialSpacing.Sm, IndustrialSpacing.Xs),
            Margin = new Padding(IndustrialSpacing.Xs, 2, IndustrialSpacing.Xs, 2)
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
            RefreshSnapshot();
        }
    }

    private void ResetSimulation()
    {
        _session.ResetSimulation();
        RefreshSnapshot();
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
        SimulationProcessState processState = SimulationProcessStateProjector.Project(
            _session.Profile,
            snapshot);
        PlatformUi.UpdateStatusChip(
            _state,
            NormalizeOperationalState(snapshot.State),
            snapshot.OutputsBlocked ? PlatformStatusTone.Fault : PlatformStatusTone.Normal);
        _alarms.Text = snapshot.ActiveAlarms.Count == 0
            ? "✓ Nenhum alarme"
            : "! " + string.Join(Environment.NewLine + "! ", snapshot.ActiveAlarms);
        _alarms.ForeColor = snapshot.ActiveAlarms.Count == 0 ? PlatformUi.Success : PlatformUi.Warning;
        _counters.Text = $"Operações simuladas: {_session.Simulation.Counters.SimulatedOperations}\r\n"
            + $"Comandos simulados: {_session.Simulation.Counters.SimulatedCommands}\r\n"
            + "Físico C/R/W/CMD: 0/0/0/0\r\n"
            + "Timer contínuo: NÃO";

        SimulationSignalDefinition? safetyDefinition = _session.FindSignal("SafetyChain");
        if (safetyDefinition is null)
        {
            PlatformUi.UpdateStatusChip(
                _safety,
                "SAFETYCHAIN • N/A",
                PlatformStatusTone.Disabled);
        }
        else
        {
            bool closed = _session.Simulation.GetValue("SafetyChain") >= 0.5D;
            PlatformUi.UpdateStatusChip(
                _safety,
                closed ? "SEGURANÇA OK • READ-ONLY" : "CADEIA ABERTA • READ-ONLY",
                closed ? PlatformStatusTone.Normal : PlatformStatusTone.Fault);
        }

        RefreshInputEditors();

        if (_pivotVisual is not null)
        {
            _pivotVisual.UpdateState(PivotProcessStateProjector.Project(_session.Profile, processState));
        }
        else
        {
            string pump = processState.IsRoleActive("pump") ? "ON" : "OFF";
            string valve = processState.IsRoleActive("valve") ? "ON" : "OFF";
            _overviewStatus.Text = $"Estado: {processState.OverallState}\r\n"
                + $"Bomba: {pump}\r\n"
                + $"Válvula: {valve}\r\n"
                + "Execução: SIMULADA";
            _overviewStatus.ForeColor = processState.OutputsBlocked ? PlatformUi.Danger : PlatformUi.Text;
        }

        foreach (ProcessMetricState metric in processState.Metrics)
        {
            if (_metricValues.TryGetValue(metric.SignalId, out Label? value))
            {
                string unit = (metric.Unit ?? string.Empty)
                    .Replace("(simulado)", string.Empty, StringComparison.OrdinalIgnoreCase)
                    .Trim();
                value.Text = string.IsNullOrWhiteSpace(unit)
                    ? $"{metric.Value:0.##} raw"
                    : $"{metric.Value:0.##} {unit}";
            }
        }

        foreach (SimulationSignalDefinition definition in _session.Profile.Signals.Where(signal =>
                     signal.Kind == SimulationSignalKind.VirtualOutput))
        {
            Control[] controls = Controls.Find($"simulation{definition.Id}Output", searchAllChildren: true);
            if (controls.FirstOrDefault() is Label value)
            {
                bool active = _session.Simulation.GetValue(definition.Id!) != 0;
                value.Text = active ? "● ON" : "○ OFF";
                value.ForeColor = active ? PlatformUi.Success : PlatformUi.Muted;
            }
        }

        foreach (SimulationDerivedSignal derived in _session.Profile.DerivedSignals)
        {
            if (string.IsNullOrWhiteSpace(derived.TargetSignalId))
            {
                continue;
            }

            Control[] controls = Controls.Find($"simulation{derived.TargetSignalId}Derived", searchAllChildren: true);
            if (controls.FirstOrDefault() is Label value)
            {
                bool active = _session.Simulation.GetValue(derived.TargetSignalId!) != 0;
                value.Text = active ? "● ON" : "○ OFF";
                value.ForeColor = active ? PlatformUi.Success : PlatformUi.Muted;
            }
        }
    }

    internal void ApplyTheme()
    {
        IndustrialPalette palette = IndustrialTheme.Palette;
        BackColor = palette.Background;
        ForeColor = palette.TextPrimary;
        ApplyThemeToChildren(this);
        foreach (Label value in _metricValues.Values)
        {
            value.ForeColor = palette.TextPrimary;
        }
        _pivotVisual?.ApplyTheme();
        RefreshSnapshot();
        Invalidate(true);
    }

    private void UpdateResponsiveLayout()
    {
        if (_rootLayout is null)
        {
            return;
        }

        float logicalWidth = ClientSize.Width * 96F / Math.Max(DeviceDpi, 96);
        bool compact = logicalWidth < 900F;
        _rootLayout.ColumnStyles[0].Width = compact ? 248F : 286F;
        if (_pivotOverview is not null)
        {
            _pivotOverview.ColumnStyles[1].Width = compact ? 190F : 232F;
        }

        if (_processPanel is not null)
        {
            _processPanel.RowStyles[1].Height = logicalWidth < 1400F ? 268F : 292F;
        }

        if (_profileEvidence is not null)
        {
            _profileEvidence.Visible = !compact;
        }

        int signalColumns = PreferredSignalColumnCount();
        foreach (SignalGroupLayout layout in _signalLayouts)
        {
            ReflowSignalGrid(layout, signalColumns);
        }
    }

    private int PreferredSignalColumnCount()
    {
        float logicalWidth = ClientSize.Width * 96F / Math.Max(DeviceDpi, 96);
        return logicalWidth >= 1450F ? 3 : logicalWidth >= 900F ? 2 : 1;
    }

    private static void ReflowSignalGrid(SignalGroupLayout layout, int columns)
    {
        if (layout.Grid.ColumnCount == columns && layout.Grid.Controls.Count == layout.Items.Count)
        {
            return;
        }

        layout.Grid.SuspendLayout();
        layout.Grid.Controls.Clear();
        layout.Grid.ColumnStyles.Clear();
        layout.Grid.RowStyles.Clear();
        layout.Grid.ColumnCount = columns;
        layout.Grid.RowCount = 0;
        for (int column = 0; column < columns; column++)
        {
            layout.Grid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F / columns));
        }

        int row = 0;
        int columnIndex = 0;
        IReadOnlyList<SignalLayoutItem> editable = OrderedEditableSignals(layout, columns);
        foreach (SignalLayoutItem item in editable)
        {
            EnsureSignalRow(layout.Grid, row);
            layout.Grid.Controls.Add(item.Control, columnIndex, row);
            columnIndex++;
            if (columnIndex == columns)
            {
                columnIndex = 0;
                row++;
            }
        }

        if (columnIndex != 0)
        {
            row++;
        }

        foreach (SignalLayoutItem item in layout.Items.Where(item => item.IsDerived))
        {
            EnsureSignalRow(layout.Grid, row);
            layout.Grid.Controls.Add(item.Control, 0, row);
            layout.Grid.SetColumnSpan(item.Control, columns);
            row++;
        }

        layout.Grid.RowCount = row;
        layout.Grid.ResumeLayout(performLayout: true);
    }

    private static IReadOnlyList<SignalLayoutItem> OrderedEditableSignals(
        SignalGroupLayout layout,
        int columns)
    {
        SignalLayoutItem[] editable = layout.Items.Where(item => !item.IsDerived).ToArray();
        if (!layout.GroupName.Contains("seguran", StringComparison.OrdinalIgnoreCase)
            || editable.Length != 6)
        {
            return editable;
        }

        int[] order = columns switch
        {
            >= 3 => [0, 2, 4, 1, 3, 5],
            2 => [0, 2, 1, 3, 4, 5],
            _ => [0, 1, 2, 3, 4, 5]
        };
        return order.Select(index => editable[index]).ToArray();
    }

    private static void EnsureSignalRow(TableLayoutPanel grid, int row)
    {
        while (grid.RowStyles.Count <= row)
        {
            grid.RowStyles.Add(new RowStyle(SizeType.Absolute, 42F));
        }
    }

    private void ApplyThemeToChildren(Control root)
    {
        IndustrialPalette palette = IndustrialTheme.Palette;
        foreach (Control child in root.Controls)
        {
            if (child is Label label
                && label.Tag is not PlatformStatusTone
                && label.BorderStyle != BorderStyle.FixedSingle)
            {
                label.ForeColor = palette.TextSecondary;
            }

            switch (child)
            {
                case IndustrialComboBox combo:
                    combo.ApplyTheme();
                    break;
                case TextBox or ComboBox or NumericUpDown:
                    child.BackColor = palette.Field;
                    child.ForeColor = palette.TextPrimary;
                    break;
                case CheckBox checkBox:
                    checkBox.BackColor = checkBox.Parent is GroupBox
                        ? palette.SurfaceElevated
                        : palette.Background;
                    checkBox.ForeColor = palette.TextPrimary;
                    break;
                case Button button:
                    PlatformUi.StyleButton(
                        button,
                        primary: button.Name == "applyScenarioButton");
                    break;
                case FlowLayoutPanel flow:
                    flow.BackColor = flow.Parent is GroupBox
                        ? palette.SurfaceElevated
                        : palette.Background;
                    break;
                case TableLayoutPanel table:
                    table.BackColor = table.Name == "simulationMetricHierarchy"
                        ? palette.SurfaceElevated
                        : table.Parent is GroupBox
                        ? palette.SurfaceElevated
                        : palette.Background;
                    break;
                case GroupBox:
                case Panel:
                    child.BackColor = palette.SurfaceElevated;
                    break;
            }

            ApplyThemeToChildren(child);
        }
    }

    private void RefreshInputEditors()
    {
        _updatingEditors = true;
        try
        {
            foreach ((string signalId, CheckBox editor) in _digitalEditors)
            {
                bool value = _session.Simulation.GetValue(signalId) != 0;
                if (editor.Checked != value)
                {
                    editor.Checked = value;
                }
            }

            foreach ((string signalId, NumericUpDown editor) in _analogEditors)
            {
                decimal value = Math.Clamp(
                    (decimal)_session.Simulation.GetValue(signalId),
                    editor.Minimum,
                    editor.Maximum);
                if (editor.Value != value)
                {
                    editor.Value = value;
                }
            }
        }
        finally
        {
            _updatingEditors = false;
        }
    }

    private static string NormalizeOperationalState(string state) => state.ToUpperInvariant() switch
    {
        "MOVING" => "MOVIMENTO",
        "FAULT" => "FALHA",
        "DEVELOPMENT" => "DESENVOLVIMENTO",
        _ => state
    };

    private sealed record ScenarioItem(string Id, string DisplayName)
    {
        public override string ToString() => DisplayName;
    }

    private sealed record SignalLayoutItem(Control Control, bool IsDerived);

    private sealed record SignalGroupLayout(
        string GroupName,
        TableLayoutPanel Grid,
        IReadOnlyList<SignalLayoutItem> Items);
}
