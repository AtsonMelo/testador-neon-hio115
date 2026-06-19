namespace TestadorCLPHI.App.Ui.Industrial.Layout3;

internal sealed class Layout3ReadOnlyCommandGuard : ILayout3CommandGuard
{
    public const string BlockedMessage =
        "Bloqueado pelo modo read-only. Nenhum comando físico foi enviado.";

    public Layout3CommandDecision Evaluate(Layout3CommandIntent intent)
    {
        ArgumentNullException.ThrowIfNull(intent);
        return new Layout3CommandDecision(
            Allowed: false,
            Message: BlockedMessage,
            CorrelationId: intent.CorrelationId);
    }
}
