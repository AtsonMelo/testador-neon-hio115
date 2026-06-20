namespace TestadorCLPHI.App.Ui.Industrial.Layout3;

/// <summary>
/// Contrato do gate de ativacao do futuro bridge de leitura do host Layout 3.
///
/// O contrato existe apenas para preparar a arquitetura de decisao: dada uma
/// selecao local de perfil, ele devolve uma <see cref="Layout3ReadBridgeActivationDecision"/>
/// imutavel. A unica implementacao desta fase e
/// <see cref="Layout3BlockedReadBridgeActivationGate"/>, que sempre bloqueia. Um
/// gate que libere ativacao real so podera surgir em milestone futura, com
/// aprovacao explicita; nada neste contrato autoriza abrir conexao, ler/escrever
/// registrador ou enviar comando.
/// </summary>
internal interface ILayout3ReadBridgeActivationGate
{
    /// <summary>Estado atual do gate. Nesta fase, sempre bloqueado.</summary>
    Layout3ReadBridgeActivationStatus Status { get; }

    /// <summary>
    /// Avalia a ativacao para uma selecao local de perfil. Nao abre transporte e
    /// nao realiza nenhuma leitura: a selecao entra somente como contexto e a
    /// decisao nesta fase e sempre bloqueada.
    /// </summary>
    Layout3ReadBridgeActivationDecision Evaluate(Layout3ProfileSelection? selection);
}
