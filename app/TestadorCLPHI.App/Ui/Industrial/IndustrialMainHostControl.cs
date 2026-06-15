using TestadorCLPHI.App.Ui;

namespace TestadorCLPHI.App.Ui.Industrial;

public sealed class IndustrialMainHostControl : UserControl
{
    private const int MinimumContentWidth = 1280;
    private const int MinimumContentHeight = 720;

    private readonly IndustrialMainContentControl _content;

    public event EventHandler? EnableTestClicked;
    public event EventHandler? ResetOutputsClicked;

    public IDigitalIoManualPanel ManualIoPanel => _content.ManualIoPanel;

    public IndustrialMainHostControl()
    {
        Dock = DockStyle.Fill;
        BackColor = Color.FromArgb(10, 16, 22);
        Padding = new Padding(0);
        AutoScroll = true;
        AutoScrollMinSize = new Size(MinimumContentWidth, MinimumContentHeight);

        _content = new IndustrialMainContentControl
        {
            Dock = DockStyle.None,
            Location = new Point(0, 0),
            Size = new Size(MinimumContentWidth, MinimumContentHeight),
            Margin = new Padding(0)
        };

        _content.EnableTestClicked += (_, e) => EnableTestClicked?.Invoke(this, e);
        _content.ResetOutputsClicked += (_, e) => ResetOutputsClicked?.Invoke(this, e);

        Controls.Add(_content);
        ResizeContentToViewport();
    }

    protected override void OnResize(EventArgs e)
    {
        base.OnResize(e);
        ResizeContentToViewport();
    }

    private void ResizeContentToViewport()
    {
        int width = Math.Max(ClientSize.Width, MinimumContentWidth);
        int height = Math.Max(ClientSize.Height, MinimumContentHeight);
        Size desiredSize = new(width, height);

        if (_content.Size != desiredSize)
        {
            _content.Size = desiredSize;
        }

        AutoScrollMinSize = desiredSize;
    }
}
