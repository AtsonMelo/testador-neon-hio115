namespace TestadorCLPHI.App;

public sealed class MainForm : Form
{
    private readonly Label _tituloLabel;
    private readonly Label _statusLabel;
    private readonly Button _pararTudoButton;

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

        _pararTudoButton.Click += PararTudoButton_Click;

        Controls.Add(_tituloLabel);
        Controls.Add(_statusLabel);
        Controls.Add(_pararTudoButton);
    }

    private void PararTudoButton_Click(object? sender, EventArgs e)
    {
        MessageBox.Show(
            "Futuramente este botão vai acionar RESET_SAIDAS no CLP.",
            "Parar tudo",
            MessageBoxButtons.OK,
            MessageBoxIcon.Information);
    }
}