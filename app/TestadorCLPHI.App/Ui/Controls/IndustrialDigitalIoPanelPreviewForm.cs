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

        IndustrialDigitalIoPanelControl panel = new()
        {
            Dock = DockStyle.Fill
        };

        Controls.Add(panel);
    }
}
