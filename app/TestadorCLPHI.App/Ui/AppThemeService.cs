using Microsoft.Win32;

namespace TestadorCLPHI.App.Ui;

public static class AppThemeService
{
    public static bool WindowsIsInDarkTheme()
    {
        using RegistryKey? key = Registry.CurrentUser.OpenSubKey(
            @"Software\Microsoft\Windows\CurrentVersion\Themes\Personalize");

        object? value = key?.GetValue("AppsUseLightTheme");

        return value is int appsUseLightTheme && appsUseLightTheme == 0;
    }

    public static void ApplyTitleBar(Form form, bool darkMode)
    {
        WindowsTitleBarTheme.Apply(form, darkMode);
    }

    public static void ApplyButton(Button button, Color backgroundColor, Color textColor)
    {
        button.BackColor = backgroundColor;
        button.ForeColor = textColor;
        button.FlatStyle = FlatStyle.Flat;
    }
}
