using TestadorCLPHI.App.Hardware;
using TestadorCLPHI.App.Industrial.Platform.Integration;
using TestadorCLPHI.App.Industrial.Platform.Mapping;
using TestadorCLPHI.App.Industrial.Platform.Rtu;
using TestadorCLPHI.App.Industrial.Platform.Simulation;
using TestadorCLPHI.App.Industrial.Transport;

List<CheckResult> results = [];

SimulationProfile pivot = LoadProfile("pivo-central");
SimulationProfile well = LoadProfile("poco");
HardwareCatalog catalog = LoadHardwareCatalog();

Check("catalogo de hardware desserializa e valida", () =>
    HardwareCatalogValidator.Validate(catalog).IsValid);
Check("perfil Pivo desserializa e valida", () =>
    SimulationProfileValidator.Validate(pivot).IsValid);
Check("perfil Poco desserializa e valida", () =>
    SimulationProfileValidator.Validate(well).IsValid);
Check("SafetyChain inicia derivada e fechada", () =>
{
    SimulationEngine engine = new(pivot);
    return engine.GetValue("SafetyChain") == 1;
});
Check("falha de torre abre SafetyChain e bloqueia saidas", () =>
{
    SimulationEngine engine = new(pivot);
    engine.SetDigitalInput("Tower3Safety", false);
    return engine.GetValue("SafetyChain") == 0
        && engine.Snapshot.OutputsBlocked
        && engine.Snapshot.ActiveAlarms.Contains("SafetyChainOpen");
});
Check("SafetyChain derivada rejeita escrita direta", () =>
{
    SimulationEngine engine = new(pivot);
    return Throws<InvalidOperationException>(() => engine.SetDigitalInput("SafetyChain", false));
});
Check("mapeamento de I/O do Pivo permanece valido", () =>
    IndustrialIoMappingValidator.Validate(pivot).IsValid);
Check("mapeamento de I/O do Poco permanece valido", () =>
    IndustrialIoMappingValidator.Validate(well).IsValid);
Check("CRC16 RTU preserva vetor conhecido", () =>
{
    byte[] frame = RtuCrc16.Append([0x01, 0x03, 0x00, 0x00, 0x00, 0x0A]);
    return frame[^2] == 0xC5 && frame[^1] == 0xCD && RtuCrc16.IsValid(frame);
});
Check("codec RTU cria request valido sem transporte", () =>
{
    byte[] frame = RtuCodec.CreateReadRequest(1, 30012, 1);
    return frame.Length == 8 && frame[0] == 1 && frame[1] == RtuCodec.ReadHoldingRegistersFunction;
});
Check("configuracao serial neutra cobre 8N1", () =>
    IndustrialSerialParity.None.ToString() == "None"
    && IndustrialSerialStopBits.One.ToString() == "One");
Check("sessao in-memory identifica fake HIO115", () => Task.Run(async () =>
{
    using IndustrialPlatformSession session = new(pivot);
    RtuIdentificationResult identification = await session.IdentifyAsync(1, CancellationToken.None);
    return identification.State == RtuIdentificationState.Identified;
}).GetAwaiter().GetResult());
Check("transporte in-memory respeita cancelamento", () => Task.Run(async () =>
{
    using CancellationTokenSource cancellation = new();
    cancellation.Cancel();
    try
    {
        InMemoryRtuTransport transport = new([]);
        await transport.ExchangeAsync(
            RtuCodec.CreateReadRequest(1, 30012, 1),
            TimeSpan.FromMilliseconds(100),
            cancellation.Token);
        return false;
    }
    catch (OperationCanceledException)
    {
        return true;
    }
}).GetAwaiter().GetResult());
Check("sessao in-memory preserva contadores fisicos em zero", () =>
{
    using IndustrialPlatformSession session = new(pivot);
    return PhysicalCountersAreZero(session.Counters);
});
Check("camadas compartilhadas operam sem WinForms", () =>
    typeof(SimulationEngine).Assembly.GetReferencedAssemblies().All(name =>
        !string.Equals(name.Name, "System.Windows.Forms", StringComparison.Ordinal))
    && typeof(IndustrialPlatformSession).Assembly.GetReferencedAssemblies().All(name =>
        !string.Equals(name.Name, "System.Windows.Forms", StringComparison.Ordinal)));

foreach (CheckResult result in results)
{
    Console.WriteLine($"[{(result.Passed ? "OK" : "FALHA")}] {result.Name}");
}

int passed = results.Count(result => result.Passed);
Console.WriteLine();
Console.WriteLine($"Core/Application neutral validation: {passed}/{results.Count}");
Console.WriteLine("PhysicalConnections=0");
Console.WriteLine("PhysicalReads=0");
Console.WriteLine("PhysicalWrites=0");
Console.WriteLine("PhysicalCommands=0");
Console.WriteLine(passed == results.Count ? "CORE_OFFLINE_READY" : "CORE_VALIDATION_FAILED");
return passed == results.Count ? 0 : 1;

void Check(string name, Func<bool> test)
{
    try
    {
        results.Add(new(name, test()));
    }
    catch (Exception exception)
    {
        results.Add(new(name, false, $"{exception.GetType().Name}: {exception.Message}"));
    }
}

static SimulationProfile LoadProfile(string id)
{
    string path = Path.Combine(AppContext.BaseDirectory, "Data", "Simulation", "Profiles", id + ".json");
    return SimulationProfileJson.Deserialize(File.ReadAllText(path), path);
}

static HardwareCatalog LoadHardwareCatalog()
{
    string path = Path.Combine(AppContext.BaseDirectory, "Data", "Hardware", "hi-hardware-catalog.json");
    return HardwareCatalogJson.Deserialize(File.ReadAllText(path), path);
}

static bool PhysicalCountersAreZero(IndustrialOperationCounters counters) =>
    counters.PhysicalConnections == 0
    && counters.PhysicalReads == 0
    && counters.PhysicalWrites == 0
    && counters.PhysicalCommands == 0;

static bool Throws<TException>(Action action)
    where TException : Exception
{
    try
    {
        action();
        return false;
    }
    catch (TException)
    {
        return true;
    }
}

internal sealed record CheckResult(string Name, bool Passed, string? Detail = null);
