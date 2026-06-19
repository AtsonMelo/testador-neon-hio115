using TestadorCLPHI.App.Hardware;

namespace TestadorCLPHI.App.Ui.Industrial.Layout3;

public sealed class Layout3PreviewForm : Form
{
    private static readonly Color BackgroundColor = Color.FromArgb(18, 24, 32);

    public Layout3PreviewForm(HardwareCatalog? hardwareCatalog)
    {
        Text = "Layout 3 - Preview isolado | Testador CLP HI";
        StartPosition = FormStartPosition.CenterScreen;
        Size = new Size(1280, 720);
        MinimumSize = new Size(1040, 620);
        BackColor = BackgroundColor;
        ForeColor = Color.FromArgb(226, 232, 240);
        Font = new Font("Segoe UI", 9F);
        AutoScaleMode = AutoScaleMode.Dpi;

        Controls.Add(new Layout3PreviewControl(hardwareCatalog));
    }

    protected override void OnShown(EventArgs e)
    {
        base.OnShown(e);
        AppThemeService.ApplyTitleBar(this, darkMode: true);
    }
}
