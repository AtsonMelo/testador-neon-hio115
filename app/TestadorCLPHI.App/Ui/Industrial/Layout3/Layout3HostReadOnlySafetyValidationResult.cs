namespace TestadorCLPHI.App.Ui.Industrial.Layout3;

/// <summary>
/// Resultado imutavel da verificacao de seguranca read-only de um unico cenario.
/// Reune os dados observados (todos de catalogo/local) e as falhas detectadas.
/// Quando <see cref="Failures"/> esta vazio o cenario e considerado seguro.
/// </summary>
internal sealed record Layout3HostReadOnlySafetyValidationResult(
    string ScenarioName,
    string ProfileLabel,
    string CatalogProtocol,
    string CommunicationStatus,
    string Origin,
    int RealConnectionAttempts,
    int PhysicalCommandsExecuted,
    bool CommandGuardBlocked,
    IReadOnlyList<string> Failures)
{
    public bool IsSafe => Failures.Count == 0;

    public string ResultLabel => IsSafe ? "OK" : "ERRO";
}
