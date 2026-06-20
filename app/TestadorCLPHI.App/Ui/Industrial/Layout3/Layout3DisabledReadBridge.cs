namespace TestadorCLPHI.App.Ui.Industrial.Layout3;

/// <summary>
/// Implementacao desabilitada/no-op do bridge de leitura do host Layout 3.
///
/// Esta e a unica implementacao desta fase. Ela nunca abre conexao, nunca fala
/// protocolo de campo, nunca le ou escreve registrador e nunca executa comando
/// fisico. O <see cref="CreateSnapshot"/> apenas monta uma fotografia local a
/// partir do catalogo/selecao em memoria, sempre declarando o bridge desligado,
/// conexao ativa = false e os contadores reais fixados em 0.
///
/// Substituir esta classe por um bridge real exige uma milestone futura e
/// aprovacao explicita; nesta fase nenhum caminho de codigo cria transporte.
/// </summary>
internal sealed class Layout3DisabledReadBridge : ILayout3ReadBridge
{
    public Layout3ReadBridgeStatus Status => Layout3ReadBridgeStatus.Disabled;

    public Layout3ReadBridgeSnapshot CreateSnapshot(Layout3ProfileSelection? selection)
    {
        // No-op: nenhuma conexao e criada; a selecao entra apenas como contexto.
        return Layout3ReadBridgeSnapshot.Disabled(selection);
    }
}
