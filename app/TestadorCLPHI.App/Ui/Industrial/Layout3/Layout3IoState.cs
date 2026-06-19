namespace TestadorCLPHI.App.Ui.Industrial.Layout3;

internal sealed record Layout3IoState(
    IReadOnlyList<bool?> Inputs,
    IReadOnlyList<bool> Outputs,
    string InputStatus,
    string OutputStatus)
{
    public static Layout3IoState Disconnected { get; } = new(
        Inputs: Enumerable.Repeat<bool?>(null, 8).ToArray(),
        Outputs: Enumerable.Repeat(false, 4).ToArray(),
        InputStatus: "Sem leitura fisica",
        OutputStatus: "Bloqueadas / inativas");
}
