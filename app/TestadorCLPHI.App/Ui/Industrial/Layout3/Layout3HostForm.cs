using TestadorCLPHI.App.Hardware;

namespace TestadorCLPHI.App.Ui.Industrial.Layout3;

public sealed class Layout3HostForm : Form
{
    private readonly Layout3ThemePalette _palette;

    public Layout3HostForm(HardwareCatalog? hardwareCatalog)
    {
        HardwareCatalog catalog = hardwareCatalog ?? HardwareCatalog.Empty;
        _palette = Layout3ThemePalette.For(Layout3PreviewTheme.Automatic);

        Text = "Layout 3 - Host read-only | Testador CLP HI";
        StartPosition = FormStartPosition.CenterScreen;
        Size = new Size(1280, 720);
        MinimumSize = new Size(1040, 620);
        BackColor = _palette.Background;
        ForeColor = _palette.Text;
        Font = new Font("Segoe UI", 9F);
        AutoScaleMode = AutoScaleMode.Dpi;

        Layout3HostState state = Layout3HostState.CreateInitial(catalog);
        Layout3HostControl hostControl = new(
            catalog,
            state,
            new Layout3ReadOnlyCommandGuard(),
            Layout3PreviewTheme.Automatic);
        hostControl.ThemeChanged += theme =>
        {
            Layout3ThemePalette palette = Layout3ThemePalette.For(theme);
            BackColor = palette.Background;
            ForeColor = palette.Text;
            AppThemeService.ApplyTitleBar(this, darkMode: palette.UseDarkTitleBar);
        };
        Controls.Add(hostControl);
    }

    protected override void OnShown(EventArgs e)
    {
        base.OnShown(e);
        AppThemeService.ApplyTitleBar(this, darkMode: _palette.UseDarkTitleBar);
    }
}
