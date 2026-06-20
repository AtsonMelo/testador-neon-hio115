namespace TestadorCLPHI.App.Ui.Industrial.Layout3;

/// <summary>
/// Resultado imutavel da verificacao do bridge de leitura desabilitado para um
/// unico cenario. Reune os dados observados (todos derivados do snapshot local) e
/// as falhas detectadas. Quando <see cref="Failures"/> esta vazio o cenario e
/// considerado seguro (bridge desligado, sem conexao, sem leitura, sem comando).
/// </summary>
internal sealed record Layout3ReadBridgeSafetyValidationResult(
    string ScenarioName,
    string ProfileContext,
    string ProtocolContext,
    string BridgeStatus,
    string BridgeMode,
    bool ConnectionActive,
    int RealReads,
    int RealWrites,
    int PhysicalCommands,
    IReadOnlyList<string> Failures)
{
    public bool IsSafe => Failures.Count == 0;

    public string ResultLabel => IsSafe ? "OK" : "ERRO";
}
