using System.Drawing;
using System.Windows.Forms;

namespace TestadorCLPHI.App.Ui.Controls;

public sealed class TerminalLogPanelControl : UserControl
{
    private readonly GroupBox _groupBox;
    private readonly TextBox _logTextBox;

    public TerminalLogPanelControl()
    {
        Width = 690;
        Height = 115;

        _groupBox = new GroupBox
        {
            Text = "Terminal / Log",
            Left = 0,
            Top = 0,
            Width = 690,
            Height = 115
        };

        _logTextBox = new TextBox
        {
            Left = 12,
            Top = 22,
            Width = 665,
            Height = 80,
            Multiline = true,
            ReadOnly = true,
            ScrollBars = ScrollBars.Vertical,
            Font = new Font("Consolas", 9),
            Text =
                "Terminal/log reservado para futuras mensagens de comunicação." +
                Environment.NewLine +
                "Próximo passo: registrar eventos do app, comandos Modbus e respostas do CLP."
        };

        _groupBox.Controls.Add(_logTextBox);
        Controls.Add(_groupBox);
    }

    public void SetLines(IEnumerable<string> lines)
    {
        _logTextBox.Lines = lines.ToArray();
    }

    public void AppendLine(string message)
    {
        if (string.IsNullOrWhiteSpace(message))
        {
            return;
        }

        _logTextBox.AppendText(message + Environment.NewLine);
    }
}
