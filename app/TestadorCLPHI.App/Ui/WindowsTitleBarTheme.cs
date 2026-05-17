using System.Runtime.InteropServices;

namespace TestadorCLPHI.App.Ui;

public static class WindowsTitleBarTheme
{
    private const int DwmwaUseImmersiveDarkModeBefore20h1 = 19;
    private const int DwmwaUseImmersiveDarkMode = 20;

    [DllImport("dwmapi.dll")]
    private static extern int DwmSetWindowAttribute(
        IntPtr hwnd,
        int attribute,
        ref int attributeValue,
        int attributeSize);

    public static void Apply(Form form, bool darkMode)
    {
        if (!OperatingSystem.IsWindowsVersionAtLeast(10, 0, 17763))
        {
            return;
        }

        int value = darkMode ? 1 : 0;

        int result = DwmSetWindowAttribute(
            form.Handle,
            DwmwaUseImmersiveDarkMode,
            ref value,
            sizeof(int));

        if (result != 0)
        {
            DwmSetWindowAttribute(
                form.Handle,
                DwmwaUseImmersiveDarkModeBefore20h1,
                ref value,
                sizeof(int));
        }
    }
}
