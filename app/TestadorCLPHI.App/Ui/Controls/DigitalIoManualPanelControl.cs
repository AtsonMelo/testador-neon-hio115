using System.Drawing;
using System.Windows.Forms;

namespace TestadorCLPHI.App.Ui.Controls;

public sealed class DigitalIoManualPanelControl : UserControl
{
    private readonly GroupBox _groupBox;
    private readonly Button[] _outputButtons = new Button[4];
    private readonly Label[] _inputLedLabels = new Label[8];
    private readonly Button _refreshInputsButton;

    public event Action<int>? OutputCommandClicked;
    public event EventHandler? RefreshInputsClicked;

    public DigitalIoManualPanelControl()
    {
        Width = 690;
        Height = 125;

        _groupBox = new GroupBox
        {
            Text = "Painel manual 4DO/8DI",
            Left = 0,
            Top = 0,
            Width = 690,
            Height = 125
        };

        Label outputsTitleLabel = new()
        {
            Text = "1) Clique em Habilitar teste  2) Acione a saída",
            AutoSize = true,
            Left = 15,
            Top = 24,
            Font = new Font("Segoe UI", 9, FontStyle.Bold)
        };

        _groupBox.Controls.Add(outputsTitleLabel);

        string[] expectedInputs =
        [
            "DI00+DI04",
            "DI01+DI05",
            "DI02+DI06",
            "DI03+DI07"
        ];

        for (int channel = 0; channel < _outputButtons.Length; channel++)
        {
            int capturedChannel = channel;

            Button button = new()
            {
                Text = $"D{channel:000} -> {expectedInputs[channel]}",
                Left = 15 + (channel * 165),
                Top = 45,
                Width = 155,
                Height = 28
            };

            button.Click += (_, _) => OutputCommandClicked?.Invoke(capturedChannel);

            _outputButtons[channel] = button;
            _groupBox.Controls.Add(button);
        }

        Label inputsTitleLabel = new()
        {
            Text = "Entradas digitais",
            AutoSize = true,
            Left = 15,
            Top = 83,
            Font = new Font("Segoe UI", 9, FontStyle.Bold)
        };

        _groupBox.Controls.Add(inputsTitleLabel);

        for (int channel = 0; channel < _inputLedLabels.Length; channel++)
        {
            Label label = new()
            {
                Text = $"DI{channel:00} ●",
                AutoSize = true,
                Left = 135 + (channel * 48),
                Top = 84,
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                ForeColor = Color.Gray
            };

            _inputLedLabels[channel] = label;
            _groupBox.Controls.Add(label);
        }

        _refreshInputsButton = new Button
        {
            Text = "Atualizar",
            Left = 585,
            Top = 80,
            Width = 85,
            Height = 28
        };

        _refreshInputsButton.Click += (_, e) => RefreshInputsClicked?.Invoke(this, e);

        _groupBox.Controls.Add(_refreshInputsButton);
        Controls.Add(_groupBox);
    }

    public void SetInputState(int channel, bool active)
    {
        if (channel < 0 || channel >= _inputLedLabels.Length)
        {
            return;
        }

        _inputLedLabels[channel].Text = $"DI{channel:00} ●";

        _inputLedLabels[channel].ForeColor = active
            ? Color.LimeGreen
            : Color.Gray;
    }

    public void SetPanelEnabled(bool enabled)
    {
        foreach (Button button in _outputButtons)
        {
            button.Enabled = enabled;
        }

        _refreshInputsButton.Enabled = enabled;
    }
}
