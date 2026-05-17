namespace TestadorCLPHI.App.Ui.Controls;

public sealed class TesterCommandPanelControl : UserControl
{
    private readonly GroupBox _groupBox;
    private readonly Button _enableTestButton;
    private readonly Button _resetOutputsButton;

    public event EventHandler? EnableTestClicked;
    public event EventHandler? ResetOutputsClicked;

    public TesterCommandPanelControl()
    {
        Width = 690;
        Height = 75;

        _groupBox = new GroupBox
        {
            Text = "Comandos do testador",
            Left = 0,
            Top = 0,
            Width = 690,
            Height = 75
        };

        _enableTestButton = new Button
        {
            Text = "Habilitar teste",
            Left = 15,
            Top = 28,
            Width = 140,
            Height = 30
        };

        _resetOutputsButton = new Button
        {
            Text = "Resetar saídas",
            Left = 170,
            Top = 28,
            Width = 140,
            Height = 30
        };

        _enableTestButton.Click += (_, e) => EnableTestClicked?.Invoke(this, e);
        _resetOutputsButton.Click += (_, e) => ResetOutputsClicked?.Invoke(this, e);

        _groupBox.Controls.Add(_enableTestButton);
        _groupBox.Controls.Add(_resetOutputsButton);

        Controls.Add(_groupBox);
    }
}
