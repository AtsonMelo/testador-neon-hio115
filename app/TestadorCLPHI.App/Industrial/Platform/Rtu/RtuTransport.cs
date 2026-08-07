namespace TestadorCLPHI.App.Industrial.Platform.Rtu;

internal interface IRtuTransport
{
    bool IsSimulated { get; }

    ValueTask<ReadOnlyMemory<byte>?> ExchangeAsync(
        ReadOnlyMemory<byte> request,
        TimeSpan timeout,
        CancellationToken cancellationToken);
}

internal sealed class IndustrialOperationCounters
{
    private int _simulatedConnections;
    private int _simulatedReads;
    private int _simulatedWrites;
    private int _simulatedCommands;

    internal int SimulatedConnections => Volatile.Read(ref _simulatedConnections);
    internal int SimulatedReads => Volatile.Read(ref _simulatedReads);
    internal int SimulatedWrites => Volatile.Read(ref _simulatedWrites);
    internal int SimulatedCommands => Volatile.Read(ref _simulatedCommands);
    internal int PhysicalConnections => 0;
    internal int PhysicalReads => 0;
    internal int PhysicalWrites => 0;
    internal int PhysicalCommands => 0;

    internal void RecordSimulatedConnection() => Interlocked.Increment(ref _simulatedConnections);
    internal void RecordSimulatedRead() => Interlocked.Increment(ref _simulatedReads);

    internal void RecordSimulatedWrite()
    {
        Interlocked.Increment(ref _simulatedWrites);
        Interlocked.Increment(ref _simulatedCommands);
    }
}

internal sealed record IndustrialOperationLogEntry(
    DateTimeOffset Timestamp,
    string Mode,
    string Profile,
    string Transport,
    int? Address,
    string Operation,
    string? RegisterAlias,
    string Result,
    TimeSpan Duration,
    string? Error,
    bool Simulated);

internal sealed class InMemoryOperationLog
{
    private readonly List<IndustrialOperationLogEntry> _entries = [];
    private readonly object _syncRoot = new();

    internal IReadOnlyList<IndustrialOperationLogEntry> Entries
    {
        get
        {
            lock (_syncRoot)
            {
                return _entries.ToArray();
            }
        }
    }

    internal void Add(IndustrialOperationLogEntry entry)
    {
        lock (_syncRoot)
        {
            _entries.Add(entry);
        }
    }
}

internal sealed class InMemoryRtuTransport : IRtuTransport
{
    private readonly IReadOnlyDictionary<byte, NeonHio115FakeDevice> _devices;

    internal InMemoryRtuTransport(IEnumerable<NeonHio115FakeDevice> devices)
    {
        ArgumentNullException.ThrowIfNull(devices);
        _devices = devices.ToDictionary(device => device.Address);
    }

    public bool IsSimulated => true;

    public ValueTask<ReadOnlyMemory<byte>?> ExchangeAsync(
        ReadOnlyMemory<byte> request,
        TimeSpan timeout,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (timeout <= TimeSpan.Zero)
        {
            throw new ArgumentOutOfRangeException(nameof(timeout));
        }

        ReadOnlySpan<byte> frame = request.Span;
        if (frame.Length == 0 || !_devices.TryGetValue(frame[0], out NeonHio115FakeDevice? device))
        {
            return ValueTask.FromResult<ReadOnlyMemory<byte>?>(null);
        }

        if (device.ResponseMode == FakeDeviceResponseMode.Timeout)
        {
            throw new TimeoutException("Timeout simulado pelo transporte em memoria.");
        }

        byte[]? response = device.Process(request.Span);
        return ValueTask.FromResult<ReadOnlyMemory<byte>?>(response);
    }
}
