using Microsoft.Win32;

namespace TestadorCLPHI.App;

public sealed class MainForm : Form
{
    private readonly Label _tituloLabel;
    private readonly Label _statusLabel;
    private readonly Button _pararTudoButton;

    private readonly GroupBox _temaGroupBox;
    private readonly RadioButton _usarTemaWindowsRadioButton;
    private readonly RadioButton _temaClaroRadioButton;
    private readonly RadioButton _temaEscuroRadioButton;

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

        _pararTudoButton.Click += PararTudoButton_Click;

        _usarTemaWindowsRadioButton.CheckedChanged += TemaRadioButton_CheckedChanged;
        _temaClaroRadioButton.CheckedChanged += TemaRadioButton_CheckedChanged;
        _temaEscuroRadioButton.CheckedChanged += TemaRadioButton_CheckedChanged;

        _temaGroupBox.Controls.Add(_usarTemaWindowsRadioButton);
        _temaGroupBox.Controls.Add(_temaClaroRadioButton);
        _temaGroupBox.Controls.Add(_temaEscuroRadioButton);

        Controls.Add(_tituloLabel);
        Controls.Add(_statusLabel);
        Controls.Add(_pararTudoButton);
        Controls.Add(_temaGroupBox);

        _usarTemaWindowsRadioButton.Checked = true;
        AplicarTemaSelecionado();
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

        if (temaEscuro)
        {
            corFundo = Color.FromArgb(32, 32, 32);
            corTexto = Color.WhiteSmoke;
            corBotao = Color.FromArgb(55, 55, 55);
            corTextoBotao = Color.White;
        }
        else
        {
            corFundo = SystemColors.Control;
            corTexto = Color.Black;
            corBotao = SystemColors.Control;
            corTextoBotao = Color.Black;
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

        _pararTudoButton.BackColor = corBotao;
        _pararTudoButton.ForeColor = corTextoBotao;
        _pararTudoButton.FlatStyle = FlatStyle.Flat;
    }

    private static bool WindowsEstaEmTemaEscuro()
    {
        using RegistryKey? key = Registry.CurrentUser.OpenSubKey(
            @"Software\Microsoft\Windows\CurrentVersion\Themes\Personalize");

        object? value = key?.GetValue("AppsUseLightTheme");

        return value is int appsUseLightTheme && appsUseLightTheme == 0;
    }
}