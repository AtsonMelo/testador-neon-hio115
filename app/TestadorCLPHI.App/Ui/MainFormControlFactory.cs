namespace TestadorCLPHI.App.Ui;

internal static class MainFormControlFactory
{
    public static Label CriarLabel(string text, int left, int top)
    {
        return new Label
        {
            Text = text,
            Left = left,
            Top = top,
            Width = 85,
            AutoSize = true
        };
    }

    public static Label CriarLabelEstadoMensagem(int left, int top)
    {
        return new Label
        {
            Text = "Desconectado",
            Left = left,
            Top = top,
            Width = 230,
            Height = 55,
            AutoEllipsis = true
        };
    }
}
