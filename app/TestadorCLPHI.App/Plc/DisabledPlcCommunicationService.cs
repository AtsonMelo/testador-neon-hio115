namespace TestadorCLPHI.App.Plc;

public sealed class DisabledPlcCommunicationService : IPlcCommunicationService
{
    private const string DisabledMessage =
        "Comunicacao fisica desabilitada. Use somente o modo offline/simulado nesta versao.";

    public PlcConnectionState State { get; } = new();

    public Task ConnectAsync(
        PlcConnectionSettings settings,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        State.SetError(DisabledMessage);
        return Task.FromException(new InvalidOperationException(DisabledMessage));
    }

    public Task DisconnectAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        State.SetDisconnected("Comunicacao fisica permanece desabilitada.");
        return Task.CompletedTask;
    }

    public Task<ushort> ReadHoldingRegisterAsync(
        int registerAddress,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        State.SetError(DisabledMessage);
        return Task.FromException<ushort>(new InvalidOperationException(DisabledMessage));
    }

    public Task WriteHoldingRegisterAsync(
        int registerAddress,
        ushort value,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        State.SetError(DisabledMessage);
        return Task.FromException(new InvalidOperationException(DisabledMessage));
    }
}
