namespace TestadorCLPHI.App.Ui.Controls;

public enum ThemeSelection
{
    Windows,
    Light,
    Dark
}

public sealed class ThemeSelectorControl : UserControl
{
    private readonly Button _themeButton;
    private readonly ContextMenuStrip _themeMenu;
    private readonly ToolStripMenuItem _useWindowsThemeMenuItem;
    private readonly ToolStripMenuItem _lightThemeMenuItem;
    private readonly ToolStripMenuItem _darkThemeMenuItem;

    public event EventHandler? ThemeChanged;

    public ThemeSelection SelectedTheme { get; private set; } = ThemeSelection.Windows;

    public ContextMenuStrip ThemeMenu => _themeMenu;

    public ThemeSelectorControl()
    {
        Width = 90;
        Height = 32;

        _themeButton = new Button
        {
            Text = "Tema",
            Left = 0,
            Top = 0,
            Width = 90,
            Height = 32
        };

        _themeMenu = new ContextMenuStrip();

        _useWindowsThemeMenuItem = new ToolStripMenuItem("Usar tema do Windows");
        _lightThemeMenuItem = new ToolStripMenuItem("Claro");
        _darkThemeMenuItem = new ToolStripMenuItem("Escuro");

        _themeMenu.Items.Add(_useWindowsThemeMenuItem);
        _themeMenu.Items.Add(_lightThemeMenuItem);
        _themeMenu.Items.Add(_darkThemeMenuItem);

        _themeButton.Click += ThemeButton_Click;
        _useWindowsThemeMenuItem.Click += (_, _) => SelectTheme(ThemeSelection.Windows);
        _lightThemeMenuItem.Click += (_, _) => SelectTheme(ThemeSelection.Light);
        _darkThemeMenuItem.Click += (_, _) => SelectTheme(ThemeSelection.Dark);

        Controls.Add(_themeButton);

        UpdateMenuChecks();
    }

    private void ThemeButton_Click(object? sender, EventArgs e)
    {
        _themeMenu.Show(_themeButton, new Point(0, _themeButton.Height));
    }

    private void SelectTheme(ThemeSelection themeSelection)
    {
        SelectedTheme = themeSelection;
        UpdateMenuChecks();
        ThemeChanged?.Invoke(this, EventArgs.Empty);
    }

    private void UpdateMenuChecks()
    {
        _useWindowsThemeMenuItem.Checked = SelectedTheme == ThemeSelection.Windows;
        _lightThemeMenuItem.Checked = SelectedTheme == ThemeSelection.Light;
        _darkThemeMenuItem.Checked = SelectedTheme == ThemeSelection.Dark;
    }
}
