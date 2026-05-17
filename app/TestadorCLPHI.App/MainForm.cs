using System.IO.Ports;
using TestadorCLPHI.App.Plc;
using TestadorCLPHI.App.Ui;
using TestadorCLPHI.App.Ui.Controls;

namespace TestadorCLPHI.App;

public sealed class MainForm : Form
{
    private readonly PlcConnectionSettings _connectionSettings = new();
    private readonly IPlcCommunicationService _plcService = new ModbusRtuPlcCommunicationService();
    private readonly PlcRegisterCommandService _registerCommandService;
    private readonly PlcAutoDetectionService _autoDetectionService;

    private readonly Label _tituloLabel;
    private readonly Label _statusLabel;
    private readonly Button _pararTudoButton;

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

    public MainForm()
    {
        Text = "Testador CLP HI";
        Width = 900;
        Height = 620;
        StartPosition = FormStartPosition.CenterScreen;

        _registerCommandService = new PlcRegisterCommandService(_plcService, _connectionSettings);
        _autoDetectionService = new PlcAutoDetectionService(_plcService);

        _tituloLabel = new Label
        {
            Text = "Testador CLP HI",
            AutoSize = true,
            Left = 20,
            Top = 25,
            Font = new Font("Segoe UI", 18, FontStyle.Bold)
        };

        _statusLabel = new Label
        {
            Text = "Comunicação Modbus RTU real em fase inicial.",
            AutoSize = true,
            Left = 20,
            Top = 80,
            Font = new Font("Segoe UI", 10)
        };

        _pararTudoButton = new Button
        {
            Text = "Parar tudo",
            Left = 20,
            Top = 130,
            Width = 160,
            Height = 40
        };

        _themeSelector = new ThemeSelectorControl
        {
            Left = 760,
            Top = 25
        };

        _connectionStatePanel = new ConnectionStatePanelControl
        {
            Left = 20,
            Top = 220
        };
        _testerCommandPanel = new TesterCommandPanelControl
        {
            Left = 20,
            Top = 490
        };

        _conexaoGroupBox = new GroupBox
        {
            Text = "Conexão com CLP",
            Left = 320,
            Top = 220,
            Width = 390,
            Height = 250
        };

        _portaTituloLabel = CriarLabel("Porta COM:", 15, 30);

        _portaComboBox = new ComboBox
        {
            Left = 110,
            Top = 27,
            Width = 120,
            DropDownStyle = ComboBoxStyle.DropDownList
        };

        _atualizarPortasButton = new Button
        {
            Text = "Atualizar portas",
            Left = 240,
            Top = 25,
            Width = 125,
            Height = 28
        };

        _baudRateTituloLabel = CriarLabel("Baud rate:", 15, 70);

        _baudRateComboBox = new ComboBox
        {
            Left = 110,
            Top = 67,
            Width = 120,
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

        _baudRateBuscaTituloLabel = CriarLabel("Busca:", 240, 65);

        _baudRateBuscaCheckedListBox = new CheckedListBox
        {
            Left = 240,
            Top = 88,
            Width = 125,
            Height = 55,
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
            Text = "Buscar todos",
            Left = 240,
            Top = 68,
            Width = 125,
            Height = 24,
            Checked = false
        };

        _slaveIdTituloLabel = CriarLabel("Slave ID:", 15, 110);

        _slaveIdTextBox = new TextBox
        {
            Left = 110,
            Top = 107,
            Width = 120,
            Text = _connectionSettings.SlaveId.ToString()
        };

        _detectarClpButton = new Button
        {
            Text = "Detectar CLP",
            Left = 195,
            Top = 150,
            Width = 160,
            Height = 32
        };

        _conexaoResumoLabel = new Label
        {
            AutoSize = false,
            Left = 15,
            Top = 195,
            Width = 350,
            Height = 45,
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

        Controls.Add(_tituloLabel);
        Controls.Add(_statusLabel);
        Controls.Add(_pararTudoButton);
        Controls.Add(_themeSelector);
        Controls.Add(_connectionStatePanel);
        Controls.Add(_conexaoGroupBox);
        Controls.Add(_testerCommandPanel);

        AtualizarListaDePortas();
        AtualizarResumoConexao();
        AtualizarEstadoConexao();
        AplicarTemaSelecionado();
    }

    private static Label CriarLabel(string text, int left, int top)
    {
        return new Label
        {
            Text = text,
            AutoSize = true,
            Left = left,
            Top = top,
            Font = new Font("Segoe UI", 10)
        };
    }

    private static Label CriarLabelEstadoMensagem(int left, int top)
    {
        return new Label
        {
            AutoSize = false,
            Left = left,
            Top = top,
            Width = 230,
            Height = 55,
            Font = new Font("Segoe UI", 10)
        };
    }

    private void AtualizarListaDePortas()
    {
        PlcConnectionSettingsUiService.UpdatePortList(
            _portaComboBox,
            _connectionSettings.PortName);
    }
    private void AtualizarResumoConexao()
    {
        _conexaoResumoLabel.Text =
            $"Atual: {_connectionSettings.PortName}, {_connectionSettings.BaudRate} bps, Slave {_connectionSettings.SlaveId}" +
            $"{Environment.NewLine}Paridade: {_connectionSettings.Parity}, Stop bits: {_connectionSettings.StopBits}, Timeout: {_connectionSettings.TimeoutMilliseconds} ms";
    }

    private void AtualizarEstadoConexao()
    {
        PlcConnectionState state = _plcService.State;

        _connectionStatePanel.UpdateState(state);
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
        return PlcConnectionSettingsUiService.GetSelectedBaudRatesForSearch(
            _baudRateComboBox,
            _baudRateBuscaCheckedListBox,
            _connectionSettings.BaudRate);
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

    private async System.Threading.Tasks.Task ExecutarComandoRegistradorAsync(
        int registerAddress,
        ushort valueToWrite,
        string commandName,
        string dialogTitle,
        string errorTitle)
    {
        if (!TryUpdateConnectionSettingsFromUi())
        {
            return;
        }

        try
        {
            PlcRegisterCommandResult result =
                await _registerCommandService.WriteAndReadBackAsync(
                    registerAddress,
                    valueToWrite);

            AtualizarEstadoConexao();

            MessageBox.Show(
                $"{commandName} aplicado: %MW{result.RegisterAddress} = {result.ReadBackValue}",
                dialogTitle,
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
        }
        catch (Exception ex)
        {
            _plcService.State.SetError(ex.Message);
            AtualizarEstadoConexao();

            MessageBox.Show(
                ex.Message,
                errorTitle,
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
        }
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





