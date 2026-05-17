using System.Drawing;
using System.Windows.Forms;

namespace TestadorCLPHI.App.Ui.Controls;

public sealed class AppHeaderControl : UserControl
{
    private readonly Label _titleLabel;
    private readonly Label _subtitleLabel;
    private readonly Label _connectionStatusLabel;
    private readonly Label _testStatusLabel;
    private readonly Button _stopButton;

    public event EventHandler? StopClicked;

    public AppHeaderControl()
    {
        Width = 830;
        Height = 100;

        _titleLabel = new Label
        {
            Text = "Testador CLP HI",
            Left = 10,
            Top = 8,
            Width = 260,
            Height = 34,
            Font = new Font("Segoe UI", 18, FontStyle.Bold)
        };

        _subtitleLabel = new Label
        {
            Text = "Comunicação Modbus RTU real em fase inicial.",
            Left = 12,
            Top = 48,
            Width = 360,
            Height = 22,
            Font = new Font("Segoe UI", 10, FontStyle.Regular)
        };

        Label connectionCaptionLabel = new()
        {
            Text = "CLP:",
            Left = 400,
            Top = 16,
            Width = 35,
            Height = 20,
            Font = new Font("Segoe UI", 10, FontStyle.Bold)
        };

        _connectionStatusLabel = new Label
        {
            Text = "Desconectado",
            Left = 438,
            Top = 16,
            Width = 130,
            Height = 20,
            Font = new Font("Segoe UI", 10, FontStyle.Bold),
            ForeColor = Color.Silver
        };

        Label testCaptionLabel = new()
        {
            Text = "Teste:",
            Left = 400,
            Top = 46,
            Width = 50,
            Height = 20,
            Font = new Font("Segoe UI", 10, FontStyle.Bold)
        };

        _testStatusLabel = new Label
        {
            Text = "Desabilitado",
            Left = 452,
            Top = 46,
            Width = 130,
            Height = 20,
            Font = new Font("Segoe UI", 10, FontStyle.Bold),
            ForeColor = Color.Goldenrod
        };

        _stopButton = new Button
        {
            Text = "STOP",
            Left = 660,
            Top = 20,
            Width = 145,
            Height = 52,
            BackColor = Color.DarkRed,
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat,
            Font = new Font("Segoe UI", 13, FontStyle.Bold)
        };

        _stopButton.FlatAppearance.BorderColor = Color.OrangeRed;
        _stopButton.FlatAppearance.BorderSize = 2;
        _stopButton.Click += (_, e) => StopClicked?.Invoke(this, e);

        Controls.Add(_titleLabel);
        Controls.Add(_subtitleLabel);
        Controls.Add(connectionCaptionLabel);
        Controls.Add(_connectionStatusLabel);
        Controls.Add(testCaptionLabel);
        Controls.Add(_testStatusLabel);
        Controls.Add(_stopButton);
    }

    public void SetConnectionStatus(string statusText, bool connected)
    {
        _connectionStatusLabel.Text = statusText;
        _connectionStatusLabel.ForeColor = connected
            ? Color.LimeGreen
            : Color.Silver;
    }

    public void SetTestStatus(bool enabled)
    {
        _testStatusLabel.Text = enabled
            ? "Habilitado"
            : "Desabilitado";

        _testStatusLabel.ForeColor = enabled
            ? Color.LimeGreen
            : Color.Goldenrod;
    }
}
