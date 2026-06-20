namespace TestadorCLPHI.App.Ui.Industrial.Layout3;

/// <summary>
/// Resultado imutavel da verificacao do gate de ativacao bloqueado para um unico
/// cenario. Reune os dados observados (todos derivados da decisao local) e as
/// falhas detectadas. Quando <see cref="Failures"/> esta vazio o cenario e
/// considerado seguro (ativacao bloqueada, sem conexao, sem leitura, sem comando e
/// com requisitos futuros ainda pendentes).
/// </summary>
internal sealed record Layout3ReadBridgeActivationGateValidationResult(
    string ScenarioName,
    string ProfileContext,
    string ProtocolContext,
    string GateStatus,
    bool ActivationAllowed,
    bool ConnectionActive,
    int RealReads,
    int RealWrites,
    int PhysicalCommands,
    int PendingRequirements,
    int SatisfiedRequirements,
    IReadOnlyList<string> Failures)
{
    public bool IsSafe => Failures.Count == 0;

    public string ResultLabel => IsSafe ? "OK" : "ERRO";
}
