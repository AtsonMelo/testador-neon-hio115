using TestadorCLPHI.App.Plc;

namespace TestadorCLPHI.App.Ui.Controls;

public sealed class ConnectionStatePanelControl : UserControl
{
    private readonly GroupBox _estadoGroupBox;
    private readonly Label _estadoStatusLabel;
    private readonly Label _estadoMensagemLabel;
    private readonly Label _estadoAtualizacaoLabel;
    private readonly Button _conectarClpButton;
    private readonly Button _simularErroButton;
    private readonly Button _desconectarButton;
    private readonly Button _lerMw70Button;

    public event EventHandler? ConnectClicked;
    public event EventHandler? SimulateErrorClicked;
    public event EventHandler? DisconnectClicked;
    public event EventHandler? ReadMw70Clicked;

    public ConnectionStatePanelControl()
    {
        Width = 270;
        Height = 250;

        _estadoGroupBox = new GroupBox
        {
            Text = "Estado da conexão",
            Left = 0,
            Top = 0,
            Width = 270,
            Height = 250
        };

        _estadoStatusLabel = CreateLabel(string.Empty, 15, 30);
        _estadoMensagemLabel = CreateMessageLabel(15, 55);
        _estadoAtualizacaoLabel = CreateLabel(string.Empty, 15, 120);

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

        _conectarClpButton.Click += (_, e) => ConnectClicked?.Invoke(this, e);
        _simularErroButton.Click += (_, e) => SimulateErrorClicked?.Invoke(this, e);
        _desconectarButton.Click += (_, e) => DisconnectClicked?.Invoke(this, e);
        _lerMw70Button.Click += (_, e) => ReadMw70Clicked?.Invoke(this, e);

        _estadoGroupBox.Controls.Add(_estadoStatusLabel);
        _estadoGroupBox.Controls.Add(_estadoMensagemLabel);
        _estadoGroupBox.Controls.Add(_estadoAtualizacaoLabel);
        _estadoGroupBox.Controls.Add(_conectarClpButton);
        _estadoGroupBox.Controls.Add(_simularErroButton);
        _estadoGroupBox.Controls.Add(_desconectarButton);
        _estadoGroupBox.Controls.Add(_lerMw70Button);

        Controls.Add(_estadoGroupBox);
    }

    public void UpdateState(PlcConnectionState state)
    {
        _estadoStatusLabel.Text = $"Status: {state.Status}";
        _estadoMensagemLabel.Text = $"Mensagem: {state.Message}";
        _estadoAtualizacaoLabel.Text = $"Atualizado: {state.LastUpdatedAt:HH:mm:ss}";
    }

    public void SetConnectionButtonsEnabled(bool enabled)
    {
        _conectarClpButton.Enabled = enabled;
        _desconectarButton.Enabled = enabled;
        _lerMw70Button.Enabled = enabled;
    }

    private static Label CreateLabel(string text, int left, int top)
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

    private static Label CreateMessageLabel(int left, int top)
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
}
