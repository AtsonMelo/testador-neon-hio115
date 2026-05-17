using System.IO.Ports;
using TestadorCLPHI.App.Plc;
using TestadorCLPHI.App.Ui;

namespace TestadorCLPHI.App;

public sealed class MainForm : Form
{
    private enum ThemeSelection
    {
        Windows,
        Light,
        Dark
    }

    private readonly PlcConnectionSettings _connectionSettings = new();
    private readonly IPlcCommunicationService _plcService = new ModbusRtuPlcCommunicationService();
    private readonly PlcRegisterCommandService _registerCommandService;

    private ThemeSelection _themeSelection = ThemeSelection.Windows;

    private readonly Label _tituloLabel;
    private readonly Label _statusLabel;
    private readonly Button _pararTudoButton;

    private readonly Button _temaButton;
    private readonly ContextMenuStrip _temaMenu;
    private readonly ToolStripMenuItem _usarTemaWindowsMenuItem;
    private readonly ToolStripMenuItem _temaClaroMenuItem;
    private readonly ToolStripMenuItem _temaEscuroMenuItem;

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

    private readonly GroupBox _estadoGroupBox;
    private readonly Label _estadoStatusLabel;
    private readonly Label _estadoMensagemLabel;
    private readonly Label _estadoAtualizacaoLabel;
    private readonly Button _conectarClpButton;
    private readonly Button _simularErroButton;
    private readonly Button _desconectarButton;
    private readonly Button _lerMw70Button;

    private readonly GroupBox _comandosGroupBox;
    private readonly Button _habilitarTesteButton;
    private readonly Button _resetarSaidasButton;

    public MainForm()
    {
        Text = "Testador CLP HI";
        Width = 900;
        Height = 620;
        StartPosition = FormStartPosition.CenterScreen;

        _registerCommandService = new PlcRegisterCommandService(_plcService, _connectionSettings);

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

        _temaButton = new Button
        {
            Text = "Tema",
            Left = 760,
            Top = 25,
            Width = 90,
            Height = 32
        };

        _temaMenu = new ContextMenuStrip();

        _usarTemaWindowsMenuItem = new ToolStripMenuItem("Usar tema do Windows");
        _temaClaroMenuItem = new ToolStripMenuItem("Claro");
        _temaEscuroMenuItem = new ToolStripMenuItem("Escuro");

        _temaMenu.Items.Add(_usarTemaWindowsMenuItem);
        _temaMenu.Items.Add(_temaClaroMenuItem);
        _temaMenu.Items.Add(_temaEscuroMenuItem);

        _estadoGroupBox = new GroupBox
        {
            Text = "Estado da conexão",
            Left = 20,
            Top = 220,
            Width = 270,
            Height = 250
        };

        _estadoStatusLabel = CriarLabel(string.Empty, 15, 30);
        _estadoMensagemLabel = CriarLabelEstadoMensagem(15, 55);
        _estadoAtualizacaoLabel = CriarLabel(string.Empty, 15, 120);

        _conectarClpButton = new Button
        {
            Text = "Conectar CLP",
            Left = 15,
            Top = 155,
            Width = 115,
            Height = 28
        };

        _simularErroButton = new Button
        {
            Text = "Simular erro",
            Left = 140,
            Top = 155,
            Width = 105,
            Height = 28
        };

        _desconectarButton = new Button
        {
            Text = "Desconectar",
            Left = 15,
            Top = 195,
            Width = 115,
            Height = 28
        };

        _lerMw70Button = new Button
        {
            Text = "Ler %MW70",
            Left = 140,
            Top = 195,
            Width = 105,
            Height = 28
        };

        _comandosGroupBox = new GroupBox
        {
            Text = "Comandos do testador",
            Left = 20,
            Top = 490,
            Width = 690,
            Height = 75
        };

        _habilitarTesteButton = new Button
        {
            Text = "Habilitar teste",
            Left = 15,
            Top = 28,
            Width = 140,
            Height = 30
        };

        _resetarSaidasButton = new Button
        {
            Text = "Resetar saídas",
            Left = 170,
            Top = 28,
            Width = 140,
            Height = 30
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
        _temaButton.Click += TemaButton_Click;

        _usarTemaWindowsMenuItem.Click += (_, _) => SelecionarTema(ThemeSelection.Windows);
        _temaClaroMenuItem.Click += (_, _) => SelecionarTema(ThemeSelection.Light);
        _temaEscuroMenuItem.Click += (_, _) => SelecionarTema(ThemeSelection.Dark);

        _atualizarPortasButton.Click += AtualizarPortasButton_Click;
        _detectarClpButton.Click += DetectarClpButton_Click;

        _conectarClpButton.Click += ConectarClpButton_Click;
        _simularErroButton.Click += SimularErroButton_Click;
        _desconectarButton.Click += DesconectarButton_Click;
        _lerMw70Button.Click += LerMw70Button_Click;
        _habilitarTesteButton.Click += HabilitarTesteButton_Click;
        _resetarSaidasButton.Click += ResetarSaidasButton_Click;

        _estadoGroupBox.Controls.Add(_estadoStatusLabel);
        _estadoGroupBox.Controls.Add(_estadoMensagemLabel);
        _estadoGroupBox.Controls.Add(_estadoAtualizacaoLabel);
        _estadoGroupBox.Controls.Add(_conectarClpButton);
        _estadoGroupBox.Controls.Add(_simularErroButton);
        _estadoGroupBox.Controls.Add(_desconectarButton);
        _estadoGroupBox.Controls.Add(_lerMw70Button);

        _comandosGroupBox.Controls.Add(_habilitarTesteButton);
        _comandosGroupBox.Controls.Add(_resetarSaidasButton);

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
        Controls.Add(_temaButton);
        Controls.Add(_estadoGroupBox);
        Controls.Add(_conexaoGroupBox);
        Controls.Add(_comandosGroupBox);

        AtualizarListaDePortas();
        AtualizarResumoConexao();
        AtualizarEstadoConexao();
        AtualizarMenuTema();

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
        string portaSelecionada = _portaComboBox.SelectedItem?.ToString()
            ?? _connectionSettings.PortName;

        string[] portas = SerialPort.GetPortNames()
            .OrderBy(porta => porta)
            .ToArray();

        _portaComboBox.Items.Clear();

        foreach (string porta in portas)
        {
            _portaComboBox.Items.Add(porta);
        }

        if (_portaComboBox.Items.Count == 0)
        {
            _portaComboBox.Items.Add(_connectionSettings.PortName);
        }

        int indice = _portaComboBox.Items.IndexOf(portaSelecionada);

        if (indice < 0)
        {
            indice = 0;
        }

        _portaComboBox.SelectedIndex = indice;
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

        _estadoStatusLabel.Text = $"Status: {state.Status}";
        _estadoMensagemLabel.Text = $"Mensagem: {state.Message}";
        _estadoAtualizacaoLabel.Text = $"Atualizado: {state.LastUpdatedAt:HH:mm:ss}";
    }

    private void TemaButton_Click(object? sender, EventArgs e)
    {
        _temaMenu.Show(_temaButton, new Point(0, _temaButton.Height));
    }

    private void SelecionarTema(ThemeSelection themeSelection)
    {
        _themeSelection = themeSelection;
        AtualizarMenuTema();
        AplicarTemaSelecionado();
    }

    private void AtualizarMenuTema()
    {
        _usarTemaWindowsMenuItem.Checked = _themeSelection == ThemeSelection.Windows;
        _temaClaroMenuItem.Checked = _themeSelection == ThemeSelection.Light;
        _temaEscuroMenuItem.Checked = _themeSelection == ThemeSelection.Dark;
    }

    private void AtualizarPortasButton_Click(object? sender, EventArgs e)
    {
        AtualizarListaDePortas();
    }

    private bool TryUpdateConnectionSettingsFromUi()
    {
        if (_portaComboBox.SelectedItem is null)
        {
            MessageBox.Show(
                "Selecione uma porta COM.",
                "Configuração inválida",
                MessageBoxButtons.OK,
                MessageBoxIcon.Warning);

            return false;
        }

        if (!int.TryParse(_baudRateComboBox.Text, out int baudRate) || baudRate <= 0)
        {
            MessageBox.Show(
                "Informe um baud rate válido. Exemplo: 9600.",
                "Configuração inválida",
                MessageBoxButtons.OK,
                MessageBoxIcon.Warning);

            return false;
        }

        if (!byte.TryParse(_slaveIdTextBox.Text, out byte slaveId) || slaveId is < 1 or > 247)
        {
            MessageBox.Show(
                "Informe um Slave ID válido entre 1 e 247.",
                "Configuração inválida",
                MessageBoxButtons.OK,
                MessageBoxIcon.Warning);

            return false;
        }

        _connectionSettings.PortName = _portaComboBox.SelectedItem.ToString()!;
        _connectionSettings.BaudRate = baudRate;
        _connectionSettings.SlaveId = slaveId;

        AtualizarResumoConexao();

        return true;
    }
    private int[] ObterBaudRatesSelecionadosParaBusca()
    {
        List<int> baudRates = [];

        if (int.TryParse(_baudRateComboBox.Text, out int baudRateAtual))
        {
            baudRates.Add(baudRateAtual);
        }

        foreach (object? item in _baudRateBuscaCheckedListBox.CheckedItems)
        {
            if (item is not null && int.TryParse(item.ToString(), out int baudRateMarcado))
            {
                baudRates.Add(baudRateMarcado);
            }
        }

        if (baudRates.Count == 0)
        {
            baudRates.Add(_connectionSettings.BaudRate);
        }

        return baudRates
            .Distinct()
            .ToArray();
    }
    private async void DetectarClpButton_Click(object? sender, EventArgs e)
    {
        _detectarClpButton.Enabled = false;
        _conectarClpButton.Enabled = false;

        try
        {
            string[] portasDisponiveis = SerialPort.GetPortNames()
                .OrderBy(porta => porta)
                .ToArray();

            List<string> portasOrdenadas = [];
            string? portaSelecionada = _portaComboBox.SelectedItem?.ToString();

            if (!string.IsNullOrWhiteSpace(portaSelecionada) && portasDisponiveis.Contains(portaSelecionada))
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

            string[] portas = portasOrdenadas.ToArray();

            if (portas.Length == 0)
            {
                _statusLabel.Text = "Nenhuma porta COM encontrada.";
                return;
            }

            if (!int.TryParse(_baudRateComboBox.Text, out int baudRateSelecionado))
            {
                _statusLabel.Text = "Selecione um baud rate válido antes da busca.";
                return;
            }

            int[] baudRates = _buscarTodosBaudRatesCheckBox.Checked
                ?
                [
                    9600,
                    19200,
                    38400,
                    57600,
                    115200
                ]
                :
                [
                    baudRateSelecionado
                ];

            foreach (string porta in portas)
            {
                foreach (int baudRate in baudRates)
                {
                    for (byte slaveId = 1; slaveId <= 30; slaveId++)
                    {
                        _statusLabel.Text = $"Detectando: {porta}, {baudRate} bps, Slave {slaveId}...";
                        Application.DoEvents();

                        PlcConnectionSettings tentativa = new()
                        {
                            PortName = porta,
                            BaudRate = baudRate,
                            Parity = _connectionSettings.Parity,
                            DataBits = _connectionSettings.DataBits,
                            StopBits = _connectionSettings.StopBits,
                            SlaveId = slaveId,
                            TimeoutMilliseconds = 200
                        };

                        try
                        {
                            await _plcService.ConnectAsync(tentativa);

                            ushort value = await _plcService.ReadHoldingRegisterAsync(
                                Hio115MemoryMap.HabilitaTeste);

                            _connectionSettings.PortName = porta;
                            _connectionSettings.BaudRate = baudRate;
                            _connectionSettings.SlaveId = slaveId;

                            _portaComboBox.SelectedItem = porta;
                            _baudRateComboBox.SelectedItem = baudRate.ToString();
                            _slaveIdTextBox.Text = slaveId.ToString();

                            AtualizarResumoConexao();

                            _plcService.State.SetConnected(
                                $"CLP detectado: {porta}, {baudRate} bps, Slave {slaveId}, %MW{Hio115MemoryMap.HabilitaTeste} = {value}");

                            AtualizarEstadoConexao();

                            _statusLabel.Text =
                                $"CLP detectado em {porta}, {baudRate} bps, Slave {slaveId}.";

                            return;
                        }
                        catch
                        {
                            await _plcService.DisconnectAsync();
                        }
                    }
                }
            }

            _plcService.State.SetError("Nenhum CLP encontrado na varredura de Slave ID 1 a 30.");
            AtualizarEstadoConexao();
            _statusLabel.Text = "Nenhum CLP encontrado entre Slave ID 1 e 30.";
        }
        finally
        {
            _detectarClpButton.Enabled = true;
            _conectarClpButton.Enabled = true;
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
        bool temaEscuro = _themeSelection switch
        {
            ThemeSelection.Windows => AppThemeService.WindowsIsInDarkTheme(),
            ThemeSelection.Dark => true,
            _ => false
        };

        AplicarTema(temaEscuro);
    }

    private void AplicarTema(bool temaEscuro)
    {
        Color corFundo;
        Color corTexto;
        Color corBotao;
        Color corTextoBotao;
        Color corCampo;

        if (temaEscuro)
        {
            corFundo = Color.FromArgb(32, 32, 32);
            corTexto = Color.WhiteSmoke;
            corBotao = Color.FromArgb(55, 55, 55);
            corTextoBotao = Color.White;
            corCampo = Color.FromArgb(45, 45, 45);
        }
        else
        {
            corFundo = SystemColors.Control;
            corTexto = Color.Black;
            corBotao = SystemColors.Control;
            corTextoBotao = Color.Black;
            corCampo = Color.White;
        }

        BackColor = corFundo;
        AppThemeService.ApplyTitleBar(this, temaEscuro);

        _tituloLabel.ForeColor = corTexto;
        _statusLabel.ForeColor = corTexto;

        _estadoGroupBox.ForeColor = corTexto;
        _estadoGroupBox.BackColor = corFundo;
        _estadoStatusLabel.ForeColor = corTexto;
        _estadoMensagemLabel.ForeColor = corTexto;
        _estadoAtualizacaoLabel.ForeColor = corTexto;

        _conexaoGroupBox.ForeColor = corTexto;
        _conexaoGroupBox.BackColor = corFundo;

        _portaTituloLabel.ForeColor = corTexto;
        _baudRateTituloLabel.ForeColor = corTexto;
        _slaveIdTituloLabel.ForeColor = corTexto;
        _conexaoResumoLabel.ForeColor = corTexto;

        _portaComboBox.BackColor = corCampo;
        _portaComboBox.ForeColor = corTexto;

        _baudRateComboBox.BackColor = corCampo;
        _baudRateComboBox.ForeColor = corTexto;

        _baudRateBuscaTituloLabel.ForeColor = corTexto;
        _baudRateBuscaCheckedListBox.BackColor = corCampo;
        _baudRateBuscaCheckedListBox.ForeColor = corTexto;

        _buscarTodosBaudRatesCheckBox.ForeColor = corTexto;
        _buscarTodosBaudRatesCheckBox.BackColor = corFundo;

        _slaveIdTextBox.BackColor = corCampo;
        _slaveIdTextBox.ForeColor = corTexto;

        _temaMenu.BackColor = corCampo;
        _temaMenu.ForeColor = corTexto;

        foreach (ToolStripItem item in _temaMenu.Items)
        {
            item.BackColor = corCampo;
            item.ForeColor = corTexto;
        }

        AppThemeService.ApplyButton(_pararTudoButton, corBotao, corTextoBotao);
        AppThemeService.ApplyButton(_temaButton, corBotao, corTextoBotao);
        AppThemeService.ApplyButton(_atualizarPortasButton, corBotao, corTextoBotao);
        AppThemeService.ApplyButton(_detectarClpButton, corBotao, corTextoBotao);
        AppThemeService.ApplyButton(_conectarClpButton, corBotao, corTextoBotao);
        AppThemeService.ApplyButton(_simularErroButton, corBotao, corTextoBotao);
        AppThemeService.ApplyButton(_desconectarButton, corBotao, corTextoBotao);
        AppThemeService.ApplyButton(_lerMw70Button, corBotao, corTextoBotao);
        _comandosGroupBox.ForeColor = corTexto;
        _comandosGroupBox.BackColor = corFundo;
        AppThemeService.ApplyButton(_habilitarTesteButton, corBotao, corTextoBotao);
        AppThemeService.ApplyButton(_resetarSaidasButton, corBotao, corTextoBotao);
    }


}







