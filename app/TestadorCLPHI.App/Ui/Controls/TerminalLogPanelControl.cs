namespace TestadorCLPHI.App.Ui.Controls;

public sealed class TerminalLogPanelControl : UserControl
{
    private readonly GroupBox _groupBox;
    private readonly TextBox _logTextBox;

    public TerminalLogPanelControl()
    {
        Width = 690;
        Height = 150;

        _groupBox = new GroupBox
        {
            Text = "Terminal / Log",
            Dock = DockStyle.Fill
        };

        _logTextBox = new TextBox
        {
            Multiline = true,
            ReadOnly = true,
            ScrollBars = ScrollBars.Vertical,
            Dock = DockStyle.Fill,
            BorderStyle = BorderStyle.FixedSingle,
            BackColor = Color.FromArgb(18, 24, 32),
            ForeColor = Color.FromArgb(203, 213, 225),
            Font = new Font("Consolas", 9F),
            Text = "[INFO] Terminal visual preparado." + Environment.NewLine +
                   "[INFO] Logs de comunicação e diagnóstico serão integrados em etapa futura."
        };

        _groupBox.Controls.Add(_logTextBox);
        Controls.Add(_groupBox);
    }

    public void AppendLog(string message)
    {
        if (string.IsNullOrWhiteSpace(message))
        {
            return;
        }

        _logTextBox.AppendText(Environment.NewLine + message.Trim());
    }
}
