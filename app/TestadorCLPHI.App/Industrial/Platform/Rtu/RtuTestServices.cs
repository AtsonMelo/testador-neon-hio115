using System.Diagnostics;
using TestadorCLPHI.App.Ui.Industrial.Layout3;

namespace TestadorCLPHI.App.Industrial.Platform.Rtu;

internal sealed class RtuNoResponseException : Exception
{
    internal RtuNoResponseException(byte address)
        : base($"Sem resposta simulada no endereco {address}.")
    {
    }
}

internal sealed class RtuClient
{
    private readonly IRtuTransport _transport;
    private readonly IndustrialOperationCounters _counters;
    private readonly InMemoryOperationLog _log;
    private readonly TimeSpan _timeout;

    internal RtuClient(
        IRtuTransport transport,
        IndustrialOperationCounters counters,
        InMemoryOperationLog log,
        TimeSpan timeout)
    {
        _transport = transport ?? throw new ArgumentNullException(nameof(transport));
        _counters = counters ?? throw new ArgumentNullException(nameof(counters));
        _log = log ?? throw new ArgumentNullException(nameof(log));
        _timeout = timeout > TimeSpan.Zero ? timeout : throw new ArgumentOutOfRangeException(nameof(timeout));
    }

    internal bool IsSimulated => _transport.IsSimulated;

    internal async Task<ushort[]> ReadAsync(
        byte deviceAddress,
        ushort startReference,
        ushort quantity,
        string registerAlias,
        string mode,
        CancellationToken cancellationToken)
    {
        Stopwatch stopwatch = Stopwatch.StartNew();
        try
        {
            byte[] request = RtuCodec.CreateReadRequest(deviceAddress, startReference, quantity);
            ReadOnlyMemory<byte>? response = await _transport.ExchangeAsync(request, _timeout, cancellationToken);
            if (response is null)
            {
                throw new RtuNoResponseException(deviceAddress);
            }

            ushort[] values = RtuCodec.ParseReadResponse(response.Value.Span, deviceAddress, quantity);
            _counters.RecordSimulatedRead();
            AddLog(mode, deviceAddress, "FC03_READ", registerAlias, "OK", stopwatch.Elapsed, null);
            return values;
        }
        catch (Exception ex)
        {
            AddLog(mode, deviceAddress, "FC03_READ", registerAlias, "ERROR", stopwatch.Elapsed, ex.Message);
            throw;
        }
    }

    internal async Task SetOutputAsync(
        byte deviceAddress,
        Layout3OutputChannel channel,
        bool state,
        CancellationToken cancellationToken)
    {
        Stopwatch stopwatch = Stopwatch.StartNew();
        ushort documentedReference = checked((ushort)Layout3BenchWorkflowPolicy.GetOutputDocumentedReference(channel));
        ushort value = state ? (ushort)1 : (ushort)0;
        try
        {
            byte[] request = RtuCodec.CreateWriteSingleRequest(deviceAddress, documentedReference, value);
            ReadOnlyMemory<byte>? response = await _transport.ExchangeAsync(request, _timeout, cancellationToken);
            if (response is null)
            {
                throw new RtuNoResponseException(deviceAddress);
            }

            RtuCodec.ParseWriteSingleResponse(response.Value.Span, deviceAddress, documentedReference, value);
            _counters.RecordSimulatedWrite();
            AddLog(
                "SUPERVISED_OUTPUT_TEST",
                deviceAddress,
                state ? "OUTPUT_ON" : "OUTPUT_OFF",
                channel.ToString(),
                "OK",
                stopwatch.Elapsed,
                null);
        }
        catch (Exception ex)
        {
            AddLog(
                "SUPERVISED_OUTPUT_TEST",
                deviceAddress,
                state ? "OUTPUT_ON" : "OUTPUT_OFF",
                channel.ToString(),
                "ERROR",
                stopwatch.Elapsed,
                ex.Message);
            throw;
        }
    }

    private void AddLog(
        string mode,
        byte address,
        string operation,
        string registerAlias,
        string result,
        TimeSpan duration,
        string? error) =>
        _log.Add(new IndustrialOperationLogEntry(
            DateTimeOffset.UtcNow,
            mode,
            "NEON5-CPU450-HIO115",
            "IN_MEMORY_RTU",
            address,
            operation,
            registerAlias,
            result,
            duration,
            error,
            Simulated: true));
}

internal enum RtuIdentificationState
{
    NoResponse,
    ModbusResponse,
    UnknownDevice,
    KnownFamily,
    KnownProgram,
    SignatureMismatch,
    CriticalFault,
    Identified,
    Cancelled,
    Timeout
}

internal sealed record RtuIdentificationResult(
    byte Address,
    RtuIdentificationState State,
    ushort? ProgramId,
    ushort? ProgramCrc,
    ushort? GeneralFailureStatus,
    string? Detail);

internal sealed record RtuDiscoveryProgress(int Attempt, int Total, byte Address, RtuIdentificationState? State);

internal sealed record RtuDiscoveryResult(
    RtuIdentificationResult? Match,
    IReadOnlyList<RtuIdentificationResult> Attempts,
    bool Cancelled);

internal sealed class RtuEquipmentIdentificationService
{
    private readonly RtuClient _client;

    internal RtuEquipmentIdentificationService(RtuClient client)
    {
        _client = client;
    }

    internal async Task<RtuIdentificationResult> ProbeAsync(byte address, CancellationToken cancellationToken)
    {
        try
        {
            ushort programId = (await _client.ReadAsync(
                address,
                30012,
                1,
                "PROG_ID",
                "IDENTIFICATION",
                cancellationToken))[0];
            ushort programCrc = (await _client.ReadAsync(
                address,
                30013,
                1,
                "PROG_CRC",
                "IDENTIFICATION",
                cancellationToken))[0];
            ushort failureStatus = (await _client.ReadAsync(
                address,
                30021,
                1,
                "DEV_GFAIL_STS",
                "IDENTIFICATION",
                cancellationToken))[0];

            if (Layout3BenchWorkflowPolicy.HasCriticalFailure(failureStatus))
            {
                return new(address, RtuIdentificationState.CriticalFault, programId, programCrc, failureStatus, "F21 critico.");
            }

            if (programId != Layout3BenchWorkflowPolicy.ExpectedProgramId
                || programCrc != Layout3BenchWorkflowPolicy.ExpectedProgramCrc)
            {
                return new(
                    address,
                    RtuIdentificationState.SignatureMismatch,
                    programId,
                    programCrc,
                    failureStatus,
                    "Resposta RTU valida, assinatura de programa divergente.");
            }

            return new(
                address,
                RtuIdentificationState.Identified,
                programId,
                programCrc,
                failureStatus,
                "Programa conhecido; F10/F11 permanecem opcionais ate mapeamento confirmado.");
        }
        catch (RtuNoResponseException ex)
        {
            return new(address, RtuIdentificationState.NoResponse, null, null, null, ex.Message);
        }
        catch (TimeoutException ex)
        {
            return new(address, RtuIdentificationState.Timeout, null, null, null, ex.Message);
        }
        catch (OperationCanceledException)
        {
            return new(address, RtuIdentificationState.Cancelled, null, null, null, "Cancelado.");
        }
        catch (RtuProtocolException ex)
        {
            return new(address, RtuIdentificationState.UnknownDevice, null, null, null, ex.Message);
        }
    }
}

internal sealed class RtuDiscoveryService
{
    private readonly RtuEquipmentIdentificationService _identification;
    private readonly IndustrialOperationCounters _counters;

    internal RtuDiscoveryService(
        RtuEquipmentIdentificationService identification,
        IndustrialOperationCounters counters)
    {
        _identification = identification;
        _counters = counters;
    }

    internal async Task<RtuDiscoveryResult> DiscoverAsync(
        byte startAddress,
        byte endAddress,
        TimeSpan attemptInterval,
        IProgress<RtuDiscoveryProgress>? progress,
        CancellationToken cancellationToken)
    {
        if (startAddress is 0 or > 247
            || endAddress is 0 or > 247
            || startAddress > endAddress)
        {
            throw new ArgumentOutOfRangeException(nameof(startAddress), "Faixa deve permanecer crescente em 1..247.");
        }

        if (attemptInterval < TimeSpan.Zero)
        {
            throw new ArgumentOutOfRangeException(nameof(attemptInterval));
        }

        List<RtuIdentificationResult> attempts = [];
        int total = endAddress - startAddress + 1;
        int attempt = 0;
        for (int current = startAddress; current <= endAddress; current++)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                return new(null, attempts, Cancelled: true);
            }

            attempt++;
            byte address = checked((byte)current);
            _counters.RecordSimulatedConnection();
            progress?.Report(new(attempt, total, address, null));
            RtuIdentificationResult result = await _identification.ProbeAsync(address, cancellationToken);
            attempts.Add(result);
            progress?.Report(new(attempt, total, address, result.State));
            if (result.State == RtuIdentificationState.Identified)
            {
                return new(result, attempts, Cancelled: false);
            }

            if (attemptInterval > TimeSpan.Zero && current < endAddress)
            {
                try
                {
                    await Task.Delay(attemptInterval, cancellationToken);
                }
                catch (OperationCanceledException)
                {
                    return new(null, attempts, Cancelled: true);
                }
            }
        }

        return new(null, attempts, Cancelled: false);
    }
}

internal sealed record RtuInputSnapshot(bool[] DigitalInputs, ushort[] AnalogInputs);

internal sealed class RtuInputTestService
{
    private readonly RtuClient _client;

    internal RtuInputTestService(RtuClient client)
    {
        _client = client;
    }

    internal async Task<RtuInputSnapshot> ReadAsync(byte address, CancellationToken cancellationToken)
    {
        ushort[] digitalValues = await _client.ReadAsync(
            address,
            31120,
            8,
            "DI00..DI07",
            "INPUT_TEST",
            cancellationToken);
        ushort[] analogValues = await _client.ReadAsync(
            address,
            31132,
            3,
            "AI00..AI02",
            "INPUT_TEST",
            cancellationToken);
        return new(digitalValues.Select(value => value != 0).ToArray(), analogValues);
    }
}

internal enum RtuOutputTestState
{
    Completed,
    Rejected,
    Cancelled,
    AttentionRequired
}

internal sealed record RtuOutputTestResult(
    RtuOutputTestState State,
    Layout3OutputChannel Channel,
    bool TurnOffConfirmed,
    string Detail);

internal sealed class RtuSupervisedOutputService
{
    private readonly RtuClient _client;
    private readonly TimeSpan _maximumDuration;
    private readonly object _syncRoot = new();
    private Layout3OutputChannel? _activeChannel;

    internal RtuSupervisedOutputService(RtuClient client, TimeSpan maximumDuration)
    {
        _client = client;
        _maximumDuration = maximumDuration > TimeSpan.Zero
            ? maximumDuration
            : throw new ArgumentOutOfRangeException(nameof(maximumDuration));
    }

    internal async Task<RtuOutputTestResult> ActivateMomentaryAsync(
        byte address,
        Layout3OutputChannel channel,
        TimeSpan duration,
        Layout3OutputAuthorization authorization,
        CancellationToken cancellationToken)
    {
        bool gateAuthorized = _client.IsSimulated
            ? Layout3BenchWorkflowPolicy.CanEnableSimulatedOutput(authorization)
            : Layout3BenchWorkflowPolicy.CanEnableSupervisedOutput(authorization);
        if (!gateAuthorized
            || duration <= TimeSpan.Zero
            || duration > _maximumDuration)
        {
            return new(RtuOutputTestState.Rejected, channel, false, "Gate ou duracao rejeitados.");
        }

        lock (_syncRoot)
        {
            if (_activeChannel is not null)
            {
                return new(RtuOutputTestState.Rejected, channel, false, "Outra saida ja esta ativa.");
            }

            _activeChannel = channel;
        }

        bool activationConfirmed = false;
        bool turnOffConfirmed = false;
        try
        {
            await _client.SetOutputAsync(address, channel, state: true, cancellationToken);
            activationConfirmed = true;
            await Task.Delay(duration, cancellationToken);
            await _client.SetOutputAsync(address, channel, state: false, CancellationToken.None);
            turnOffConfirmed = true;
            return new(RtuOutputTestState.Completed, channel, true, "Ciclo momentaneo concluido no fake.");
        }
        catch (OperationCanceledException)
        {
            turnOffConfirmed = await TryTurnOffAsync(address, channel, activationConfirmed);
            return new(RtuOutputTestState.Cancelled, channel, turnOffConfirmed, "Cancelado; desligamento solicitado.");
        }
        catch (Exception ex)
        {
            turnOffConfirmed = await TryTurnOffAsync(address, channel, activationConfirmed);
            return new(
                RtuOutputTestState.AttentionRequired,
                channel,
                turnOffConfirmed,
                $"Falha simulada: {ex.Message}");
        }
        finally
        {
            lock (_syncRoot)
            {
                _activeChannel = null;
            }
        }
    }

    internal Task TurnOffAsync(
        byte address,
        Layout3OutputChannel channel,
        CancellationToken cancellationToken) =>
        _client.SetOutputAsync(address, channel, state: false, cancellationToken);

    private async Task<bool> TryTurnOffAsync(
        byte address,
        Layout3OutputChannel channel,
        bool activationConfirmed)
    {
        if (!activationConfirmed)
        {
            return false;
        }

        try
        {
            await _client.SetOutputAsync(address, channel, state: false, CancellationToken.None);
            return true;
        }
        catch
        {
            return false;
        }
    }
}
