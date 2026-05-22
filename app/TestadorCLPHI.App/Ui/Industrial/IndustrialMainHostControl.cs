namespace TestadorCLPHI.App.Ui.Industrial;

public sealed class IndustrialMainHostControl : UserControl
{
    private readonly IndustrialMainContentControl _content;

    public IndustrialMainHostControl()
    {
        Dock = DockStyle.Fill;
        BackColor = Color.FromArgb(10, 16, 22);
        Padding = new Padding(0);
        AutoScroll = true;
        AutoScrollMinSize = new Size(1280, 720);

        _content = new IndustrialMainContentControl
        {
            Dock = DockStyle.None,
            Location = new Point(0, 0),
            Size = new Size(1280, 720),
            Margin = new Padding(0)
        };

        Controls.Add(_content);
    }

    protected override void OnResize(EventArgs e)
    {
        base.OnResize(e);

        int width = Math.Max(ClientSize.Width, 1280);
        int height = Math.Max(ClientSize.Height, 720);

        _content.Size = new Size(width, height);
        AutoScrollMinSize = new Size(1280, 720);
    }
}
