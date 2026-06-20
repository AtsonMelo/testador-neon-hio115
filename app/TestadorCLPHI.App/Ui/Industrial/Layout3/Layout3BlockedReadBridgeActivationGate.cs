namespace TestadorCLPHI.App.Ui.Industrial.Layout3;

/// <summary>
/// Implementacao bloqueada/no-op do gate de ativacao do bridge de leitura do host
/// Layout 3.
///
/// Esta e a unica implementacao desta fase. Ela nunca libera ativacao, nunca abre
/// conexao, nunca fala protocolo de campo, nunca le ou escreve registrador e nunca
/// executa comando fisico. O <see cref="Evaluate"/> apenas monta uma decisao local
/// a partir do catalogo/selecao em memoria, sempre declarando ativacao bloqueada,
/// conexao ativa = false e os contadores reais fixados em 0, com os requisitos
/// futuros pendentes.
///
/// Substituir esta classe por um gate que libere leitura real exige uma milestone
/// futura e aprovacao explicita; nesta fase nenhum caminho de codigo autoriza
/// ativacao.
/// </summary>
internal sealed class Layout3BlockedReadBridgeActivationGate : ILayout3ReadBridgeActivationGate
{
    public Layout3ReadBridgeActivationStatus Status => Layout3ReadBridgeActivationStatus.Blocked;

    public Layout3ReadBridgeActivationDecision Evaluate(Layout3ProfileSelection? selection)
    {
        // No-op: nenhuma ativacao e concedida; a selecao entra apenas como contexto.
        return Layout3ReadBridgeActivationDecision.Blocked(selection);
    }
}
