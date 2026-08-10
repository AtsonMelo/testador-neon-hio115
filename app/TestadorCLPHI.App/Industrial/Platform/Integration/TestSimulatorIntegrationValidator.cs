using TestadorCLPHI.App.Industrial.Platform.Devices;
using TestadorCLPHI.App.Industrial.Platform.Rtu;
using TestadorCLPHI.App.Industrial.Platform.Simulation;
using TestadorCLPHI.App.Ui.Industrial.Layout3;

namespace TestadorCLPHI.App.Industrial.Platform.Integration;

internal static class TestSimulatorIntegrationValidator
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
            error.WriteLine($"Falha fechada na integracao: {ex.Message}");
            WritePhysicalZeroCounters(output);
            return 1;
        }
    }

    private static async Task<int> ValidateAsync(TextWriter output, TextWriter error)
    {
        IReadOnlyList<Scenario> scenarios = BuildScenarios();
        int passed = 0;
        output.WriteLine("VALIDADOR TESTADOR + SIMULADOR EM MEMORIA");
        output.WriteLine("Nenhuma porta COM, rede ou equipamento fisico participa dos cenarios.");
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
            else
            {
                output.WriteLine($"[ERRO] {scenario.Name}");
            }
        }

        output.WriteLine();
        output.WriteLine($"Cenarios aprovados: {passed}/{scenarios.Count}");
        WritePhysicalZeroCounters(output);
        output.WriteLine(passed == scenarios.Count ? "Resultado: SIMULATION_READY" : "Resultado: ERRO");
        if (passed != scenarios.Count)
        {
            error.WriteLine("Integracao Testador + Simulador falhou.");
        }

        return passed == scenarios.Count ? 0 : 1;
    }

    private static IReadOnlyList<Scenario> BuildScenarios() =>
    [
        new("Pivo: discovery e identificacao", async () =>
        {
            using IntegrationContext context = CreateContext("pivo-central", 1);
            RtuDiscoveryResult discovery = await context.Discovery.DiscoverAsync(1, 247, TimeSpan.Zero, null, CancellationToken.None);
            return discovery.Match?.Address == 1
                && discovery.Match.State == RtuIdentificationState.Identified;
        }),
        new("Pivo: Pressostato chega a DI01", async () =>
        {
            using IntegrationContext context = CreateContext("pivo-central", 1);
            context.Engine.SetDigitalInput("Pressostato", false);
            context.Adapter.SyncInputsToDevice();
            RtuInputSnapshot snapshot = await context.Inputs.ReadAsync(1, CancellationToken.None);
            return !snapshot.DigitalInputs[1];
        }),
        new("Pivo: Pressao chega a AI00 como raw simulado", async () =>
        {
            using IntegrationContext context = CreateContext("pivo-central", 1);
            context.Engine.SetAnalogInput("Pressao", 7);
            context.Adapter.SyncInputsToDevice();
            RtuInputSnapshot snapshot = await context.Inputs.ReadAsync(1, CancellationToken.None);
            return snapshot.AnalogInputs[0] == 7;
        }),
        new("Pivo: DO00 aciona e desliga Bomba virtual", async () =>
        {
            using IntegrationContext context = CreateContext("pivo-central", 1);
            Task<RtuOutputTestResult> cycle = context.Outputs.ActivateMomentaryAsync(
                1,
                Layout3OutputChannel.DO00,
                TimeSpan.FromMilliseconds(40),
                AuthorizedOutput(),
                CancellationToken.None);
            await Task.Delay(5);
            bool activeObserved = context.Engine.GetValue("Bomba") == 1;
            RtuOutputTestResult result = await cycle;
            return activeObserved
                && result.State == RtuOutputTestState.Completed
                && context.Engine.GetValue("Bomba") == 0;
        }),
        new("Pivo: DO01 e DO02 permanecem mutuamente exclusivos", async () =>
        {
            using IntegrationContext context = CreateContext("pivo-central", 1);
            await context.Outputs.ActivateMomentaryAsync(
                1,
                Layout3OutputChannel.DO01,
                TimeSpan.FromMilliseconds(1),
                AuthorizedOutput(),
                CancellationToken.None);
            await context.Outputs.ActivateMomentaryAsync(
                1,
                Layout3OutputChannel.DO02,
                TimeSpan.FromMilliseconds(1),
                AuthorizedOutput(),
                CancellationToken.None);
            return context.Engine.GetValue("Frente") == 0 && context.Engine.GetValue("Reverso") == 0;
        }),
        new("Pivo: emergencia rejeita efeito de DO00 no processo", async () =>
        {
            using IntegrationContext context = CreateContext("pivo-central", 1);
            context.Engine.SetDigitalInput("Emergencia", true);
            context.Adapter.SyncInputsToDevice();
            await context.Outputs.ActivateMomentaryAsync(
                1,
                Layout3OutputChannel.DO00,
                TimeSpan.FromMilliseconds(1),
                AuthorizedOutput(),
                CancellationToken.None);
            return context.Engine.CurrentState == "Emergencia"
                && context.Engine.GetValue("Bomba") == 0
                && context.Adapter.RejectedOutputCommands == 1;
        }),
        new("Poco: nivel e pressao chegam a AI00/AI01", async () =>
        {
            using IntegrationContext context = CreateContext("poco", 1);
            context.Engine.SetAnalogInput("Nivel", 42);
            context.Engine.SetAnalogInput("Pressao", 6);
            context.Adapter.SyncInputsToDevice();
            RtuInputSnapshot snapshot = await context.Inputs.ReadAsync(1, CancellationToken.None);
            return snapshot.AnalogInputs[0] == 42 && snapshot.AnalogInputs[1] == 6;
        }),
        new("Poco: DO00 controla Bomba virtual", async () =>
        {
            using IntegrationContext context = CreateContext("poco", 1);
            Task<RtuOutputTestResult> cycle = context.Outputs.ActivateMomentaryAsync(
                1,
                Layout3OutputChannel.DO00,
                TimeSpan.FromMilliseconds(40),
                AuthorizedOutput(),
                CancellationToken.None);
            await Task.Delay(5);
            bool pumping = context.Engine.CurrentState == "Bombeando";
            await cycle;
            return pumping && context.Engine.CurrentState == "Parado";
        }),
        new("Poco: emergencia chega a DI04 e bloqueia processo", async () =>
        {
            using IntegrationContext context = CreateContext("poco", 1);
            context.Engine.SetDigitalInput("Emergencia", true);
            context.Adapter.SyncInputsToDevice();
            RtuInputSnapshot snapshot = await context.Inputs.ReadAsync(1, CancellationToken.None);
            return snapshot.DigitalInputs[4]
                && context.Engine.Snapshot.OutputsBlocked
                && context.Engine.CurrentState == "Emergencia";
        }),
        new("mudanca de perfil troca bindings sem reaproveitar aliases", () =>
        {
            using IndustrialPlatformSession pivot = new("pivo-central");
            using IndustrialPlatformSession well = new("poco");
            return Task.FromResult(
                pivot.FindBinding(SimulationIoDirection.Input, SimulationIoType.Digital, 0)?.SignalId
                    == "Emergencia"
                && well.FindBinding(SimulationIoDirection.Input, SimulationIoType.Digital, 0)?.SignalId
                    == "NivelMinimo"
                && pivot.FindBinding(SimulationIoDirection.Output, SimulationIoType.Digital, 1)?.SignalId
                    == "Frente"
                && well.FindBinding(SimulationIoDirection.Output, SimulationIoType.Digital, 1)?.SignalId
                    == "Valvula");
        }),
        new("perfil sem mapeamento falha fechado", () =>
        {
            SimulationProfile profile = SimulationProfileLoader.Load("pivo-central");
            profile.IoBindings.Clear();
            return Task.FromResult(Throws<ArgumentException>(() =>
                SimulationHio115Mappings.FromProfile(profile)));
        }),
        new("sessao default resolve perfil HIO115 executavel", () =>
        {
            using IndustrialPlatformSession session = new("pivo-central");
            return Task.FromResult(
                session.DeviceProfile.Id == NeonHio115DeviceProfile.ProfileId
                && session.DeviceProfile.CanCreateSimulatedSession
                && !session.DeviceProfile.CanCreatePhysicalSession);
        }),
        new("sessao de equipamento unsupported falha fechado", () =>
        {
            IndustrialDeviceProfile unsupported = IndustrialDeviceProfile.CreateUnsupported(
                "UNKNOWN",
                "Equipamento pendente");
            return Task.FromResult(Throws<InvalidOperationException>(() =>
                new IndustrialPlatformSession("pivo-central", unsupported)));
        }),
        new("integracao nao usa porta serial", () => Task.FromResult(
            typeof(SimulationHio115Adapter).AssemblyQualifiedName is not null
            && typeof(SimulationHio115Adapter).GetFields().All(field =>
                !field.FieldType.FullName!.Contains("System.IO.Ports", StringComparison.Ordinal)))),
        new("contadores fisicos permanecem zero", async () =>
        {
            using IntegrationContext context = CreateContext("pivo-central", 1);
            await context.Discovery.DiscoverAsync(1, 1, TimeSpan.Zero, null, CancellationToken.None);
            await context.Inputs.ReadAsync(1, CancellationToken.None);
            return context.RtuCounters.SimulatedConnections > 0
                && context.RtuCounters.SimulatedReads > 0
                && context.RtuCounters.PhysicalConnections == 0
                && context.RtuCounters.PhysicalReads == 0
                && context.RtuCounters.PhysicalWrites == 0
                && context.RtuCounters.PhysicalCommands == 0
                && context.Engine.Counters.PhysicalCommands == 0;
        })
    ];

    private static IntegrationContext CreateContext(string profileId, byte address)
    {
        IndustrialDeviceProfile deviceProfile = NeonHio115DeviceProfile.Current;
        SimulationProfile profile = SimulationProfileLoader.Load(profileId);
        SimulationEngine engine = new(profile);
        NeonHio115FakeDevice device = new(address, profile: deviceProfile);
        SimulationHio115Adapter adapter = new(
            engine,
            device,
            SimulationHio115Mappings.FromProfile(profile, deviceProfile),
            deviceProfile);
        adapter.SyncInputsToDevice();

        IndustrialOperationCounters counters = new();
        InMemoryOperationLog log = new();
        RtuClient client = new(
            new InMemoryRtuTransport([device]),
            counters,
            log,
            TimeSpan.FromMilliseconds(500),
            deviceProfile.LogIdentity);
        RtuEquipmentIdentificationService identification = new(
            client,
            deviceProfile.IdentificationPolicy);
        return new(
            engine,
            device,
            adapter,
            new RtuDiscoveryService(identification, counters),
            new RtuInputTestService(client, deviceProfile.InputMap),
            new RtuSupervisedOutputService(
                client,
                deviceProfile.OutputMap,
                deviceProfile.Limits.MaximumSimulatedOutputDuration),
            counters);
    }

    private static Layout3OutputAuthorization AuthorizedOutput() => new(
        true,
        true,
        true,
        true,
        true,
        PhysicalGateAuthorized: false,
        SimulationOnly: true);

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

    private static void WritePhysicalZeroCounters(TextWriter output)
    {
        output.WriteLine("PhysicalConnections: 0");
        output.WriteLine("PhysicalReads: 0");
        output.WriteLine("PhysicalWrites: 0");
        output.WriteLine("PhysicalCommands: 0");
    }

    private sealed record IntegrationContext(
        SimulationEngine Engine,
        NeonHio115FakeDevice Device,
        SimulationHio115Adapter Adapter,
        RtuDiscoveryService Discovery,
        RtuInputTestService Inputs,
        RtuSupervisedOutputService Outputs,
        IndustrialOperationCounters RtuCounters) : IDisposable
    {
        public void Dispose() => Adapter.Dispose();
    }
}
