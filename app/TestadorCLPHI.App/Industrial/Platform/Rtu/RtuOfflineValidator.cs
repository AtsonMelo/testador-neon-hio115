using TestadorCLPHI.App.Ui.Industrial.Layout3;
using TestadorCLPHI.App.Plc;

namespace TestadorCLPHI.App.Industrial.Platform.Rtu;

internal static class RtuOfflineValidator
{
    private sealed record Scenario(string Name, Func<Task<bool>> Run);

    internal static int Validate(TextWriter output, TextWriter error)
    {
        try
        {
            return ValidateAsync(output, error).GetAwaiter().GetResult();
        }
        catch (Exception ex)
        {
            error.WriteLine($"Falha fechada no validador RTU offline: {ex.Message}");
            WritePhysicalZeroCounters(output);
            return 1;
        }
    }

    private static async Task<int> ValidateAsync(TextWriter output, TextWriter error)
    {
        ArgumentNullException.ThrowIfNull(output);
        ArgumentNullException.ThrowIfNull(error);

        IReadOnlyList<Scenario> scenarios = BuildScenarios();
        int passed = 0;
        output.WriteLine("VALIDADOR RTU OFFLINE - LAYOUT 3");
        output.WriteLine("Transporte: exclusivamente em memoria; nenhuma porta serial e enumerada ou aberta.");
        output.WriteLine();

        foreach (Scenario scenario in scenarios)
        {
            bool result;
            try
            {
                result = await scenario.Run();
            }
            catch (Exception ex)
            {
                result = false;
                output.WriteLine($"[ERRO] {scenario.Name}: {ex.GetType().Name}: {ex.Message}");
            }

            if (result)
            {
                passed++;
                output.WriteLine($"[OK] {scenario.Name}");
            }
            else if (!output.ToString()!.EndsWith(Environment.NewLine, StringComparison.Ordinal))
            {
                output.WriteLine($"[ERRO] {scenario.Name}");
            }
        }

        output.WriteLine();
        output.WriteLine($"Cenarios aprovados: {passed}/{scenarios.Count}");
        WritePhysicalZeroCounters(output);
        output.WriteLine(passed == scenarios.Count ? "Resultado: OFFLINE_READY" : "Resultado: ERRO");
        if (passed != scenarios.Count)
        {
            error.WriteLine("Validacao RTU offline falhou.");
        }

        return passed == scenarios.Count ? 0 : 1;
    }

    private static IReadOnlyList<Scenario> BuildScenarios() =>
    [
        Sync("CRC16 vetor conhecido", () =>
        {
            byte[] frame = RtuCrc16.Append([0x01, 0x03, 0x00, 0x00, 0x00, 0x0A]);
            return frame[^2] == 0xC5 && frame[^1] == 0xCD && RtuCrc16.IsValid(frame);
        }),
        Sync("CRC invalido rejeitado", () =>
        {
            byte[] frame = RtuCodec.CreateReadRequest(1, 30012, 1);
            frame[^1] ^= 0xFF;
            return Throws<RtuProtocolException>(() => RtuCodec.ValidateRequest(frame));
        }),
        Sync("frame incompleto rejeitado", () =>
            Throws<RtuProtocolException>(() => RtuCodec.ValidateRequest([1, 3, 0]))),
        Sync("frame maior rejeitado", () =>
            Throws<RtuProtocolException>(() => RtuCodec.ValidateRequest(new byte[9]))),
        Sync("endereco zero rejeitado", () =>
            Throws<ArgumentOutOfRangeException>(() => RtuCodec.CreateReadRequest(0, 30012, 1))),
        Sync("endereco 248 rejeitado", () =>
            Throws<ArgumentOutOfRangeException>(() => RtuCodec.CreateReadRequest(248, 30012, 1))),
        Sync("quantidade FC03 invalida rejeitada", () =>
            Throws<ArgumentOutOfRangeException>(() => RtuCodec.CreateReadRequest(1, 30012, 0))),
        Async("FC03 le PROG_ID", async () =>
        {
            TestContext context = CreateContext(new NeonHio115FakeDevice(1));
            ushort[] values = await context.Client.ReadAsync(1, 30012, 1, "PROG_ID", "TEST", CancellationToken.None);
            return values.SequenceEqual([(ushort)31134]);
        }),
        Async("slave diferente nao responde", async () =>
        {
            TestContext context = CreateContext(new NeonHio115FakeDevice(10));
            return await ThrowsAsync<RtuNoResponseException>(() =>
                context.Client.ReadAsync(1, 30012, 1, "PROG_ID", "TEST", CancellationToken.None));
        }),
        Async("timeout simulado", async () =>
        {
            NeonHio115FakeDevice device = new(1) { ResponseMode = FakeDeviceResponseMode.Timeout };
            TestContext context = CreateContext(device);
            return await ThrowsAsync<TimeoutException>(() =>
                context.Client.ReadAsync(1, 30012, 1, "PROG_ID", "TEST", CancellationToken.None));
        }),
        Async("cancelamento antes da troca", async () =>
        {
            TestContext context = CreateContext(new NeonHio115FakeDevice(1));
            using CancellationTokenSource cancellation = new();
            cancellation.Cancel();
            return await ThrowsAsync<OperationCanceledException>(() =>
                context.Client.ReadAsync(1, 30012, 1, "PROG_ID", "TEST", cancellation.Token));
        }),
        Async("CRC invalido na resposta", async () =>
        {
            NeonHio115FakeDevice device = new(1) { ResponseMode = FakeDeviceResponseMode.InvalidCrc };
            TestContext context = CreateContext(device);
            return await ThrowsAsync<RtuProtocolException>(() =>
                context.Client.ReadAsync(1, 30012, 1, "PROG_ID", "TEST", CancellationToken.None));
        }),
        Async("exception response invalid function", async () =>
        {
            NeonHio115FakeDevice device = new(1) { ResponseMode = FakeDeviceResponseMode.InvalidFunction };
            TestContext context = CreateContext(device);
            return await ThrowsAsync<RtuProtocolException>(() =>
                context.Client.ReadAsync(1, 30012, 1, "PROG_ID", "TEST", CancellationToken.None));
        }),
        Async("exception response invalid address", async () =>
        {
            NeonHio115FakeDevice device = new(1) { ResponseMode = FakeDeviceResponseMode.InvalidAddress };
            TestContext context = CreateContext(device);
            return await ThrowsAsync<RtuProtocolException>(() =>
                context.Client.ReadAsync(1, 30012, 1, "PROG_ID", "TEST", CancellationToken.None));
        }),
        DiscoveryAt("descoberta encontra endereco 1", 1),
        DiscoveryAt("descoberta encontra endereco 10", 10),
        DiscoveryAt("descoberta encontra endereco 247", 247),
        Async("descoberta sem equipamento", async () =>
        {
            TestContext context = CreateContext();
            RtuDiscoveryResult result = await context.Discovery.DiscoverAsync(1, 247, TimeSpan.Zero, null, CancellationToken.None);
            return result.Match is null && result.Attempts.Count == 247 && !result.Cancelled;
        }),
        Async("descoberta continua apos assinatura divergente", async () =>
        {
            TestContext context = CreateContext(
                new NeonHio115FakeDevice(1, programId: 1),
                new NeonHio115FakeDevice(2));
            RtuDiscoveryResult result = await context.Discovery.DiscoverAsync(1, 2, TimeSpan.Zero, null, CancellationToken.None);
            return result.Match?.Address == 2
                && result.Attempts.Select(item => item.State).SequenceEqual(
                    [RtuIdentificationState.SignatureMismatch, RtuIdentificationState.Identified]);
        }),
        Async("descoberta para no primeiro de dois equipamentos validos", async () =>
        {
            TestContext context = CreateContext(new NeonHio115FakeDevice(3), new NeonHio115FakeDevice(5));
            RtuDiscoveryResult result = await context.Discovery.DiscoverAsync(1, 10, TimeSpan.Zero, null, CancellationToken.None);
            return result.Match?.Address == 3 && result.Attempts.Count == 3;
        }),
        Async("descoberta reporta progresso deterministico", async () =>
        {
            TestContext context = CreateContext(new NeonHio115FakeDevice(2));
            List<RtuDiscoveryProgress> progress = [];
            SynchronousProgress<RtuDiscoveryProgress> reporter = new(progress.Add);
            RtuDiscoveryResult result = await context.Discovery.DiscoverAsync(1, 2, TimeSpan.Zero, reporter, CancellationToken.None);
            return result.Match?.Address == 2 && progress.Count == 4 && progress[^1].State == RtuIdentificationState.Identified;
        }),
        Async("descoberta cancelada", async () =>
        {
            TestContext context = CreateContext();
            using CancellationTokenSource cancellation = new();
            cancellation.Cancel();
            RtuDiscoveryResult result = await context.Discovery.DiscoverAsync(1, 247, TimeSpan.Zero, null, cancellation.Token);
            return result.Cancelled && result.Attempts.Count == 0;
        }),
        Async("F21 critico bloqueia identificacao", async () =>
        {
            TestContext context = CreateContext(new NeonHio115FakeDevice(1, generalFailureStatus: 1 << 10));
            RtuIdentificationResult result = await context.Identification.ProbeAsync(1, CancellationToken.None);
            return result.State == RtuIdentificationState.CriticalFault;
        }),
        Async("entradas DI e AI lidas do fake", async () =>
        {
            NeonHio115FakeDevice device = new(1);
            device.SetDigitalInput(3, true);
            device.SetAnalogInput(1, 1234);
            TestContext context = CreateContext(device);
            RtuInputSnapshot snapshot = await context.Inputs.ReadAsync(1, CancellationToken.None);
            return snapshot.DigitalInputs[3]
                && snapshot.DigitalInputs.Count(value => value) == 1
                && snapshot.AnalogInputs[1] == 1234;
        }),
        OutputCycle("DO00 ON/OFF", Layout3OutputChannel.DO00),
        OutputCycle("DO01 ON/OFF", Layout3OutputChannel.DO01),
        OutputCycle("DO02 ON/OFF", Layout3OutputChannel.DO02),
        OutputCycle("DO03 ON/OFF", Layout3OutputChannel.DO03),
        BlockedOutputReference("31137 reservado bloqueado", 31137),
        BlockedOutputReference("31140 reservado bloqueado", 31140),
        BlockedOutputReference("31143 reservado bloqueado", 31143),
        BlockedOutputReference("31144 PWM bloqueado", 31144),
        BlockedOutputReference("31145 PWM bloqueado", 31145),
        BlockedOutputReference("endereco arbitrario bloqueado", 32000),
        Async("saida rejeitada sem gate", async () =>
        {
            TestContext context = CreateContext(new NeonHio115FakeDevice(1));
            RtuOutputTestResult result = await context.Outputs.ActivateMomentaryAsync(
                1,
                Layout3OutputChannel.DO00,
                TimeSpan.FromMilliseconds(1),
                AuthorizedOutput() with { PhysicalGateAuthorized = false },
                CancellationToken.None);
            return result.State == RtuOutputTestState.Rejected;
        }),
        Async("duas saidas simultaneas bloqueadas", async () =>
        {
            NeonHio115FakeDevice device = new(1);
            TestContext context = CreateContext(device);
            Task<RtuOutputTestResult> first = context.Outputs.ActivateMomentaryAsync(
                1,
                Layout3OutputChannel.DO00,
                TimeSpan.FromMilliseconds(50),
                AuthorizedOutput(),
                CancellationToken.None);
            await Task.Delay(5);
            RtuOutputTestResult second = await context.Outputs.ActivateMomentaryAsync(
                1,
                Layout3OutputChannel.DO01,
                TimeSpan.FromMilliseconds(1),
                AuthorizedOutput(),
                CancellationToken.None);
            RtuOutputTestResult firstResult = await first;
            return firstResult.State == RtuOutputTestState.Completed
                && second.State == RtuOutputTestState.Rejected
                && !device.GetDigitalOutput(Layout3OutputChannel.DO00)
                && !device.GetDigitalOutput(Layout3OutputChannel.DO01);
        }),
        Async("cancelamento desliga saida", async () =>
        {
            NeonHio115FakeDevice device = new(1);
            TestContext context = CreateContext(device);
            using CancellationTokenSource cancellation = new(TimeSpan.FromMilliseconds(5));
            RtuOutputTestResult result = await context.Outputs.ActivateMomentaryAsync(
                1,
                Layout3OutputChannel.DO02,
                TimeSpan.FromMilliseconds(50),
                AuthorizedOutput(),
                cancellation.Token);
            return result.State == RtuOutputTestState.Cancelled
                && result.TurnOffConfirmed
                && !device.GetDigitalOutput(Layout3OutputChannel.DO02);
        }),
        Async("timeout no ON entra em atencao", async () =>
        {
            NeonHio115FakeDevice device = new(1) { ResponseMode = FakeDeviceResponseMode.Timeout };
            TestContext context = CreateContext(device);
            RtuOutputTestResult result = await context.Outputs.ActivateMomentaryAsync(
                1,
                Layout3OutputChannel.DO03,
                TimeSpan.FromMilliseconds(1),
                AuthorizedOutput(),
                CancellationToken.None);
            return result.State == RtuOutputTestState.AttentionRequired
                && !result.TurnOffConfirmed
                && !device.GetDigitalOutput(Layout3OutputChannel.DO03);
        }),
        Async("duracao acima do limite rejeitada", async () =>
        {
            TestContext context = CreateContext(new NeonHio115FakeDevice(1));
            RtuOutputTestResult result = await context.Outputs.ActivateMomentaryAsync(
                1,
                Layout3OutputChannel.DO00,
                TimeSpan.FromMilliseconds(1001),
                AuthorizedOutput(),
                CancellationToken.None);
            return result.State == RtuOutputTestState.Rejected;
        }),
        Sync("Read service nao oferece escrita", () =>
            !typeof(RtuInputTestService).GetMethods().Any(method => method.Name.Contains("Write", StringComparison.OrdinalIgnoreCase))),
        Sync("Identification nao oferece escrita", () =>
            !typeof(RtuEquipmentIdentificationService).GetMethods().Any(method => method.Name.Contains("Write", StringComparison.OrdinalIgnoreCase))),
        Sync("Output service nao aceita endereco de registrador", () =>
            typeof(RtuSupervisedOutputService).GetMethods()
                .Where(method => method.DeclaringType == typeof(RtuSupervisedOutputService))
                .SelectMany(method => method.GetParameters())
                .All(parameter => !parameter.Name!.Contains("register", StringComparison.OrdinalIgnoreCase))),
        Async("servico fisico default falha fechado", async () =>
        {
            DisabledPlcCommunicationService disabled = new();
            return await ThrowsAsync<InvalidOperationException>(() =>
                disabled.ConnectAsync(new PlcConnectionSettings { PortName = "COM-NOT-OPENED" }));
        }),
        Async("logs distinguem operacoes simuladas", async () =>
        {
            TestContext context = CreateContext(new NeonHio115FakeDevice(1));
            await context.Client.ReadAsync(1, 30012, 1, "PROG_ID", "TEST", CancellationToken.None);
            return context.Log.Entries.Count == 1
                && context.Log.Entries[0].Simulated
                && context.Log.Entries[0].Transport == "IN_MEMORY_RTU";
        }),
        Async("contadores fisicos permanecem zero", async () =>
        {
            TestContext context = CreateContext(new NeonHio115FakeDevice(1));
            await context.Identification.ProbeAsync(1, CancellationToken.None);
            return context.Counters.SimulatedReads == 3
                && context.Counters.PhysicalConnections == 0
                && context.Counters.PhysicalReads == 0
                && context.Counters.PhysicalWrites == 0
                && context.Counters.PhysicalCommands == 0;
        })
    ];

    private static Scenario DiscoveryAt(string name, byte address) => Async(name, async () =>
    {
        TestContext context = CreateContext(new NeonHio115FakeDevice(address));
        RtuDiscoveryResult result = await context.Discovery.DiscoverAsync(1, 247, TimeSpan.Zero, null, CancellationToken.None);
        return result.Match?.Address == address
            && result.Attempts.Count == address
            && context.Counters.SimulatedConnections == address;
    });

    private static Scenario OutputCycle(string name, Layout3OutputChannel channel) => Async(name, async () =>
    {
        NeonHio115FakeDevice device = new(1);
        TestContext context = CreateContext(device);
        RtuOutputTestResult result = await context.Outputs.ActivateMomentaryAsync(
            1,
            channel,
            TimeSpan.FromMilliseconds(1),
            AuthorizedOutput(),
            CancellationToken.None);
        return result.State == RtuOutputTestState.Completed
            && result.TurnOffConfirmed
            && !device.GetDigitalOutput(channel)
            && context.Counters.SimulatedWrites == 2
            && context.Counters.SimulatedCommands == 2;
    });

    private static Scenario BlockedOutputReference(string name, int reference) => Sync(name, () =>
    {
        NeonHio115FakeDevice device = new(1);
        byte[] request = RtuCodec.CreateWriteSingleRequest(1, checked((ushort)reference), 1);
        byte[] response = device.Process(request) ?? throw new InvalidOperationException("Fake nao respondeu.");
        return Throws<RtuProtocolException>(() => RtuCodec.ParseWriteSingleResponse(response, 1, checked((ushort)reference), 1));
    });

    private static Layout3OutputAuthorization AuthorizedOutput() => new(true, true, true, true, true, true);

    private static TestContext CreateContext(params NeonHio115FakeDevice[] devices)
    {
        IndustrialOperationCounters counters = new();
        InMemoryOperationLog log = new();
        RtuClient client = new(new InMemoryRtuTransport(devices), counters, log, TimeSpan.FromMilliseconds(500));
        RtuEquipmentIdentificationService identification = new(client);
        return new(
            client,
            identification,
            new RtuDiscoveryService(identification, counters),
            new RtuInputTestService(client),
            new RtuSupervisedOutputService(client, TimeSpan.FromSeconds(1)),
            counters,
            log);
    }

    private static Scenario Sync(string name, Func<bool> run) => new(name, () => Task.FromResult(run()));
    private static Scenario Async(string name, Func<Task<bool>> run) => new(name, run);

    private static bool Throws<TException>(Action action)
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

    private static async Task<bool> ThrowsAsync<TException>(Func<Task> action)
        where TException : Exception
    {
        try
        {
            await action();
            return false;
        }
        catch (TException)
        {
            return true;
        }
    }

    private static void WritePhysicalZeroCounters(TextWriter output)
    {
        output.WriteLine("PhysicalConnections: 0");
        output.WriteLine("PhysicalReads: 0");
        output.WriteLine("PhysicalWrites: 0");
        output.WriteLine("PhysicalCommands: 0");
    }

    private sealed record TestContext(
        RtuClient Client,
        RtuEquipmentIdentificationService Identification,
        RtuDiscoveryService Discovery,
        RtuInputTestService Inputs,
        RtuSupervisedOutputService Outputs,
        IndustrialOperationCounters Counters,
        InMemoryOperationLog Log);

    private sealed class SynchronousProgress<T>(Action<T> report) : IProgress<T>
    {
        public void Report(T value) => report(value);
    }
}
