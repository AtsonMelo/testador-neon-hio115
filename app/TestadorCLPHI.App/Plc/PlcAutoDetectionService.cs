namespace TestadorCLPHI.App.Plc;

public sealed class PlcAutoDetectionService
{
    private readonly IPlcCommunicationService _plcService;

    public PlcAutoDetectionService(IPlcCommunicationService plcService)
    {
        _plcService = plcService;
    }

    public async Task<PlcAutoDetectionResult?> DetectAsync(
        IReadOnlyList<string> portNames,
        IReadOnlyList<int> baudRates,
        byte? preferredSlaveId,
        PlcConnectionSettings baseSettings,
        int probeRegisterAddress,
        IProgress<string>? progress = null,
        int maxSlaveId = 30,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(portNames);
        ArgumentNullException.ThrowIfNull(baudRates);
        ArgumentNullException.ThrowIfNull(baseSettings);

        if (portNames.Count == 0)
        {
            _plcService.State.SetError("Nenhuma porta COM encontrada.");
            return null;
        }

        if (baudRates.Count == 0)
        {
            _plcService.State.SetError("Nenhum baud rate selecionado para busca.");
            return null;
        }

        byte[] slaveIds = BuildSlaveIdList(preferredSlaveId, maxSlaveId);


        int attemptCount = 0;
foreach (string portName in portNames)
        {
            foreach (int baudRate in baudRates)
            {
                foreach (byte slaveId in slaveIds)
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    attemptCount++;

                    progress?.Report($"Tentativa {attemptCount}: {portName} / {baudRate} bps / Slave {slaveId}...");

                    PlcConnectionSettings attemptSettings = new()
                    {
                        PortName = portName,
                        BaudRate = baudRate,
                        Parity = baseSettings.Parity,
                        DataBits = baseSettings.DataBits,
                        StopBits = baseSettings.StopBits,
                        SlaveId = slaveId,
                        TimeoutMilliseconds = 200
                    };

                    try
                    {
                        await _plcService.ConnectAsync(attemptSettings);

                        ushort value = await _plcService.ReadHoldingRegisterAsync(
                            probeRegisterAddress);

                        _plcService.State.SetConnected(
                            $"CLP detectado: {portName}, {baudRate} bps, Slave {slaveId}, %MW{probeRegisterAddress} = {value}");

                        PlcConnectionSettings detectedSettings = new()
                        {
                            PortName = portName,
                            BaudRate = baudRate,
                            Parity = attemptSettings.Parity,
                            DataBits = attemptSettings.DataBits,
                            StopBits = attemptSettings.StopBits,
                            SlaveId = slaveId,
                            TimeoutMilliseconds = baseSettings.TimeoutMilliseconds
                        };

                        return new PlcAutoDetectionResult(
                            detectedSettings,
                            value,
                            attemptCount);
                    }
                    catch
                    {
                        await TryDisconnectAsync();
                    }
                }
            }
        }

        _plcService.State.SetError($"Nenhum CLP encontrado após {attemptCount} tentativa(s), entre Slave ID 1 e {maxSlaveId}.");
        return null;
    }

    private static byte[] BuildSlaveIdList(byte? preferredSlaveId, int maxSlaveId)
    {
        List<byte> slaveIds = [];

        if (preferredSlaveId is >= 1 && preferredSlaveId <= maxSlaveId)
        {
            slaveIds.Add(preferredSlaveId.Value);
        }

        for (byte slaveId = 1; slaveId <= maxSlaveId; slaveId++)
        {
            if (!slaveIds.Contains(slaveId))
            {
                slaveIds.Add(slaveId);
            }
        }

        return slaveIds.ToArray();
    }

    private async Task TryDisconnectAsync()
    {
        try
        {
            await _plcService.DisconnectAsync();
        }
        catch
        {
            // Ignora falha ao desconectar durante tentativa de detecção.
            // A detecção deve continuar testando as próximas combinações.
        }
    }
}
