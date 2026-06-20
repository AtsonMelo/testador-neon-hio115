namespace TestadorCLPHI.App.Ui.Industrial.Layout3;

/// <summary>
/// Contrato do futuro bridge de leitura do host Layout 3.
///
/// O contrato existe apenas para preparar a arquitetura: dada uma selecao local
/// de perfil, ele devolve um <see cref="Layout3ReadBridgeSnapshot"/> imutavel.
/// A unica implementacao desta fase e <see cref="Layout3DisabledReadBridge"/>,
/// que e no-op e mantem tudo desligado. Uma implementacao real de leitura so
/// podera surgir em milestone futura, com aprovacao explicita; nada neste
/// contrato autoriza abrir conexao, ler/escrever registrador ou enviar comando.
/// </summary>
internal interface ILayout3ReadBridge
{
    /// <summary>Estado atual do bridge. Nesta fase, sempre desligado.</summary>
    Layout3ReadBridgeStatus Status { get; }

    /// <summary>
    /// Produz um snapshot local do bridge. Nao abre transporte e nao realiza
    /// nenhuma leitura: a selecao entra somente como contexto informativo.
    /// </summary>
    Layout3ReadBridgeSnapshot CreateSnapshot(Layout3ProfileSelection? selection);
}
