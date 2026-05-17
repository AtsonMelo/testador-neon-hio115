namespace TestadorCLPHI.App.Plc;

public sealed record PlcRegisterCommandResult(
    int RegisterAddress,
    ushort WrittenValue,
    ushort ReadBackValue);

public sealed class PlcRegisterCommandService
{
    private readonly IPlcCommunicationService _plcService;
    private readonly PlcConnectionSettings _connectionSettings;

    public PlcRegisterCommandService(
        IPlcCommunicationService plcService,
        PlcConnectionSettings connectionSettings)
    {
        _plcService = plcService;
        _connectionSettings = connectionSettings;
    }

    public async Task<ushort> ReadAsync(
        int registerAddress,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        await EnsureConnectedAsync(cancellationToken);

        cancellationToken.ThrowIfCancellationRequested();

        return await _plcService.ReadHoldingRegisterAsync(registerAddress);
    }

    public async Task<PlcRegisterCommandResult> WriteAndReadBackAsync(
        int registerAddress,
        ushort valueToWrite,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        await EnsureConnectedAsync(cancellationToken);

        cancellationToken.ThrowIfCancellationRequested();

        await _plcService.WriteHoldingRegisterAsync(
            registerAddress,
            valueToWrite);

        cancellationToken.ThrowIfCancellationRequested();

        ushort readBackValue = await _plcService.ReadHoldingRegisterAsync(
            registerAddress);

        return new PlcRegisterCommandResult(
            registerAddress,
            valueToWrite,
            readBackValue);
    }

    private async Task EnsureConnectedAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (!_plcService.State.IsConnected)
        {
            await _plcService.ConnectAsync(_connectionSettings);
        }
    }
}
