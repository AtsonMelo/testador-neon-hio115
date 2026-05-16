using Microsoft.Win32;
using System.IO.Ports;
using TestadorCLPHI.App.Plc;

namespace TestadorCLPHI.App;

public sealed class MainForm : Form
{
    private readonly PlcConnectionSettings _connectionSettings = new();

    private readonly Label _tituloLabel;
    private readonly Label _statusLabel;
    private readonly Button _pararTudoButton;

    private readonly GroupBox _temaGroupBox;
    private readonly RadioButton _usarTemaWindowsRadioButton;
    private readonly RadioButton _temaClaroRadioButton;
    private readonly RadioButton _temaEscuroRadioButton;

    private readonly GroupBox _conexaoGroupBox;
    private readonly Label _portaTituloLabel;
    private readonly ComboBox _portaComboBox;
    private readonly Button _atualizarPortasButton;

    private readonly Label _baudRateTituloLabel;
    private readonly TextBox _baudRateTextBox;

    private readonly Label _slaveIdTituloLabel;
    private readonly TextBox _slaveIdTextBox;

    private readonly Label _conexaoResumoLabel;
    private readonly Button _aplicarConexaoButton;

    public MainForm()
    {
        Text = "Testador CLP HI";
        Width = 900;
        Height = 600;
        StartPosition = FormStartPosition.CenterScreen;

        _tituloLabel = new Label
        {
            Text = "Testador CLP HI",
            AutoSize = true,
            Left = 20,
            Top = 20,
            Font = new Font("Segoe UI", 18, FontStyle.Bold)
        };

        _statusLabel = new Label
        {
            Text = "App em desenvolvimento. Comunicação com CLP ainda não implementada.",
            AutoSize = true,
            Left = 20,
            Top = 70,
            Font = new Font("Segoe UI", 10)
        };

        _pararTudoButton = new Button
        {
            Text = "Parar tudo",
            Left = 20,
            Top = 120,
            Width = 160,
            Height = 40
        };

        _temaGroupBox = new GroupBox
        {
            Text = "Tema",
            Left = 20,
            Top = 190,
            Width = 260,
            Height = 120
        };

        _usarTemaWindowsRadioButton = new RadioButton
        {
            Text = "Usar tema do Windows",
            AutoSize = true,
            Left = 15,
            Top = 25
        };

        _temaClaroRadioButton = new RadioButton
        {
            Text = "Claro",
            AutoSize = true,
            Left = 15,
            Top = 55
        };

        _temaEscuroRadioButton = new RadioButton
        {
            Text = "Escuro",
            AutoSize = true,
            Left = 15,
            Top = 85
        };

        _conexaoGroupBox = new GroupBox
        {
            Text = "Conexão com CLP",
            Left = 320,
            Top = 190,
            Width = 380,
            Height = 250
        };

        _portaTituloLabel = CriarLabelConexao("Porta COM:", 15, 30);

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
            Width = 120,
            Height = 28
        };

        _baudRateTituloLabel = CriarLabelConexao("Baud rate:", 15, 70);

        _baudRateTextBox = new TextBox
        {
            Left = 110,
            Top = 67,
            Width = 120,
            Text = _connectionSettings.BaudRate.ToString()
        };

        _slaveIdTituloLabel = CriarLabelConexao("Slave ID:", 15, 110);

        _slaveIdTextBox = new TextBox
        {
            Left = 110,
            Top = 107,
            Width = 120,
            Text = _connectionSettings.SlaveId.ToString()
        };

        _aplicarConexaoButton = new Button
        {
            Text = "Aplicar configuração",
            Left = 15,
            Top = 145,
            Width = 160,
            Height = 32
        };

        _conexaoResumoLabel = new Label
        {
            AutoSize = false,
            Left = 15,
            Top = 190,
            Width = 340,
            Height = 45,
            Font = new Font("Segoe UI", 9)
        };

        _pararTudoButton.Click += PararTudoButton_Click;

        _usarTemaWindowsRadioButton.CheckedChanged += TemaRadioButton_CheckedChanged;
        _temaClaroRadioButton.CheckedChanged += TemaRadioButton_CheckedChanged;
        _temaEscuroRadioButton.CheckedChanged += TemaRadioButton_CheckedChanged;

        _atualizarPortasButton.Click += AtualizarPortasButton_Click;
        _aplicarConexaoButton.Click += AplicarConexaoButton_Click;

        _temaGroupBox.Controls.Add(_usarTemaWindowsRadioButton);
        _temaGroupBox.Controls.Add(_temaClaroRadioButton);
        _temaGroupBox.Controls.Add(_temaEscuroRadioButton);

        _conexaoGroupBox.Controls.Add(_portaTituloLabel);
        _conexaoGroupBox.Controls.Add(_portaComboBox);
        _conexaoGroupBox.Controls.Add(_atualizarPortasButton);
        _conexaoGroupBox.Controls.Add(_baudRateTituloLabel);
        _conexaoGroupBox.Controls.Add(_baudRateTextBox);
        _conexaoGroupBox.Controls.Add(_slaveIdTituloLabel);
        _conexaoGroupBox.Controls.Add(_slaveIdTextBox);
        _conexaoGroupBox.Controls.Add(_aplicarConexaoButton);
        _conexaoGroupBox.Controls.Add(_conexaoResumoLabel);

        Controls.Add(_tituloLabel);
        Controls.Add(_statusLabel);
        Controls.Add(_pararTudoButton);
        Controls.Add(_temaGroupBox);
        Controls.Add(_conexaoGroupBox);

        AtualizarListaDePortas();
        AtualizarResumoConexao();

        _usarTemaWindowsRadioButton.Checked = true;
        AplicarTemaSelecionado();
    }

    private static Label CriarLabelConexao(string text, int left, int top)
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

    private void AtualizarPortasButton_Click(object? sender, EventArgs e)
    {
        AtualizarListaDePortas();
    }

    private void AplicarConexaoButton_Click(object? sender, EventArgs e)
    {
        if (_portaComboBox.SelectedItem is null)
        {
            MessageBox.Show(
                "Selecione uma porta COM.",
                "Configuração inválida",
                MessageBoxButtons.OK,
                MessageBoxIcon.Warning);
            return;
        }

        if (!int.TryParse(_baudRateTextBox.Text, out int baudRate) || baudRate <= 0)
        {
            MessageBox.Show(
                "Informe um baud rate válido. Exemplo: 9600.",
                "Configuração inválida",
                MessageBoxButtons.OK,
                MessageBoxIcon.Warning);
            return;
        }

        if (!byte.TryParse(_slaveIdTextBox.Text, out byte slaveId) || slaveId == 0)
        {
            MessageBox.Show(
                "Informe um Slave ID válido entre 1 e 247.",
                "Configuração inválida",
                MessageBoxButtons.OK,
                MessageBoxIcon.Warning);
            return;
        }

        if (slaveId > 247)
        {
            MessageBox.Show(
                "No Modbus, o Slave ID normalmente deve ficar entre 1 e 247.",
                "Configuração inválida",
                MessageBoxButtons.OK,
                MessageBoxIcon.Warning);
            return;
        }

        _connectionSettings.PortName = _portaComboBox.SelectedItem.ToString()!;
        _connectionSettings.BaudRate = baudRate;
        _connectionSettings.SlaveId = slaveId;

        AtualizarResumoConexao();

        MessageBox.Show(
            "Configuração aplicada apenas no app. Ainda não houve comunicação com o CLP.",
            "Configuração aplicada",
            MessageBoxButtons.OK,
            MessageBoxIcon.Information);
    }

    private void PararTudoButton_Click(object? sender, EventArgs e)
    {
        MessageBox.Show(
            "Futuramente este botão vai acionar RESET_SAIDAS no CLP.",
            "Parar tudo",
            MessageBoxButtons.OK,
            MessageBoxIcon.Information);
    }

    private void TemaRadioButton_CheckedChanged(object? sender, EventArgs e)
    {
        AplicarTemaSelecionado();
    }

    private void AplicarTemaSelecionado()
    {
        bool temaEscuro;

        if (_usarTemaWindowsRadioButton.Checked)
        {
            temaEscuro = WindowsEstaEmTemaEscuro();
        }
        else if (_temaEscuroRadioButton.Checked)
        {
            temaEscuro = true;
        }
        else
        {
            temaEscuro = false;
        }

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

        _tituloLabel.ForeColor = corTexto;
        _statusLabel.ForeColor = corTexto;

        _temaGroupBox.ForeColor = corTexto;
        _temaGroupBox.BackColor = corFundo;

        _usarTemaWindowsRadioButton.ForeColor = corTexto;
        _usarTemaWindowsRadioButton.BackColor = corFundo;

        _temaClaroRadioButton.ForeColor = corTexto;
        _temaClaroRadioButton.BackColor = corFundo;

        _temaEscuroRadioButton.ForeColor = corTexto;
        _temaEscuroRadioButton.BackColor = corFundo;

        _conexaoGroupBox.ForeColor = corTexto;
        _conexaoGroupBox.BackColor = corFundo;

        _portaTituloLabel.ForeColor = corTexto;
        _baudRateTituloLabel.ForeColor = corTexto;
        _slaveIdTituloLabel.ForeColor = corTexto;
        _conexaoResumoLabel.ForeColor = corTexto;

        _portaComboBox.BackColor = corCampo;
        _portaComboBox.ForeColor = corTexto;

        _baudRateTextBox.BackColor = corCampo;
        _baudRateTextBox.ForeColor = corTexto;

        _slaveIdTextBox.BackColor = corCampo;
        _slaveIdTextBox.ForeColor = corTexto;

        AplicarTemaBotao(_pararTudoButton, corBotao, corTextoBotao);
        AplicarTemaBotao(_atualizarPortasButton, corBotao, corTextoBotao);
        AplicarTemaBotao(_aplicarConexaoButton, corBotao, corTextoBotao);
    }

    private static void AplicarTemaBotao(Button button, Color corFundo, Color corTexto)
    {
        button.BackColor = corFundo;
        button.ForeColor = corTexto;
        button.FlatStyle = FlatStyle.Flat;
    }

    private static bool WindowsEstaEmTemaEscuro()
    {
        using RegistryKey? key = Registry.CurrentUser.OpenSubKey(
            @"Software\Microsoft\Windows\CurrentVersion\Themes\Personalize");

        object? value = key?.GetValue("AppsUseLightTheme");

        return value is int appsUseLightTheme && appsUseLightTheme == 0;
    }
}
