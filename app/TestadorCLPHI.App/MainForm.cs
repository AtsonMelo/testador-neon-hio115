using System.IO.Ports;
using TestadorCLPHI.App.Plc;
using TestadorCLPHI.App.Ui;
using TestadorCLPHI.App.Ui.Controls;
using TestadorCLPHI.App.Ui.Industrial;

namespace TestadorCLPHI.App;

public sealed class MainForm : Form
{
    private readonly PlcConnectionSettings _connectionSettings = new();
    private readonly IPlcCommunicationService _plcService = new ModbusRtuPlcCommunicationService();
    private readonly PlcRegisterCommandService _registerCommandService;
    private readonly PlcAutoDetectionService _autoDetectionService;
    private readonly PlcDigitalIoManualService _digitalIoManualService;
    private readonly MainFormCommandUiService _commandUiService;
    private readonly MainFormDigitalIoUiService _digitalIoUiService;
    private readonly bool _useIndustrialHost;

    private readonly Label _tituloLabel;
    private readonly Label _statusLabel;
    private readonly EmergencyStopButtonControl _pararTudoButton;

    private readonly ThemeSelectorControl _themeSelector;

    private readonly GroupBox _conexaoGroupBox;
    private readonly Label _portaTituloLabel;
    private readonly ComboBox _portaComboBox;
    private readonly Button _atualizarPortasButton;

    private readonly Label _baudRateTituloLabel;
    private readonly ComboBox _baudRateComboBox;
    private readonly Label _baudRateBuscaTituloLabel;
    private readonly CheckedListBox _baudRateBuscaCheckedListBox;

    private readonly Label _slaveIdTituloLabel;
    private readonly TextBox _slaveIdTextBox;

    private readonly Label _conexaoResumoLabel;
    private readonly Button _detectarClpButton;
    private readonly CheckBox _buscarTodosBaudRatesCheckBox;

    private readonly ConnectionStatePanelControl _connectionStatePanel;
    private readonly TesterCommandPanelControl _testerCommandPanel;
    private readonly DigitalIoManualPanelControl _digitalIoManualPanel;
    private readonly TerminalLogPanelControl _terminalLogPanel;

    public MainForm(bool useIndustrialHost = false)
    {
        _useIndustrialHost = useIndustrialHost;

        Text = "Testador CLP HI";
        Width = 900;
        Height = 740;
        StartPosition = FormStartPosition.CenterScreen;
        AutoScaleMode = AutoScaleMode.Font;
        MinimumSize = new Size(900, 700);
        AutoScroll = true;

        _registerCommandService = new PlcRegisterCommandService(_plcService, _connectionSettings);
        _autoDetectionService = new PlcAutoDetectionService(_plcService);
        _digitalIoManualService = new PlcDigitalIoManualService(_registerCommandService);
        _commandUiService = new MainFormCommandUiService(
            this,
            _registerCommandService,
            _plcService,
            TryUpdateConnectionSettingsFromUi,
            AtualizarEstadoConexao);

        _tituloLabel = new Label
        {
            Text = "Testador CLP HI",
            AutoSize = true,
            Left = 20,
            Top = 20,
            Font = new Font("Segoe UI", 17, FontStyle.Bold)
        };

        _statusLabel = new Label
        {
            Text = "Comunicação Modbus RTU real em fase inicial.",
            AutoSize = true,
            Left = 20,
            Top = 92,
            Font = new Font("Segoe UI", 10)
        };

        _pararTudoButton = new EmergencyStopButtonControl
        {
            Title = "STOP",
            ButtonImagePath = System.IO.Path.Combine(AppContext.BaseDirectory, "Assets", "Ui", "stop_emergency.png"),
            Left = 755,
            Top = 88,
            Width = 86,
            Height = 78,
            Anchor = AnchorStyles.Top | AnchorStyles.Right,
            BackColor = Color.FromArgb(31, 31, 31),
            ForeColor = Color.White,
            Cursor = Cursors.Hand
        };

        _themeSelector = new ThemeSelectorControl
        {
            Left = 760,
            Top = 20,
            Anchor = AnchorStyles.Top | AnchorStyles.Right
        };

        _connectionStatePanel = new ConnectionStatePanelControl
        {
            Left = 20,
            Top = 45
        };
        _testerCommandPanel = new TesterCommandPanelControl
        {
            Left = 20,
            Top = 305,
            Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
        };

        _digitalIoManualPanel = new DigitalIoManualPanelControl
        {
            Left = 20,
            Top = 390,
            Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
        };
        _digitalIoUiService = new MainFormDigitalIoUiService(
            this,
            _digitalIoManualService,
            _plcService,
            _digitalIoManualPanel,
            _statusLabel,
            TryUpdateConnectionSettingsFromUi,
            AtualizarEstadoConexao);

        _terminalLogPanel = new TerminalLogPanelControl
        {
            Left = 20,
            Top = 620,
            Width = 690,
            Height = 150,
            Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
        };

        _conexaoGroupBox = new GroupBox
        {
            Text = "Conexão com CLP",
            Left = 320,
            Top = 45,
            Width = 390,
            Height = 250,
            Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
        };

        _portaTituloLabel = MainFormControlFactory.CriarLabel("Porta COM:", 20, 32);

        _portaComboBox = new ComboBox
        {
            Left = 145,
            Top = 28,
            Width = 170,
            DropDownStyle = ComboBoxStyle.DropDownList
        };

        _atualizarPortasButton = new Button
        {
            Text = "Atualizar portas",
            Left = 335,
            Top = 27,
            Width = 170,
            Height = 30
        };

        _baudRateTituloLabel = MainFormControlFactory.CriarLabel("Baud rate:", 20, 72);

        _baudRateComboBox = new ComboBox
        {
            Left = 145,
            Top = 68,
            Width = 170,
            DropDownStyle = ComboBoxStyle.DropDownList
        };

        _baudRateComboBox.Items.AddRange(new object[]
        {
            "9600",
            "19200",
            "38400",
            "57600",
            "115200"
        });

        _baudRateComboBox.SelectedItem = _connectionSettings.BaudRate.ToString();

        _baudRateBuscaTituloLabel = MainFormControlFactory.CriarLabel("Busca:", 240, 65);

        _baudRateBuscaCheckedListBox = new CheckedListBox
        {
            Left = 335,
            Top = 98,
            Width = 170,
            Height = 72,
            CheckOnClick = true,
            IntegralHeight = false
        };

        _baudRateBuscaCheckedListBox.Items.Add("9600", true);
        _baudRateBuscaCheckedListBox.Items.Add("19200", true);
        _baudRateBuscaCheckedListBox.Items.Add("38400", true);
        _baudRateBuscaCheckedListBox.Items.Add("57600", false);
        _baudRateBuscaCheckedListBox.Items.Add("115200", false);

        _buscarTodosBaudRatesCheckBox = new CheckBox
        {
            Text = "Todos",
            Left = 395,
            Top = 68,
            Width = 90,
            Height = 24,
            Checked = false
        };

        _slaveIdTituloLabel = MainFormControlFactory.CriarLabel("Slave ID:", 20, 112);

        _slaveIdTextBox = new TextBox
        {
            Left = 145,
            Top = 108,
            Width = 170,
            Text = _connectionSettings.SlaveId.ToString()
        };

        _detectarClpButton = new Button
        {
            Text = "Detectar CLP",
            Left = 335,
            Top = 182,
            Width = 170,
            Height = 34
        };

        _conexaoResumoLabel = new Label
        {
            AutoSize = false,
            Left = 20,
            Top = 178,
            Width = 300,
            Height = 55,
            Font = new Font("Segoe UI", 9)
        };

        _pararTudoButton.Click += PararTudoButton_Click;
        _themeSelector.ThemeChanged += (_, _) => AplicarTemaSelecionado();

        _atualizarPortasButton.Click += AtualizarPortasButton_Click;
        _detectarClpButton.Click += DetectarClpButton_Click;

        _connectionStatePanel.ConnectClicked += ConectarClpButton_Click;
        _connectionStatePanel.SimulateErrorClicked += SimularErroButton_Click;
        _connectionStatePanel.DisconnectClicked += DesconectarButton_Click;
        _connectionStatePanel.ReadMw70Clicked += LerMw70Button_Click;
        _testerCommandPanel.EnableTestClicked += HabilitarTesteButton_Click;
        _testerCommandPanel.ResetOutputsClicked += ResetarSaidasButton_Click;
        _digitalIoManualPanel.OutputCommandClicked += AcionarSaidaDigitalManual;
        _digitalIoManualPanel.RefreshInputsClicked += AtualizarEntradasDigitaisButton_Click;

        _conexaoGroupBox.Controls.Add(_portaTituloLabel);
        _conexaoGroupBox.Controls.Add(_portaComboBox);
        _conexaoGroupBox.Controls.Add(_atualizarPortasButton);
        _conexaoGroupBox.Controls.Add(_baudRateTituloLabel);
        _conexaoGroupBox.Controls.Add(_baudRateComboBox);
        _conexaoGroupBox.Controls.Add(_baudRateBuscaTituloLabel);
        _conexaoGroupBox.Controls.Add(_baudRateBuscaCheckedListBox);
        _conexaoGroupBox.Controls.Add(_buscarTodosBaudRatesCheckBox);
        _conexaoGroupBox.Controls.Add(_slaveIdTituloLabel);
        _conexaoGroupBox.Controls.Add(_slaveIdTextBox);
        _conexaoGroupBox.Controls.Add(_detectarClpButton);
        _conexaoGroupBox.Controls.Add(_conexaoResumoLabel);
        if (_useIndustrialHost)
        {
            Controls.Add(new IndustrialMainHostControl());
            AtualizarListaDePortas();
            AtualizarResumoConexao();
            AtualizarEstadoConexao();
            AplicarTemaSelecionado();
            return;
        }

        TableLayoutPanel rootLayout = new()
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 4,
            Padding = new Padding(14),
            AutoScroll = true
        };

        rootLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        rootLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 92F));
        rootLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 292F));
        rootLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 70F));
        rootLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));

        FlowLayoutPanel actionsPanel = new()
        {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.RightToLeft,
            WrapContents = false,
            Margin = new Padding(0, 0, 0, 8),
            Padding = new Padding(0, 0, 8, 0)
        };

        _themeSelector.Width = 108;
        _themeSelector.Height = 34;
        _themeSelector.Margin = new Padding(12, 12, 0, 0);
        _themeSelector.Anchor = AnchorStyles.None;

        _pararTudoButton.Width = 86;
        _pararTudoButton.Height = 78;
        _pararTudoButton.Margin = new Padding(12, 4, 0, 0);
        _pararTudoButton.Anchor = AnchorStyles.None;

        actionsPanel.Controls.Add(_themeSelector);
        actionsPanel.Controls.Add(_pararTudoButton);

        TableLayoutPanel connectionLayout = new()
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            RowCount = 1,
            Margin = new Padding(0, 0, 0, 10)
        };

        connectionLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 310F));
        connectionLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        connectionLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));

        _connectionStatePanel.Dock = DockStyle.Fill;
        _connectionStatePanel.Margin = new Padding(0, 0, 14, 0);

        _conexaoGroupBox.Dock = DockStyle.Fill;
        _conexaoGroupBox.Margin = new Padding(0);
        _conexaoGroupBox.MinimumSize = new Size(560, 250);

        connectionLayout.Controls.Add(_connectionStatePanel, 0, 0);
        connectionLayout.Controls.Add(_conexaoGroupBox, 1, 0);

        _testerCommandPanel.Dock = DockStyle.Fill;
        _testerCommandPanel.Margin = new Padding(0, 0, 0, 10);

        _digitalIoManualPanel.Dock = DockStyle.Top;
        _digitalIoManualPanel.Margin = new Padding(0, 0, 0, 10);

        _terminalLogPanel.Dock = DockStyle.Fill;
        _terminalLogPanel.Margin = new Padding(0);

        rootLayout.Controls.Add(actionsPanel, 0, 0);
        rootLayout.Controls.Add(connectionLayout, 0, 1);
        TabControl workTabs = new()
        {
            Dock = DockStyle.Fill,
            Margin = new Padding(0)
        };

        TabPage ioTabPage = new("I/O Manual")
        {
            Padding = new Padding(6),
            AutoScroll = true
        };

        TabPage terminalTabPage = new("Terminal / Log")
        {
            Padding = new Padding(6),
            AutoScroll = true
        };

        _digitalIoManualPanel.Dock = DockStyle.Top;
        _digitalIoManualPanel.Margin = new Padding(0);

        _terminalLogPanel.Dock = DockStyle.Fill;
        _terminalLogPanel.Margin = new Padding(0);

        ioTabPage.Controls.Add(_digitalIoManualPanel);
        terminalTabPage.Controls.Add(_terminalLogPanel);

        workTabs.TabPages.Add(ioTabPage);
        workTabs.TabPages.Add(terminalTabPage);

        rootLayout.Controls.Add(_testerCommandPanel, 0, 2);
        rootLayout.Controls.Add(workTabs, 0, 3);

        Controls.Add(rootLayout);

        AtualizarListaDePortas();
        AtualizarResumoConexao();
        AtualizarEstadoConexao();
        AplicarTemaSelecionado();
    }
    private void AtualizarListaDePortas()
    {
        MainFormConnectionUiService.AtualizarListaDePortas(
            _portaComboBox,
            _connectionSettings);
    }
    private void AtualizarResumoConexao()
    {
        MainFormConnectionUiService.AtualizarResumoConexao(
            _conexaoResumoLabel,
            _connectionSettings);
    }
    private void AtualizarEstadoConexao()
    {
        MainFormConnectionUiService.AtualizarEstadoConexao(
            _connectionStatePanel,
            _plcService.State);
    }

    private void AtualizarPortasButton_Click(object? sender, EventArgs e)
    {
        AtualizarListaDePortas();
    }

    private bool TryUpdateConnectionSettingsFromUi()
    {
        if (!PlcConnectionSettingsUiService.TryUpdateSettingsFromUi(
                _portaComboBox,
                _baudRateComboBox,
                _slaveIdTextBox,
                _connectionSettings,
                out string? errorMessage))
        {
            MessageBox.Show(
                errorMessage,
                "Configuração inválida",
                MessageBoxButtons.OK,
                MessageBoxIcon.Warning);

            return false;
        }

        AtualizarResumoConexao();

        return true;
    }
    private int[] ObterBaudRatesSelecionadosParaBusca()
    {
        return MainFormConnectionUiService.ObterBaudRatesSelecionadosParaBusca(
            _baudRateComboBox,
            _baudRateBuscaCheckedListBox,
            _connectionSettings);
    }

    private async void DetectarClpButton_Click(object? sender, EventArgs e)
    {
        _detectarClpButton.Enabled = false;
        _connectionStatePanel.SetConnectionButtonsEnabled(false);

        try
        {
            if (!TryUpdateConnectionSettingsFromUi())
            {
                return;
            }

            string[] portasDisponiveis = SerialPort.GetPortNames()
                .OrderBy(porta => porta)
                .ToArray();

            List<string> portasOrdenadas = [];
            string? portaSelecionada = _portaComboBox.SelectedItem?.ToString();

            if (!string.IsNullOrWhiteSpace(portaSelecionada) &&
                portasDisponiveis.Contains(portaSelecionada))
            {
                portasOrdenadas.Add(portaSelecionada);
            }

            foreach (string porta in portasDisponiveis)
            {
                if (!portasOrdenadas.Contains(porta))
                {
                    portasOrdenadas.Add(porta);
                }
            }

            int[] baudRates = ObterBaudRatesSelecionadosParaBusca();

            byte? preferredSlaveId = null;

            if (byte.TryParse(_slaveIdTextBox.Text, out byte slaveIdSelecionado))
            {
                preferredSlaveId = slaveIdSelecionado;
            }

            Progress<string> progress = new(message =>
            {
                _statusLabel.Text = message;
            });

            PlcAutoDetectionResult? result =
                await _autoDetectionService.DetectAsync(
                    portasOrdenadas,
                    baudRates,
                    preferredSlaveId,
                    _connectionSettings,
                    Hio115MemoryMap.HabilitaTeste,
                    progress);

            if (result is null)
            {
                AtualizarEstadoConexao();
                _statusLabel.Text = _plcService.State.Message;
                return;
            }

            _connectionSettings.PortName = result.Settings.PortName;
            _connectionSettings.BaudRate = result.Settings.BaudRate;
            _connectionSettings.Parity = result.Settings.Parity;
            _connectionSettings.DataBits = result.Settings.DataBits;
            _connectionSettings.StopBits = result.Settings.StopBits;
            _connectionSettings.SlaveId = result.Settings.SlaveId;
            _connectionSettings.TimeoutMilliseconds = result.Settings.TimeoutMilliseconds;

            _portaComboBox.SelectedItem = result.Settings.PortName;
            _baudRateComboBox.SelectedItem = result.Settings.BaudRate.ToString();
            _slaveIdTextBox.Text = result.Settings.SlaveId.ToString();

            AtualizarResumoConexao();
            AtualizarEstadoConexao();

            _statusLabel.Text =
                $"CLP detectado em {result.Settings.PortName}, {result.Settings.BaudRate} bps, Slave {result.Settings.SlaveId} após {result.AttemptCount} tentativa(s).";
        }
        catch (Exception ex)
        {
            _plcService.State.SetError(ex.Message);
            AtualizarEstadoConexao();

            _statusLabel.Text = $"Erro na detecção automática: {ex.Message}";
        }
        finally
        {
            _detectarClpButton.Enabled = true;
            _connectionStatePanel.SetConnectionButtonsEnabled(true);
        }
    }
    private async void ConectarClpButton_Click(object? sender, EventArgs e)
    {
        if (!TryUpdateConnectionSettingsFromUi())
        {
            return;
        }

        try
        {
            await _plcService.ConnectAsync(_connectionSettings);

            ushort value = await _plcService.ReadHoldingRegisterAsync(
                Hio115MemoryMap.HabilitaTeste);

            _plcService.State.SetConnected(
                $"CLP respondeu Modbus: %MW{Hio115MemoryMap.HabilitaTeste} = {value}");

            AtualizarEstadoConexao();

            _statusLabel.Text =
                $"CLP conectado e validado em {_connectionSettings.PortName}, {_connectionSettings.BaudRate} bps, Slave {_connectionSettings.SlaveId}.";
        }
        catch (Exception ex)
        {
            _plcService.State.SetError(ex.Message);
            AtualizarEstadoConexao();

            _statusLabel.Text =
                $"Falha ao validar comunicação em {_connectionSettings.PortName}, {_connectionSettings.BaudRate} bps, Slave {_connectionSettings.SlaveId}.";
        }
    }
    private void SimularErroButton_Click(object? sender, EventArgs e)
    {
        _plcService.State.SetError("Erro simulado de comunicação");
        AtualizarEstadoConexao();
    }

    private async void DesconectarButton_Click(object? sender, EventArgs e)
    {
        try
        {
            await _plcService.DisconnectAsync();
        }
        catch (Exception ex)
        {
            _plcService.State.SetError(ex.Message);
        }

        AtualizarEstadoConexao();
    }

    private async void LerMw70Button_Click(object? sender, EventArgs e)
    {
        if (!TryUpdateConnectionSettingsFromUi())
        {
            return;
        }

        try
        {
            ushort value = await _registerCommandService.ReadAsync(
                Hio115MemoryMap.HabilitaTeste);

            AtualizarEstadoConexao();

            MessageBox.Show(
                $"Leitura Modbus RTU: %MW{Hio115MemoryMap.HabilitaTeste} = {value}",
                "Leitura %MW70",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
        }
        catch (Exception ex)
        {
            _plcService.State.SetError(ex.Message);
            AtualizarEstadoConexao();

            MessageBox.Show(
                ex.Message,
                "Erro na leitura %MW70",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
        }
    }
    private async void HabilitarTesteButton_Click(object? sender, EventArgs e)
    {
        await ExecutarComandoRegistradorAsync(
            Hio115MemoryMap.HabilitaTeste,
            1,
            "HABILITA_TESTE",
            "Comando CLP",
            "Erro no comando CLP");
    }
    private async void ResetarSaidasButton_Click(object? sender, EventArgs e)
    {
        await ExecutarComandoRegistradorAsync(
            Hio115MemoryMap.ResetSaidas,
            1,
            "RESET_SAIDAS",
            "Comando CLP",
            "Erro no comando CLP");
    }

    private async void AcionarSaidaDigitalManual(int channel)
    {
        await _digitalIoUiService.AcionarSaidaDigitalManualAsync(channel);
    }

    private async void AtualizarEntradasDigitaisButton_Click(object? sender, EventArgs e)
    {
        await AtualizarEntradasDigitaisAsync();
    }

    private async System.Threading.Tasks.Task AtualizarEntradasDigitaisAsync()
    {
        await _digitalIoUiService.AtualizarEntradasDigitaisAsync();
    }

    private async System.Threading.Tasks.Task ExecutarComandoRegistradorAsync(
        int registerAddress,
        ushort valueToWrite,
        string commandName,
        string dialogTitle,
        string errorTitle)
    {
        await _commandUiService.ExecutarComandoRegistradorAsync(
            registerAddress,
            valueToWrite,
            commandName,
            dialogTitle,
            errorTitle);
    }

    private async void PararTudoButton_Click(object? sender, EventArgs e)
    {
        await ExecutarComandoRegistradorAsync(
            Hio115MemoryMap.ResetSaidas,
            1,
            "RESET_SAIDAS",
            "Parar tudo",
            "Erro no parar tudo");
    }
    private void AplicarTemaSelecionado()
    {
        bool temaEscuro = _themeSelector.SelectedTheme switch
        {
            ThemeSelection.Windows => AppThemeService.WindowsIsInDarkTheme(),
            ThemeSelection.Dark => true,
            _ => false
        };

        AppThemeService.ApplyTheme(this, _themeSelector.ThemeMenu, temaEscuro);
    }
}
