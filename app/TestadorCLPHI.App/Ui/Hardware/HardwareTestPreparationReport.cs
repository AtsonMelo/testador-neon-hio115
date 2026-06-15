namespace TestadorCLPHI.App.Ui.Hardware;

public sealed class HardwareTestPreparationReport
{
    public HardwareTestPreparationReport(string text)
    {
        Text = text ?? string.Empty;
    }

    public string Text { get; }

    public override string ToString()
    {
        return Text;
    }
}
