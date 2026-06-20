namespace TestadorCLPHI.App.Ui.Industrial.Layout3;

/// <summary>
/// Requisito futuro, declarativo e puramente local, que precisaria ser cumprido
/// antes de qualquer ativacao real do bridge de leitura do host Layout 3.
///
/// Nesta fase todo requisito nasce e permanece <see cref="Satisfied"/> = false: a
/// lista existe apenas para documentar, na propria interface e no validador, o que
/// ainda falta. Nenhum requisito aqui executa verificacao fisica, abre transporte
/// ou autoriza leitura; satisfazer um requisito de verdade exigira milestone
/// futura com aprovacao explicita.
/// </summary>
internal sealed record Layout3ReadBridgeActivationRequirement(
    string Title,
    string Detail,
    bool Satisfied)
{
    public string StatusLabel => Satisfied ? "ATENDIDO" : "PENDENTE";

    /// <summary>
    /// Cria um requisito futuro ainda nao atendido. E o unico modo de construcao
    /// usado nesta fase: todos os requisitos sao pendentes por definicao.
    /// </summary>
    public static Layout3ReadBridgeActivationRequirement Pending(string title, string detail)
    {
        return new Layout3ReadBridgeActivationRequirement(title, detail, Satisfied: false);
    }
}
