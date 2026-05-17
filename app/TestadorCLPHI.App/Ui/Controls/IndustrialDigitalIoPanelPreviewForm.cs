namespace TestadorCLPHI.App.Ui.Controls;

public sealed class IndustrialDigitalIoPanelPreviewForm : Form
{
    public IndustrialDigitalIoPanelPreviewForm()
    {
        Text = "Preview - Painel Industrial 4DO/8DI";
        StartPosition = FormStartPosition.CenterScreen;
        Width = 900;
        Height = 520;
        MinimumSize = new Size(760, 420);
        BackColor = Color.FromArgb(18, 24, 32);

        Label titleLabel = new()
        {
            Text = "Preview isolado do painel industrial 4DO/8DI",
            AutoSize = true,
            ForeColor = Color.FromArgb(226, 232, 240),
            Font = new Font("Segoe UI", 14F, FontStyle.Bold),
            Left = 24,
            Top = 24
        };

        Label subtitleLabel = new()
        {
            Text = "Ambiente de teste visual sem alterar MainForm nem lógica Modbus.",
            AutoSize = true,
            ForeColor = Color.FromArgb(148, 163, 184),
            Font = new Font("Segoe UI", 9F, FontStyle.Regular),
            Left = 26,
            Top = 58
        };

        Controls.Add(titleLabel);
        Controls.Add(subtitleLabel);
    }
}
