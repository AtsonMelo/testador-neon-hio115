using TestadorCLPHI.App.Hardware;

namespace TestadorCLPHI.App.Ui.Industrial.Layout3;

public sealed class Layout3PreviewForm : Form
{
    private readonly Layout3ThemePalette _palette;

    public Layout3PreviewForm(
        HardwareCatalog? hardwareCatalog,
        Layout3PreviewTheme theme = Layout3PreviewTheme.Dark)
    {
        _palette = Layout3ThemePalette.For(theme);
        Text = theme switch
        {
            Layout3PreviewTheme.Light => "Layout 3 - Preview isolado (tema claro) | Testador CLP HI",
            Layout3PreviewTheme.Automatic => "Layout 3 - Preview isolado (tema automatico) | Testador CLP HI",
            _ => "Layout 3 - Preview isolado | Testador CLP HI"
        };
        StartPosition = FormStartPosition.CenterScreen;
        Size = new Size(1280, 720);
        MinimumSize = new Size(1040, 620);
        BackColor = _palette.Background;
        ForeColor = _palette.Text;
        Font = new Font("Segoe UI", 9F);
        AutoScaleMode = AutoScaleMode.Dpi;

        Controls.Add(new Layout3PreviewControl(hardwareCatalog, theme));
    }

    protected override void OnShown(EventArgs e)
    {
        base.OnShown(e);
        AppThemeService.ApplyTitleBar(this, darkMode: _palette.UseDarkTitleBar);
    }
}
