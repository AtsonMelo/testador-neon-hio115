using TestadorCLPHI.App.Industrial.Platform.Integration;
using TestadorCLPHI.App.Industrial.Platform.Rtu;
using TestadorCLPHI.App.Industrial.Platform.Simulation;
using TestadorCLPHI.App.Industrial.Platform.Safety;
using TestadorCLPHI.App.Ui.Controls;
using TestadorCLPHI.App.Ui.Theme;

namespace TestadorCLPHI.App.Ui.Industrial.Platform;

internal sealed class IndustrialTesterControl : UserControl
{
    private readonly IndustrialPlatformSession _session;
    private readonly ComboBox _port = Combo("COM8");
    private readonly ComboBox _physicalLayer = Combo("RS232", "RS485");
    private readonly ComboBox _baud = Combo("38400", "9600", "19200", "57600", "115200");
    private readonly ComboBox _dataBits = Combo("8", "7");
    private readonly ComboBox _parity = Combo("None", "Even", "Odd");
    private readonly ComboBox _stopBits = Combo("1", "2");
    private readonly NumericUpDown _timeout = Number(250, 10, 10000);
    private readonly NumericUpDown _interval = Number(10, 0, 5000);
    private readonly NumericUpDown _startAddress = Number(1, 1, 247);
    private readonly NumericUpDown _endAddress = Number(247, 1, 247);
    private readonly Label _state = PlatformUi.StatusChip(
        "PRONTO OFFLINE",
        PlatformStatusTone.Normal,
        "testerStateStatus");
    private readonly Label _purposeStatus = PlatformUi.StatusChip(
        "LEITURA • DIAGNÓSTICO • RESULTADOS",
        PlatformStatusTone.Active,
        "testerPurposeStatus");
    private readonly Label _offlineSafety = PlatformUi.StatusChip(
        "OFFLINE • RTU EM MEMÓRIA • FÍSICA BLOQUEADA",
        PlatformStatusTone.Offline,
        "testerOfflineSafetyStatus");
    private readonly Label _result = PlatformUi.Label("Equipamento ainda nao identificado.");
    private readonly Label _counters = PlatformUi.Label(string.Empty);
    private readonly Label[] _digitalValues = Enumerable.Range(0, 8).Select(_ => PlatformUi.Label("- DESCONHECIDO")).ToArray();
    private readonly Label[] _analogValues = Enumerable.Range(0, 3).Select(_ => PlatformUi.Label("- DESCONHECIDO")).ToArray();
    private readonly Label[] _outputValues = Enumerable.Range(0, 4).Select(_ => PlatformUi.Label("○ OFF")).ToArray();
    private readonly CheckBox _enableOutputs = new() { Text = "MODO SUPERVISIONADO SIMULADO", AutoSize = true };
    private readonly NumericUpDown _outputDuration = Number(250, 50, 3000);
    private readonly TextBox _log = new()
    {
        Multiline = true,
        ReadOnly = true,
        ScrollBars = ScrollBars.Both,
        WordWrap = false,
        Dock = DockStyle.Fill,
        AccessibleName = "Log técnico incremental do Testador",
        AccessibleDescription = "Eventos offline, comandos simulados e diagnósticos em ordem cronológica"
    };
    private readonly ToolTip _toolTip = new();
    private readonly TabControl _tabs = new()
    {
        Dock = DockStyle.Fill,
        Name = "testerTabs",
        AccessibleName = "Áreas do Testador",
        DrawMode = TabDrawMode.OwnerDrawFixed,
        SizeMode = TabSizeMode.Fixed,
        ItemSize = new Size(150, 38)
    };
    private IndustrialIoMapControl? _ioMap;
    private CancellationTokenSource? _operation;
    private bool _identified;
    private string _renderedLog = string.Empty;
    private string _stateText = "PRONTO OFFLINE";
    private PlatformStatusTone _stateTone = PlatformStatusTone.Normal;

    internal IndustrialTesterControl(IndustrialPlatformSession session)
    {
        _session = session ?? throw new ArgumentNullException(nameof(session));
        Name = "industrialTesterControl";
        AccessibleName = "Testador industrial offline";
        AccessibleDescription = "Leitura, diagnóstico e resultados usando somente transporte em memória";
        AutoScaleMode = AutoScaleMode.Dpi;
        AutoScroll = true;
        Controls.Add(BuildLayout());
        _tabs.DrawItem += DrawTab;
        PlatformUi.StyleTabs(_tabs);
        _state.AccessibleDescription = "Estado operacional do Testador offline";
        _result.AccessibleName = "Resultado da operação";
        _counters.AccessibleName = "Contadores simulados e físicos";
        _offlineSafety.AccessibleDescription =
            "Transporte somente em memória; comunicação física bloqueada";
        _session.Changed += SessionChanged;
        ApplyTheme();
        RefreshStatus();
    }

    internal bool UsesOnlyInMemoryTransport => true;
    internal bool RealCommunicationEnabled => false;
    internal bool HasIoMappingTab => Controls.Find("ioMappingTab", searchAllChildren: true).Length == 1;
    internal int IoMappingRowCount => _ioMap?.BindingRowCount ?? 0;
    internal bool IoMappingDeclaresSimulationEvidence => _ioMap?.DeclaresSimulationEvidence == true;
    internal bool UsesIncrementalLogUpdates => true;
    internal int GetSelectedViewIndex() => _tabs.SelectedIndex;
    internal void SelectView(int index) => _tabs.SelectedIndex = index;
    internal string RenderedLogText => _renderedLog;

    internal bool ShowsProcessAlias(string registerAlias, string expectedLabel)
    {
        Control[] controls = Controls.Find($"tester{registerAlias}Alias", searchAllChildren: true);
        return controls.FirstOrDefault() is Label label
            && string.Equals(label.Text, expectedLabel, StringComparison.Ordinal);
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _operation?.Cancel();
            _operation?.Dispose();
            _toolTip.Dispose();
            _session.Changed -= SessionChanged;
        }

        base.Dispose(disposing);
    }

    private Control BuildLayout()
    {
        TableLayoutPanel root = new()
        {
            Dock = DockStyle.Fill,
            RowCount = 3,
            ColumnCount = 1,
            BackColor = PlatformUi.Background
        };
        root.RowCount = 4;
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 44F));
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 172F));
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 60F));
        root.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
        root.Controls.Add(BuildHero(), 0, 0);
        root.Controls.Add(BuildConfiguration(), 0, 1);
        root.Controls.Add(BuildStatus(), 0, 2);
        root.Controls.Add(BuildTabs(), 0, 3);
        return root;
    }

    private Control BuildHero()
    {
        TableLayoutPanel hero = new()
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            RowCount = 1,
            Padding = new Padding(IndustrialSpacing.Md, IndustrialSpacing.Xs, IndustrialSpacing.Md, IndustrialSpacing.Xs),
            Name = "testerHero"
        };
        hero.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        hero.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 280F));
        Label title = new()
        {
            Text = "TESTADOR",
            Dock = DockStyle.Fill,
            TextAlign = ContentAlignment.MiddleLeft,
            Font = IndustrialTypography.Title(),
            AccessibleName = "Modo Testador"
        };
        _purposeStatus.Anchor = AnchorStyles.None;
        hero.Controls.Add(title, 0, 0);
        hero.Controls.Add(_purposeStatus, 1, 0);
        return hero;
    }

    private Control BuildConfiguration()
    {
        GroupBox group = PlatformUi.Group("PARÂMETROS RTU • REFERÊNCIA OFFLINE");
        group.Dock = DockStyle.Fill;
        group.Padding = new Padding(IndustrialSpacing.Sm);
        TableLayoutPanel grid = new() { Dock = DockStyle.Fill, ColumnCount = 5, RowCount = 4 };
        for (int column = 0; column < 5; column++)
        {
            grid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 20F));
        }
        grid.RowStyles.Add(new RowStyle(SizeType.Percent, 50F));
        grid.RowStyles.Add(new RowStyle(SizeType.Percent, 50F));
        grid.RowStyles.Add(new RowStyle(SizeType.Absolute, 42F));
        grid.RowStyles.Add(new RowStyle(SizeType.Absolute, 24F));

        AddField(grid, "COM (informativa)", _port, 0);
        AddField(grid, "Camada física", _physicalLayer, 1);
        AddField(grid, "Baud", _baud, 2);
        AddField(grid, "Data bits", _dataBits, 3);
        AddField(grid, "Paridade", _parity, 4);
        AddField(grid, "Stop bits", _stopBits, 5);
        AddField(grid, "Timeout (ms)", _timeout, 6);
        AddField(grid, "Intervalo (ms)", _interval, 7);
        AddField(grid, "Endereço inicial", _startAddress, 8);
        AddField(grid, "Endereço final", _endAddress, 9);

        FlowLayoutPanel actions = new()
        {
            Dock = DockStyle.Fill,
            AutoSize = false,
            WrapContents = true,
            AccessibleName = "Ações do Testador"
        };
        Button refreshPorts = PlatformUi.Button("PORTAS", "refreshPortsButton");
        refreshPorts.Enabled = false;
        _toolTip.SetToolTip(refreshPorts, "Indisponível no host estritamente offline.");
        _toolTip.SetToolTip(_port, "Valor informativo do perfil. Nenhuma porta é enumerada ou aberta.");
        Button validate = PlatformUi.Button("VALIDAR", "validateRtuButton", primary: true);
        Button identify = PlatformUi.Button("IDENTIFICAR", "identifyButton");
        Button discover = PlatformUi.Button("PROCURAR ENDEREÇO", "discoverButton");
        Button cancel = PlatformUi.Button("CANCELAR", "cancelButton");
        PlatformUi.SetButtonTone(refreshPorts, PlatformButtonTone.Ghost);
        PlatformUi.SetButtonTone(cancel, PlatformButtonTone.Danger);
        foreach (Button button in new[] { refreshPorts, validate, identify, discover, cancel })
        {
            button.Width = 172;
            button.Height = IndustrialSpacing.FieldHeight;
            button.Margin = new Padding(IndustrialSpacing.Xs, 0, IndustrialSpacing.Xs, 0);
            actions.Controls.Add(button);
        }

        validate.Click += (_, _) => ValidateConfiguration();
        identify.Click += async (_, _) => await IdentifyAsync();
        discover.Click += async (_, _) => await DiscoverAsync();
        cancel.Click += (_, _) => _operation?.Cancel();
        grid.Controls.Add(actions, 0, 2);
        grid.SetColumnSpan(actions, 5);
        _offlineSafety.Dock = DockStyle.Fill;
        _offlineSafety.AutoSize = false;
        _offlineSafety.Margin = new Padding(IndustrialSpacing.Xs, 0, IndustrialSpacing.Xs, 0);
        grid.Controls.Add(_offlineSafety, 0, 3);
        grid.SetColumnSpan(_offlineSafety, 5);
        group.Controls.Add(grid);
        return group;
    }

    private Control BuildStatus()
    {
        TableLayoutPanel status = new()
        {
            Dock = DockStyle.Fill,
            ColumnCount = 3,
            Padding = new Padding(
                IndustrialSpacing.Md,
                IndustrialSpacing.Sm,
                IndustrialSpacing.Md,
                IndustrialSpacing.Sm),
            BackColor = PlatformUi.Surface,
            Name = "testerStatus"
        };
        status.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 190F));
        status.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        status.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 360F));
        _state.Font = IndustrialTypography.BodyStrong();
        _state.Dock = DockStyle.Fill;
        _result.Dock = DockStyle.Fill;
        _counters.Dock = DockStyle.Fill;
        status.Controls.Add(_state, 0, 0);
        status.Controls.Add(_result, 1, 0);
        status.Controls.Add(_counters, 2, 0);
        return status;
    }

    private Control BuildTabs()
    {
        _tabs.TabPages.Add(BuildDigitalInputsTab());
        _tabs.TabPages.Add(BuildAnalogInputsTab());
        _tabs.TabPages.Add(BuildOutputsTab());
        _tabs.TabPages.Add(BuildIoMappingTab());
        _tabs.TabPages.Add(BuildDiagnosticsTab());
        _tabs.TabPages.Add(BuildLogTab());
        return _tabs;
    }

    private TabPage BuildDigitalInputsTab()
    {
        TabPage page = Page("Entradas digitais");
        FlowLayoutPanel list = SignalList();
        for (int index = 0; index < _digitalValues.Length; index++)
        {
            string registerAlias = $"DI{index:00}";
            list.Controls.Add(SignalRow(
                registerAlias,
                GetProcessAlias(SimulationIoDirection.Input, SimulationIoType.Digital, index),
                _digitalValues[index],
                $"tester{registerAlias}Alias"));
        }

        Button read = PlatformUi.Button("Ler entradas simuladas", "readInputsButton", primary: true);
        read.Width = 220;
        read.Click += async (_, _) => await ReadInputsAsync();
        list.Controls.Add(read);
        page.Controls.Add(list);
        return page;
    }

    private TabPage BuildAnalogInputsTab()
    {
        TabPage page = Page("Entradas analógicas");
        FlowLayoutPanel list = SignalList();
        for (int index = 0; index < _analogValues.Length; index++)
        {
            string registerAlias = $"AI{index:00}";
            list.Controls.Add(SignalRow(
                registerAlias,
                GetProcessAlias(SimulationIoDirection.Input, SimulationIoType.Analog, index),
                _analogValues[index],
                $"tester{registerAlias}Alias"));
        }

        page.Controls.Add(list);
        return page;
    }

    private TabPage BuildOutputsTab()
    {
        TabPage page = Page("Saídas digitais");
        FlowLayoutPanel list = SignalList();
        _enableOutputs.ForeColor = IndustrialTheme.Palette.Warning;
        _enableOutputs.AccessibleDescription = "Autoriza somente saída simulada momentânea; hardware permanece bloqueado";
        _enableOutputs.Margin = new Padding(8, 10, 16, 10);
        list.Controls.Add(_enableOutputs);
        list.Controls.Add(PlatformUi.Label("Duracao ms"));
        list.Controls.Add(_outputDuration);
        Layout3OutputChannel[] channels = Enum.GetValues<Layout3OutputChannel>();
        for (int index = 0; index < channels.Length; index++)
        {
            Layout3OutputChannel channel = channels[index];
            Button activate = PlatformUi.Button("Acionar", $"activate{channel}Button", primary: true);
            Button turnOff = PlatformUi.Button("Desligar", $"turnOff{channel}Button");
            activate.Width = 110;
            turnOff.Width = 110;
            activate.Click += async (_, _) => await ActivateOutputAsync(channel);
            turnOff.Click += async (_, _) => await TurnOutputOffAsync(channel);
            string registerAlias = channel.ToString();
            FlowLayoutPanel row = SignalRow(
                registerAlias,
                GetProcessAlias(SimulationIoDirection.Output, SimulationIoType.Digital, index),
                _outputValues[index],
                $"tester{registerAlias}Alias");
            row.Controls.Add(activate);
            row.Controls.Add(turnOff);
            list.Controls.Add(row);
        }

        page.Controls.Add(list);
        return page;
    }

    private TabPage BuildIoMappingTab()
    {
        TabPage page = Page("Mapa de I/O");
        page.Name = "ioMappingTab";
        page.AccessibleName = "Mapa de I/O do perfil de simulação";
        _ioMap = new IndustrialIoMapControl(_session);
        page.Controls.Add(_ioMap);
        return page;
    }

    private TabPage BuildDiagnosticsTab()
    {
        TabPage page = Page("Diagnóstico");
        TableLayoutPanel diagnostics = new()
        {
            Dock = DockStyle.Top,
            AutoSize = true,
            AutoSizeMode = AutoSizeMode.GrowAndShrink,
            ColumnCount = 2,
            RowCount = 3,
            Padding = new Padding(IndustrialSpacing.Md),
            AccessibleName = "Resumo de diagnóstico"
        };
        diagnostics.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 180F));
        diagnostics.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        AddDiagnosticRow(diagnostics, 0, "ASSINATURA", "PROG_ID 31134 • PROG_CRC 23248 • F21 0");
        AddDiagnosticRow(diagnostics, 1, "FIRMWARE DE REFERÊNCIA", "G5PLC.C950.ST [3.3.11]");
        AddDiagnosticRow(
            diagnostics,
            2,
            "PERFIL",
            "NEON5-1S / CPU450 / HIO115 • alias OMNI-PLC2 compatível, não confirmado");
        page.Controls.Add(diagnostics);
        return page;
    }

    private static void AddDiagnosticRow(
        TableLayoutPanel diagnostics,
        int row,
        string captionText,
        string valueText)
    {
        diagnostics.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        Label caption = PlatformUi.Label(captionText, heading: true);
        Label value = PlatformUi.Label(valueText);
        caption.Dock = DockStyle.Fill;
        value.Dock = DockStyle.Fill;
        value.AutoSize = true;
        value.MaximumSize = new Size(760, 0);
        value.AccessibleName = captionText;
        diagnostics.Controls.Add(caption, 0, row);
        diagnostics.Controls.Add(value, 1, row);
    }

    private TabPage BuildLogTab()
    {
        TabPage page = Page("Log");
        PlatformUi.StyleField(_log);
        _log.Font = IndustrialTypography.Technical();
        page.Controls.Add(_log);
        return page;
    }

    private void ValidateConfiguration()
    {
        bool valid = !string.IsNullOrWhiteSpace(_port.Text)
            && _physicalLayer.Text is "RS232" or "RS485"
            && int.TryParse(_baud.Text, out int baud) && baud > 0
            && int.TryParse(_dataBits.Text, out int bits) && bits is 7 or 8
            && _parity.Text is "None" or "Even" or "Odd"
            && _stopBits.Text is "1" or "2"
            && _timeout.Value > 0
            && _interval.Value >= 0
            && _startAddress.Value >= 1
            && _endAddress.Value <= 247
            && _startAddress.Value <= _endAddress.Value;
        SetState(
            valid ? "PRONTO OFFLINE" : "CONFIGURAÇÃO INVÁLIDA",
            valid ? PlatformStatusTone.Normal : PlatformStatusTone.Fault);
        _result.Text = valid
            ? "Configuração válida para transporte em memória. Comunicação real permanece OFF."
            : "Configuração RTU inválida.";
    }

    private async Task IdentifyAsync()
    {
        await RunOperationAsync(async token =>
        {
            byte address = checked((byte)_startAddress.Value);
            RtuIdentificationResult result = await _session.IdentifyAsync(address, token);
            _identified = result.State == RtuIdentificationState.Identified;
            ShowIdentification(result);
        });
    }

    private async Task DiscoverAsync()
    {
        await RunOperationAsync(async token =>
        {
            Progress<RtuDiscoveryProgress> progress = new(item =>
                _result.Text = $"Tentativa {item.Attempt}/{item.Total} | endereco {item.Address} | {item.State?.ToString() ?? "AGUARDANDO"}");
            RtuDiscoveryResult result = await _session.DiscoverAsync(
                checked((byte)_startAddress.Value),
                checked((byte)_endAddress.Value),
                TimeSpan.FromMilliseconds((double)_interval.Value),
                progress,
                token);
            _identified = result.Match?.State == RtuIdentificationState.Identified;
            if (result.Match is not null)
            {
                ShowIdentification(result.Match);
            }
            else
            {
                _result.Text = result.Cancelled
                    ? "Descoberta cancelada."
                    : $"Nenhuma assinatura valida em {result.Attempts.Count} tentativa(s).";
            }
        });
    }

    private async Task ReadInputsAsync()
    {
        await RunOperationAsync(async token =>
        {
            RtuInputSnapshot snapshot = await _session.ReadInputsAsync(
                _session.LastIdentification?.Address ?? IndustrialPlatformSession.SimulatedDeviceAddress,
                token);
            for (int index = 0; index < snapshot.DigitalInputs.Length; index++)
            {
                _digitalValues[index].Text = snapshot.DigitalInputs[index] ? "● ON" : "○ OFF";
                _digitalValues[index].ForeColor = snapshot.DigitalInputs[index]
                    ? PlatformUi.Success
                    : PlatformUi.Muted;
            }

            for (int index = 0; index < snapshot.AnalogInputs.Length; index++)
            {
                _analogValues[index].Text = $"● {snapshot.AnalogInputs[index]} raw";
            }

            _result.Text = "Entradas lidas do equipamento simulado em memoria.";
        });
    }

    private async Task ActivateOutputAsync(Layout3OutputChannel channel)
    {
        await RunOperationAsync(async token =>
        {
            RtuOutputTestResult result = await _session.ActivateOutputAsync(
                _session.LastIdentification?.Address ?? IndustrialPlatformSession.SimulatedDeviceAddress,
                channel,
                TimeSpan.FromMilliseconds((double)_outputDuration.Value),
                _enableOutputs.Checked,
                token);
            _result.Text = $"{channel}: {result.State} | {result.Detail}";
        });
    }

    private async Task TurnOutputOffAsync(Layout3OutputChannel channel)
    {
        await RunOperationAsync(async token =>
        {
            await _session.TurnOutputOffAsync(
                _session.LastIdentification?.Address ?? IndustrialPlatformSession.SimulatedDeviceAddress,
                channel,
                token);
            _result.Text = $"{channel}: desligamento simulado confirmado.";
        });
    }

    private async Task RunOperationAsync(Func<CancellationToken, Task> operation)
    {
        _operation?.Dispose();
        _operation = new CancellationTokenSource();
        try
        {
            await operation(_operation.Token);
        }
        catch (OperationCanceledException)
        {
            _result.Text = "Operacao cancelada.";
        }
        catch (Exception ex)
        {
            SetState(
                "OPERAÇÃO BLOQUEADA",
                PlatformStatusTone.Fault);
            _result.Text = ex.Message;
        }
        finally
        {
            RefreshStatus();
        }
    }

    private void ShowIdentification(RtuIdentificationResult result)
    {
        SetState(
            result.State == RtuIdentificationState.Identified
                ? "SIMULAÇÃO PRONTA"
                : "PREPARAÇÃO DE BANCADA NECESSÁRIA",
            result.State == RtuIdentificationState.Identified
                ? PlatformStatusTone.Normal
                : PlatformStatusTone.Attention);
        _result.Text = $"Endereco {result.Address} | {result.State} | ID {result.ProgramId?.ToString() ?? "-"} | "
            + $"CRC {result.ProgramCrc?.ToString() ?? "-"} | F21 {result.GeneralFailureStatus?.ToString() ?? "-"}";
    }

    private void SessionChanged(object? sender, EventArgs e) => RefreshStatus();

    private void RefreshStatus()
    {
        Layout3OutputChannel[] channels = Enum.GetValues<Layout3OutputChannel>();
        for (int index = 0; index < channels.Length; index++)
        {
            bool active = _session.Device.GetDigitalOutput(channels[index]);
            _outputValues[index].Text = active ? "● ON" : "○ OFF";
            _outputValues[index].ForeColor = active ? PlatformUi.Success : PlatformUi.Muted;
        }

        _counters.Text = $"SIM C/R/W/CMD: {_session.Counters.SimulatedConnections}/"
            + $"{_session.Counters.SimulatedReads}/{_session.Counters.SimulatedWrites}/{_session.Counters.SimulatedCommands}"
            + "   |   FISICO: 0/0/0/0";
        UpdateLogIncrementally(_session.FormatOperationLog());
    }

    private static void AddField(TableLayoutPanel grid, string label, Control field, int index)
    {
        TableLayoutPanel fieldContainer = new()
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 2,
            Margin = new Padding(IndustrialSpacing.Xs)
        };
        fieldContainer.RowStyles.Add(new RowStyle(SizeType.Absolute, 18F));
        fieldContainer.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
        Label caption = PlatformUi.Label(label);
        caption.Dock = DockStyle.Fill;
        caption.AutoEllipsis = true;
        caption.Margin = Padding.Empty;
        fieldContainer.Controls.Add(caption, 0, 0);
        field.Dock = DockStyle.Fill;
        field.Margin = Padding.Empty;
        field.AccessibleName = label;
        PlatformUi.StyleField(field);
        fieldContainer.Controls.Add(field, 0, 1);
        grid.Controls.Add(fieldContainer, index % 5, index / 5);
    }

    private static ComboBox Combo(params string[] items)
    {
        IndustrialComboBox combo = new() { DropDownStyle = ComboBoxStyle.DropDown, Height = 32 };
        combo.Items.AddRange(items);
        if (items.Length > 0)
        {
            combo.SelectedIndex = 0;
        }

        return combo;
    }

    private static NumericUpDown Number(decimal value, decimal minimum, decimal maximum) => new()
    {
        Minimum = minimum,
        Maximum = maximum,
        Value = value,
        Height = 30
    };

    private static TabPage Page(string text) => new(text)
    {
        BackColor = PlatformUi.Background,
        ForeColor = PlatformUi.Text,
        Padding = new Padding(IndustrialSpacing.Sm),
        AccessibleName = text
    };

    private string GetProcessAlias(
        SimulationIoDirection direction,
        SimulationIoType ioType,
        int channel)
    {
        SimulationIoBinding? binding = _session.FindBinding(direction, ioType, channel);
        SimulationSignalDefinition? signal = _session.FindSignal(binding?.SignalId);
        return signal?.Label ?? binding?.SignalLabel ?? "Sem binding no perfil";
    }

    private static FlowLayoutPanel SignalList()
    {
        FlowLayoutPanel list = new()
        {
            Dock = DockStyle.Fill,
            AutoScroll = true,
            FlowDirection = FlowDirection.TopDown,
            WrapContents = false,
            BackColor = PlatformUi.Background,
            Padding = new Padding(8)
        };
        list.ClientSizeChanged += (_, _) => ResizeSignalRows(list);
        return list;
    }

    private static void ResizeSignalRows(FlowLayoutPanel list)
    {
        int width = Math.Max(680, list.ClientSize.Width - list.Padding.Horizontal - 28);
        foreach (Control control in list.Controls)
        {
            if (Equals(control.Tag, "signal-row"))
            {
                control.Width = width;
            }
        }
    }

    private static FlowLayoutPanel SignalRow(
        string registerAlias,
        string processAlias,
        Label value,
        string processAliasControlName)
    {
        FlowLayoutPanel row = new()
        {
            Width = 900,
            Height = 44,
            BackColor = PlatformUi.Surface,
            Margin = new Padding(3, 3, 3, 6),
            Padding = new Padding(10, 4, 10, 4),
            WrapContents = false,
            Tag = "signal-row"
        };
        Label name = PlatformUi.Label(registerAlias, heading: true);
        name.Width = 100;
        name.Height = 28;
        name.AutoSize = false;
        name.TextAlign = ContentAlignment.MiddleLeft;
        Label process = PlatformUi.Label(processAlias);
        process.Name = processAliasControlName;
        process.Width = 270;
        process.Height = 28;
        process.AutoSize = false;
        process.AutoEllipsis = true;
        process.AccessibleDescription = processAlias;
        process.TextAlign = ContentAlignment.MiddleLeft;
        value.Width = 150;
        value.Height = 28;
        value.AutoSize = false;
        value.TextAlign = ContentAlignment.MiddleLeft;
        value.ForeColor = PlatformUi.Success;
        row.Controls.Add(name);
        row.Controls.Add(process);
        row.Controls.Add(value);
        return row;
    }

    internal void ApplyTheme()
    {
        IndustrialPalette palette = IndustrialTheme.Palette;
        BackColor = palette.Background;
        ForeColor = palette.TextPrimary;
        ApplyThemeToChildren(this);
        SetState(_stateText, _stateTone);
        PlatformUi.UpdateStatusChip(
            _purposeStatus,
            "LEITURA • DIAGNÓSTICO • RESULTADOS",
            PlatformStatusTone.Active);
        PlatformUi.UpdateStatusChip(
            _offlineSafety,
            "OFFLINE • RTU EM MEMÓRIA • FÍSICA BLOQUEADA",
            PlatformStatusTone.Offline);
        _enableOutputs.ForeColor = palette.Warning;
        _ioMap?.ApplyTheme();
        foreach (Label value in _digitalValues.Concat(_analogValues).Concat(_outputValues))
        {
            value.ForeColor = value.Text.Contains("ON", StringComparison.Ordinal)
                ? palette.Success
                : palette.TextSecondary;
        }

        _tabs.Invalidate();
    }

    private void ApplyThemeToChildren(Control root)
    {
        IndustrialPalette palette = IndustrialTheme.Palette;
        foreach (Control child in root.Controls)
        {
            child.ForeColor = child is Label label
                && label.Tag is not PlatformStatusTone
                && label.BorderStyle != BorderStyle.FixedSingle
                ? palette.TextSecondary
                : child.ForeColor;
            switch (child)
            {
                case IndustrialComboBox combo:
                    combo.ApplyTheme();
                    break;
                case TabPage:
                    child.BackColor = palette.Background;
                    break;
                case FlowLayoutPanel flow:
                    flow.BackColor = Equals(flow.Tag, "signal-row")
                        ? palette.SurfaceElevated
                        : palette.Background;
                    break;
                case GroupBox:
                    child.BackColor = palette.SurfaceElevated;
                    child.ForeColor = palette.TextPrimary;
                    break;
                case TextBox or ComboBox or NumericUpDown:
                    child.BackColor = palette.Field;
                    child.ForeColor = palette.TextPrimary;
                    break;
                case Button button:
                    PlatformUi.StyleButton(button, primary: IsPrimaryAction(button));
                    break;
                case TableLayoutPanel panel:
                    panel.BackColor = panel.Name is "testerHero" or "testerStatus"
                        ? palette.Surface
                        : panel.Parent is GroupBox
                            ? palette.SurfaceElevated
                            : palette.Background;
                    break;
            }

            ApplyThemeToChildren(child);
        }
    }

    private static bool IsPrimaryAction(Button button) => button.Name is
        "validateRtuButton" or "readInputsButton"
        || button.Name.StartsWith("activate", StringComparison.Ordinal);

    private void DrawTab(object? sender, DrawItemEventArgs e)
    {
        IndustrialPalette palette = IndustrialTheme.Palette;
        bool selected = e.Index == _tabs.SelectedIndex;
        using SolidBrush background = new(selected ? palette.SelectedSurface : palette.Surface);
        e.Graphics.FillRectangle(background, e.Bounds);
        TextRenderer.DrawText(
            e.Graphics,
            _tabs.TabPages[e.Index].Text,
            _tabs.Font,
            e.Bounds,
            selected ? palette.SelectedText : palette.TextSecondary,
            TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis);
        if (selected)
        {
            using SolidBrush accent = new(palette.Accent);
            e.Graphics.FillRectangle(accent, e.Bounds.Left, e.Bounds.Bottom - 3, e.Bounds.Width, 3);
        }

        if (selected && _tabs.Focused)
        {
            e.DrawFocusRectangle();
        }
    }

    private void UpdateLogIncrementally(IReadOnlyList<string> lines)
    {
        string updated = string.Join(Environment.NewLine, lines);
        if (string.Equals(updated, _renderedLog, StringComparison.Ordinal))
        {
            return;
        }

        if (_renderedLog.Length > 0
            && updated.StartsWith(_renderedLog + Environment.NewLine, StringComparison.Ordinal))
        {
            _log.AppendText(updated[_renderedLog.Length..]);
        }
        else
        {
            _log.Text = updated;
        }

        _renderedLog = updated;
    }

    private void SetState(string text, PlatformStatusTone tone)
    {
        _stateText = text;
        _stateTone = tone;
        PlatformUi.UpdateStatusChip(_state, text, tone);
    }
}
