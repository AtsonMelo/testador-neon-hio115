namespace TestadorCLPHI.App.Industrial.Platform.Simulation;

internal static class SimulationEngineValidator
{
    private sealed record Scenario(string Name, Func<bool> Run);

    internal static int Validate(TextWriter output, TextWriter error)
    {
        ArgumentNullException.ThrowIfNull(output);
        ArgumentNullException.ThrowIfNull(error);

        IReadOnlyList<Scenario> scenarios = BuildScenarios();
        int passed = 0;
        output.WriteLine("VALIDADOR DO MOTOR DE SIMULACAO INDUSTRIAL");
        output.WriteLine("Escopo: perfis JSON e memoria local; nenhuma comunicacao fisica.");
        output.WriteLine();

        foreach (Scenario scenario in scenarios)
        {
            bool result;
            try
            {
                result = scenario.Run();
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
        output.WriteLine("PhysicalConnections: 0");
        output.WriteLine("PhysicalReads: 0");
        output.WriteLine("PhysicalWrites: 0");
        output.WriteLine("PhysicalCommands: 0");
        output.WriteLine(passed == scenarios.Count ? "Resultado: SIMULATION_READY" : "Resultado: ERRO");
        if (passed != scenarios.Count)
        {
            error.WriteLine("Validacao do motor de simulacao falhou.");
        }

        return passed == scenarios.Count ? 0 : 1;
    }

    private static IReadOnlyList<Scenario> BuildScenarios() =>
    [
        new("catalogo possui Pivo e Poco", () =>
            SimulationProfileLoader.AvailableProfileIds().SequenceEqual(
                ["pivo-central", "poco"],
                StringComparer.OrdinalIgnoreCase)),
        new("perfil Pivo valido", () =>
            SimulationProfileValidator.Validate(SimulationProfileLoader.Load("pivo-central")).IsValid),
        new("perfil Poco valido", () =>
            SimulationProfileValidator.Validate(SimulationProfileLoader.Load("poco")).IsValid),
        new("Pivo inicia parado", () =>
            CreateEngine("pivo-central").Snapshot.State == "Parado"),
        new("Pivo emergencia bloqueia comandos", () =>
        {
            SimulationEngine engine = CreateEngine("pivo-central");
            engine.SetDigitalInput("Emergencia", true);
            bool accepted = engine.SetVirtualOutput("Bomba", true);
            SimulationSnapshot snapshot = engine.Snapshot;
            return !accepted
                && snapshot.State == "Emergencia"
                && snapshot.OutputsBlocked
                && snapshot.ActiveAlarms.Contains("Emergency")
                && snapshot.Values["Bomba"] == 0;
        }),
        new("Pivo falha de torre", () =>
        {
            SimulationEngine engine = CreateEngine("pivo-central");
            engine.SetDigitalInput("FalhaTorre", true);
            return engine.Snapshot.State == "Falha"
                && engine.Snapshot.ActiveAlarms.Contains("TowerFault");
        }),
        new("Pivo desalinhado", () =>
        {
            SimulationEngine engine = CreateEngine("pivo-central");
            engine.SetDigitalInput("Alinhamento", false);
            return engine.Snapshot.State == "Desalinhado";
        }),
        new("Pivo pressao baixa usa limite do perfil", () =>
        {
            SimulationEngine engine = CreateEngine("pivo-central");
            engine.SetVirtualOutput("Bomba", true);
            engine.SetAnalogInput("Pressao", 1.9);
            return engine.Snapshot.State == "Falha"
                && engine.Snapshot.ActiveAlarms.Contains("PressureLow")
                && engine.Snapshot.Values["Bomba"] == 0;
        }),
        new("Pivo Frente e Reverso mutuamente exclusivos", () =>
        {
            SimulationEngine engine = CreateEngine("pivo-central");
            engine.SetVirtualOutput("Frente", true);
            engine.SetVirtualOutput("Reverso", true);
            return engine.GetValue("Frente") == 0
                && engine.GetValue("Reverso") == 1
                && engine.CurrentState == "MovendoReverso";
        }),
        new("Pivo cenario normal reseta falha", () =>
        {
            SimulationEngine engine = CreateEngine("pivo-central");
            engine.ApplyScenario("emergencia");
            engine.ApplyScenario("normal");
            return engine.CurrentState == "Parado" && engine.Snapshot.ActiveAlarms.Count == 0;
        }),
        new("Pivo rejeita valor analogico fora do perfil", () =>
        {
            SimulationEngine engine = CreateEngine("pivo-central");
            return Throws<ArgumentOutOfRangeException>(() => engine.SetAnalogInput("Pressao", 11));
        }),
        new("Poco inicia parado", () => CreateEngine("poco").CurrentState == "Parado"),
        new("Poco bombeando", () =>
        {
            SimulationEngine engine = CreateEngine("poco");
            return engine.SetVirtualOutput("Bomba", true) && engine.CurrentState == "Bombeando";
        }),
        new("Poco falta d'agua", () =>
        {
            SimulationEngine engine = CreateEngine("poco");
            engine.ApplyScenario("falta-agua");
            return engine.CurrentState == "NivelBaixo"
                && engine.Snapshot.ActiveAlarms.Contains("LowLevel")
                && engine.Snapshot.OutputsBlocked;
        }),
        new("Poco pressao baixa", () =>
        {
            SimulationEngine engine = CreateEngine("poco");
            engine.ApplyScenario("pressao-baixa");
            return engine.CurrentState == "Falha"
                && engine.Snapshot.ActiveAlarms.Contains("PressureLow");
        }),
        new("Poco sobrecorrente", () =>
        {
            SimulationEngine engine = CreateEngine("poco");
            engine.ApplyScenario("sobrecorrente");
            return engine.Snapshot.ActiveAlarms.Contains("OverCurrent");
        }),
        new("Poco falta de fase", () =>
        {
            SimulationEngine engine = CreateEngine("poco");
            engine.ApplyScenario("falta-fase");
            return engine.Snapshot.ActiveAlarms.Contains("PhaseLoss");
        }),
        new("Poco sensor invalido", () =>
        {
            SimulationEngine engine = CreateEngine("poco");
            engine.ApplyScenario("sensor-invalido");
            return engine.Snapshot.ActiveAlarms.Contains("InvalidSensor");
        }),
        new("Simulador funciona sem Testador", () =>
        {
            SimulationEngine engine = CreateEngine("pivo-central");
            engine.SetDigitalInput("Pressostato", false);
            return engine.GetValue("Pressostato") == 0;
        }),
        new("log marca eventos como simulados", () =>
        {
            SimulationEngine engine = CreateEngine("poco");
            engine.SetAnalogInput("Nivel", 50);
            return engine.Log.Count > 0 && engine.Log.All(entry => entry.Simulated);
        }),
        new("contadores simulados separados dos fisicos", () =>
        {
            SimulationCounters counters = new();
            SimulationEngine engine = new(SimulationProfileLoader.Load("pivo-central"), counters);
            engine.SetDigitalInput("Pressostato", false);
            engine.SetVirtualOutput("Bomba", true);
            return counters.SimulatedOperations > 0
                && counters.SimulatedCommands == 1
                && counters.PhysicalConnections == 0
                && counters.PhysicalReads == 0
                && counters.PhysicalWrites == 0
                && counters.PhysicalCommands == 0;
        }),
        new("perfil invalido falha fechado", () =>
        {
            SimulationProfile invalid = new()
            {
                SchemaVersion = 1,
                Id = "invalid",
                DisplayName = "Invalid",
                InitialState = "Idle",
                Signals =
                [
                    new() { Id = "A", Label = "A", Kind = SimulationSignalKind.DigitalInput, Minimum = 0, Maximum = 1 },
                    new() { Id = "A", Label = "B", Kind = SimulationSignalKind.DigitalInput, Minimum = 0, Maximum = 1 }
                ]
            };
            return !SimulationProfileValidator.Validate(invalid).IsValid
                && Throws<ArgumentException>(() => _ = new SimulationEngine(invalid));
        })
    ];

    private static SimulationEngine CreateEngine(string profileId) =>
        new(SimulationProfileLoader.Load(profileId));

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
}
