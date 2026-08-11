using System.Diagnostics;
using TestadorCLPHI.App.Industrial.Platform.Devices;
using TestadorCLPHI.App.Industrial.Platform.Safety;

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
    private readonly string _logIdentity;

    internal RtuClient(
        IRtuTransport transport,
        IndustrialOperationCounters counters,
        InMemoryOperationLog log,
        TimeSpan timeout,
        string logIdentity)
    {
        _transport = transport ?? throw new ArgumentNullException(nameof(transport));
        _counters = counters ?? throw new ArgumentNullException(nameof(counters));
        _log = log ?? throw new ArgumentNullException(nameof(log));
        _timeout = timeout > TimeSpan.Zero ? timeout : throw new ArgumentOutOfRangeException(nameof(timeout));
        _logIdentity = !string.IsNullOrWhiteSpace(logIdentity)
            ? logIdentity
            : throw new ArgumentException("Identidade de log ausente.", nameof(logIdentity));
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
        ushort documentedReference,
        string outputAlias,
        bool state,
        CancellationToken cancellationToken)
    {
        Stopwatch stopwatch = Stopwatch.StartNew();
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
                outputAlias,
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
                outputAlias,
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
            _logIdentity,
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
    private readonly DeviceIdentificationPolicy _policy;

    internal RtuEquipmentIdentificationService(
        RtuClient client,
        DeviceIdentificationPolicy policy)
    {
        _client = client ?? throw new ArgumentNullException(nameof(client));
        _policy = policy ?? throw new ArgumentNullException(nameof(policy));
    }

    internal async Task<RtuIdentificationResult> ProbeAsync(byte address, CancellationToken cancellationToken)
    {
        if (!_policy.IsSupported)
        {
            return new(
                address,
                RtuIdentificationState.UnknownDevice,
                null,
                null,
                null,
                "Identificacao operacional nao suportada pelo perfil.");
        }

        try
        {
            Dictionary<string, ushort> observations = new(StringComparer.OrdinalIgnoreCase);
            foreach (DeviceIdentificationProbe probe in _policy.Probes)
            {
                ushort value = (await _client.ReadAsync(
                    address,
                    probe.DocumentedReference,
                    1,
                    probe.Alias,
                    "IDENTIFICATION",
                    cancellationToken))[0];
                observations.Add(probe.Alias, value);
            }

            DeviceIdentificationEvaluation evaluation = _policy.Evaluate(observations);
            return new(
                address,
                evaluation.State switch
                {
                    DeviceIdentificationEvaluationState.Identified => RtuIdentificationState.Identified,
                    DeviceIdentificationEvaluationState.SignatureMismatch => RtuIdentificationState.SignatureMismatch,
                    DeviceIdentificationEvaluationState.CriticalFault => RtuIdentificationState.CriticalFault,
                    _ => RtuIdentificationState.UnknownDevice
                },
                evaluation.ProgramId,
                evaluation.ProgramCrc,
                evaluation.GeneralFailureStatus,
                evaluation.Detail);
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
    private readonly DeviceInputMap _inputMap;

    internal RtuInputTestService(RtuClient client, DeviceInputMap inputMap)
    {
        _client = client ?? throw new ArgumentNullException(nameof(client));
        _inputMap = inputMap ?? throw new ArgumentNullException(nameof(inputMap));
    }

    internal async Task<RtuInputSnapshot> ReadAsync(byte address, CancellationToken cancellationToken)
    {
        bool[] digitalValues = new bool[ResolveArrayLength(_inputMap.DigitalInputs)];
        ushort[] analogValues = new ushort[ResolveArrayLength(_inputMap.AnalogInputs)];

        foreach (DeviceInputBlock block in _inputMap.Blocks)
        {
            ValidateContiguousBlock(block);
            ushort[] values = await _client.ReadAsync(
                address,
                block.StartReference,
                checked((ushort)block.Points.Count),
                block.Alias,
                "INPUT_TEST",
                cancellationToken);

            for (int index = 0; index < block.Points.Count; index++)
            {
                DeviceRegisterPoint point = block.Points[index];
                if (block.Kind == DeviceInputKind.Digital)
                {
                    digitalValues[point.Channel] = values[index] != 0;
                }
                else
                {
                    analogValues[point.Channel] = values[index];
                }
            }
        }

        return new(digitalValues, analogValues);
    }

    private static int ResolveArrayLength(IReadOnlyList<DeviceRegisterPoint> points) =>
        points.Count == 0 ? 0 : points.Max(point => point.Channel) + 1;

    private static void ValidateContiguousBlock(DeviceInputBlock block)
    {
        if (block.Points.Count == 0)
        {
            throw new InvalidOperationException($"Bloco de entrada vazio: {block.Alias}.");
        }

        for (int index = 0; index < block.Points.Count; index++)
        {
            int expectedReference = block.StartReference + index;
            if (block.Points[index].DocumentedReference != expectedReference)
            {
                throw new InvalidOperationException(
                    $"Bloco de entrada nao contiguo: {block.Alias}.");
            }
        }
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
    private readonly DeviceOutputMap _outputMap;
    private readonly TimeSpan _maximumDuration;
    private readonly object _syncRoot = new();
    private Layout3OutputChannel? _activeChannel;

    internal RtuSupervisedOutputService(
        RtuClient client,
        DeviceOutputMap outputMap,
        TimeSpan maximumDuration)
    {
        _client = client ?? throw new ArgumentNullException(nameof(client));
        _outputMap = outputMap ?? throw new ArgumentNullException(nameof(outputMap));
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
            DeviceRegisterPoint output = ResolveOutput(channel);
            await _client.SetOutputAsync(
                address,
                output.DocumentedReference,
                output.Alias,
                state: true,
                cancellationToken);
            activationConfirmed = true;
            await Task.Delay(duration, cancellationToken);
            await _client.SetOutputAsync(
                address,
                output.DocumentedReference,
                output.Alias,
                state: false,
                CancellationToken.None);
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
        CancellationToken cancellationToken)
    {
        DeviceRegisterPoint output = ResolveOutput(channel);
        return _client.SetOutputAsync(
            address,
            output.DocumentedReference,
            output.Alias,
            state: false,
            cancellationToken);
    }

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
            DeviceRegisterPoint output = ResolveOutput(channel);
            await _client.SetOutputAsync(
                address,
                output.DocumentedReference,
                output.Alias,
                state: false,
                CancellationToken.None);
            return true;
        }
        catch
        {
            return false;
        }
    }

    private DeviceRegisterPoint ResolveOutput(Layout3OutputChannel channel)
    {
        if (!_outputMap.TryResolve(channel.ToString(), out DeviceRegisterPoint output))
        {
            throw new InvalidOperationException($"Saida {channel} nao suportada pelo perfil.");
        }

        return output;
    }
}
