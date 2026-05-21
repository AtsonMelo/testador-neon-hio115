namespace TestadorCLPHI.App.Ui.Industrial;

public sealed class IndustrialMainHostControl : UserControl
{
    private readonly IndustrialMainContentControl _content;

    public IndustrialMainHostControl()
    {
        Dock = DockStyle.Fill;
        BackColor = Color.FromArgb(10, 16, 22);
        Padding = new Padding(0);

        _content = new IndustrialMainContentControl
        {
            Dock = DockStyle.Fill,
            Margin = new Padding(0)
        };

        Controls.Add(_content);
    }
}
