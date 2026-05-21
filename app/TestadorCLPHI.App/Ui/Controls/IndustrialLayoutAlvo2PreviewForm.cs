using TestadorCLPHI.App.Ui.Industrial;

namespace TestadorCLPHI.App.Ui.Controls;

public sealed class IndustrialLayoutAlvo2PreviewForm : Form
{
    public IndustrialLayoutAlvo2PreviewForm()
    {
        Text        = "Preview - Layout Alvo 2 | Testador CLP HI";
        MinimumSize = new Size(1280, 720);
        BackColor   = Color.FromArgb(10, 16, 22);

        Controls.Add(new IndustrialMainHostControl());
    }
}
