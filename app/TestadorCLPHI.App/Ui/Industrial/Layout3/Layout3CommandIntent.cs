namespace TestadorCLPHI.App.Ui.Industrial.Layout3;

internal sealed record Layout3CommandIntent(
    string Action,
    string Target,
    string Origin,
    DateTimeOffset CreatedAtUtc,
    Guid CorrelationId)
{
    public static Layout3CommandIntent Create(string action, string target, string origin)
    {
        return new Layout3CommandIntent(
            action,
            target,
            origin,
            DateTimeOffset.UtcNow,
            Guid.NewGuid());
    }
}

internal sealed record Layout3CommandDecision(
    bool Allowed,
    string Message,
    Guid CorrelationId);
