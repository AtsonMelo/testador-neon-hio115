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

    public static void ApplyTheme(Form form, ContextMenuStrip menu, bool darkMode)
    {
        Color backgroundColor;
        Color textColor;
        Color buttonColor;
        Color buttonTextColor;
        Color fieldColor;

        if (darkMode)
        {
            backgroundColor = Color.FromArgb(18, 24, 32);
            textColor = Color.FromArgb(226, 232, 240);
            buttonColor = Color.FromArgb(42, 52, 64);
            buttonTextColor = Color.FromArgb(226, 232, 240);
            fieldColor = Color.FromArgb(24, 32, 42);
        }
        else
        {
            backgroundColor = SystemColors.Control;
            textColor = Color.Black;
            buttonColor = SystemColors.Control;
            buttonTextColor = Color.Black;
            fieldColor = Color.White;
        }

        ApplyTitleBar(form, darkMode);

        ApplyControlTree(
            form,
            backgroundColor,
            textColor,
            fieldColor,
            buttonColor,
            buttonTextColor);

        ApplyMenuStrip(
            menu,
            fieldColor,
            textColor);
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

    public static void ApplyControlTree(
        Control root,
        Color backgroundColor,
        Color textColor,
        Color fieldColor,
        Color buttonColor,
        Color buttonTextColor)
    {
        root.BackColor = backgroundColor;
        root.ForeColor = textColor;

        foreach (Control control in root.Controls)
        {
            ApplyControlRecursive(
                control,
                backgroundColor,
                textColor,
                fieldColor,
                buttonColor,
                buttonTextColor);
        }
    }

    private static void ApplyControlRecursive(
        Control control,
        Color backgroundColor,
        Color textColor,
        Color fieldColor,
        Color buttonColor,
        Color buttonTextColor)
    {
        control.ForeColor = textColor;

        if (control is TextBox or ComboBox or CheckedListBox)
        {
            control.BackColor = fieldColor;
        }
        else if (control is Button button)
        {
            ApplyButton(button, buttonColor, buttonTextColor);
        }
        else
        {
            control.BackColor = backgroundColor;
        }

        foreach (Control child in control.Controls)
        {
            ApplyControlRecursive(
                child,
                backgroundColor,
                textColor,
                fieldColor,
                buttonColor,
                buttonTextColor);
        }
    }

    public static void ApplyMenuStrip(
        ContextMenuStrip menu,
        Color backgroundColor,
        Color textColor)
    {
        menu.BackColor = backgroundColor;
        menu.ForeColor = textColor;

        foreach (ToolStripItem item in menu.Items)
        {
            item.BackColor = backgroundColor;
            item.ForeColor = textColor;
        }
    }
}
