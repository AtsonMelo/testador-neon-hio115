namespace TestadorCLPHI.App.Plc;

public sealed class FakePlcCommunicationService : IPlcCommunicationService
{
    private readonly Dictionary<int, ushort> _registers = new();

    public PlcConnectionState State { get; } = new();

    public async Task ConnectAsync(
        PlcConnectionSettings settings,
        CancellationToken cancellationToken = default)
    {
        State.SetConnecting($"Conectando em {settings.PortName}...");

        await Task.Delay(300, cancellationToken);

        State.SetConnected($"Conectado em {settings.PortName} - simulação");
    }

    public Task DisconnectAsync(
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        State.SetDisconnected("Desconectado - simulação");

        return Task.CompletedTask;
    }

    public Task<ushort> ReadHoldingRegisterAsync(
        int registerAddress,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        EnsureConnected();

        if (_registers.TryGetValue(registerAddress, out ushort value))
        {
            return Task.FromResult(value);
        }

        return Task.FromResult((ushort)0);
    }

    public Task WriteHoldingRegisterAsync(
        int registerAddress,
        ushort value,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        EnsureConnected();

        _registers[registerAddress] = value;

        return Task.CompletedTask;
    }

    private void EnsureConnected()
    {
        if (!State.IsConnected)
        {
            throw new InvalidOperationException("O serviço fake não está conectado.");
        }
    }
}
