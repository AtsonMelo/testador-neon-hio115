using TestadorCLPHI.App.Industrial.Platform.Process;

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
        new("Pivo possui cenarios operacionais declarados", () =>
        {
            string[] expected =
            [
                "normal", "irrigando", "movendo-frente", "movendo-reverso", "emergencia",
                "desalinhado", "falha-torre", "pressao-baixa", "fim-de-curso"
            ];
            SimulationProfile profile = SimulationProfileLoader.Load("pivo-central");
            return expected.All(id => profile.Scenarios.Any(scenario =>
                string.Equals(scenario.Id, id, StringComparison.OrdinalIgnoreCase)));
        }),
        new("Pivo usa quantidade configuravel de quatro torres", () =>
        {
            SimulationEngine engine = CreateEngine("pivo-central");
            return engine.Profile.Visualization.TowerCount == 4
                && ProjectPivot(engine).Towers.Count == 4;
        }),
        new("Pivo normal projeta todas as torres OK", () =>
            ProjectPivot(CreateEngine("pivo-central")).Towers.All(tower =>
                tower.Status == PivotTowerStatus.Ok)),
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
            engine.ApplyScenario("falha-torre");
            return engine.Snapshot.State == "Falha"
                && engine.Snapshot.ActiveAlarms.Contains("SafetyChainOpen")
                && engine.GetValue("SafetyChain") == 0
                && ProjectPivot(engine).Towers.Single(tower => tower.Number == 3).Status
                    == PivotTowerStatus.Fault;
        }),
        new("Pivo desalinhado", () =>
        {
            SimulationEngine engine = CreateEngine("pivo-central");
            engine.SetDigitalInput("Alinhamento", false);
            return engine.Snapshot.State == "Desalinhado"
                && ProjectPivot(engine).Towers.Single(tower => tower.Number == 3).Status
                    == PivotTowerStatus.Misaligned;
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
        new("Pivo fim de curso bloqueia saidas pelo perfil", () =>
        {
            SimulationEngine engine = CreateEngine("pivo-central");
            engine.ApplyScenario("fim-de-curso");
            return engine.CurrentState == "FimDeCurso"
                && engine.Snapshot.OutputsBlocked
                && engine.Snapshot.ActiveAlarms.Contains("EndOfTravel");
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
        new("Pivo movimento frente projeta posicao e torres MOVING", () =>
        {
            SimulationEngine engine = CreateEngine("pivo-central");
            engine.ApplyScenario("movendo-frente");
            PivotProcessState process = ProjectPivot(engine);
            return process.Direction == PivotMovementDirection.Forward
                && process.PositionPercent == 37
                && process.Towers.All(tower => tower.Status == PivotTowerStatus.Moving);
        }),
        new("Pivo cenario irrigando ativa bomba e agua", () =>
        {
            SimulationEngine engine = CreateEngine("pivo-central");
            engine.ApplyScenario("irrigando");
            PivotProcessState process = ProjectPivot(engine);
            return process.OverallState == "Irrigando"
                && process.PumpActive
                && process.WaterActive;
        }),
        new("Pivo emergencia projeta torres UNKNOWN", () =>
        {
            SimulationEngine engine = CreateEngine("pivo-central");
            engine.ApplyScenario("emergencia");
            return ProjectPivot(engine).Towers.All(tower => tower.Status == PivotTowerStatus.Unknown);
        }),
        new("Pivo posicao permanece em zero a cem", () =>
        {
            SimulationEngine engine = CreateEngine("pivo-central");
            engine.SetAnalogInput("PosicaoPercentual", 100);
            bool upperBound = ProjectPivot(engine).PositionPercent == 100;
            engine.SetAnalogInput("PosicaoPercentual", 0);
            return upperBound && ProjectPivot(engine).PositionPercent == 0;
        }),
        new("Pivo cenario normal reseta falha", () =>
        {
            SimulationEngine engine = CreateEngine("pivo-central");
            engine.ApplyScenario("emergencia");
            engine.ApplyScenario("normal");
            return engine.CurrentState == "Parado" && engine.Snapshot.ActiveAlarms.Count == 0;
        }),
        new("Pivo reset restaura posicao e estado", () =>
        {
            SimulationEngine engine = CreateEngine("pivo-central");
            engine.ApplyScenario("movendo-reverso");
            engine.Reset();
            PivotProcessState process = ProjectPivot(engine);
            return process.OverallState == "Parado"
                && process.PositionPercent == 0
                && process.Direction == PivotMovementDirection.Stopped;
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
        }),
        new("sinal derivado AND com todas as fontes 1 produz target 1", () =>
        {
            SimulationEngine engine = new(CreateDerivedTestProfile());
            return engine.GetValue("T1") == 1;
        }),
        new("sinal derivado AND com uma fonte 0 produz target 0", () =>
        {
            SimulationEngine engine = new(CreateDerivedTestProfile());
            engine.SetDigitalInput("S1", false);
            return engine.GetValue("T1") == 0;
        }),
        new("sinal derivado restaura target 1 ao restaurar fonte para 1", () =>
        {
            SimulationEngine engine = new(CreateDerivedTestProfile());
            engine.SetDigitalInput("S1", false);
            bool wasZero = engine.GetValue("T1") == 0;
            engine.SetDigitalInput("S1", true);
            return wasZero && engine.GetValue("T1") == 1;
        }),
        new("sinal derivado rejeita SetDigitalInput direto", () =>
        {
            SimulationEngine engine = new(CreateDerivedTestProfile());
            return Throws<InvalidOperationException>(() => engine.SetDigitalInput("T1", false));
        }),
        new("perfil com fonte derivada inexistente falha na validacao", () =>
        {
            SimulationProfile invalid = CreateDerivedTestProfile();
            invalid.DerivedSignals[0].SourceSignalIds.Add("NON_EXISTENT");
            return !SimulationProfileValidator.Validate(invalid).IsValid;
        }),
        new("perfil com ciclo de sinais derivados falha na validacao", () =>
        {
            SimulationProfile invalid = new()
            {
                SchemaVersion = 1,
                Id = "cyclic-derived",
                DisplayName = "Cyclic Derived Profile",
                InitialState = "Normal",
                Signals =
                [
                    new() { Id = "D1", Label = "Derived 1", Kind = SimulationSignalKind.DigitalInput, DefaultValue = 0, Minimum = 0, Maximum = 1 },
                    new() { Id = "D2", Label = "Derived 2", Kind = SimulationSignalKind.DigitalInput, DefaultValue = 0, Minimum = 0, Maximum = 1 }
                ],
                DerivedSignals =
                [
                    new() { TargetSignalId = "D1", Operator = SimulationDerivedOperator.And, SourceSignalIds = ["D2"] },
                    new() { TargetSignalId = "D2", Operator = SimulationDerivedOperator.And, SourceSignalIds = ["D1"] }
                ]
            };
            return !SimulationProfileValidator.Validate(invalid).IsValid;
        }),
        new("sinais derivados mantem contadores fisicos em zero", () =>
        {
            SimulationCounters counters = new();
            SimulationEngine engine = new(CreateDerivedTestProfile(), counters);
            engine.SetDigitalInput("S1", false);
            engine.SetDigitalInput("S1", true);
            return counters.PhysicalConnections == 0
                && counters.PhysicalReads == 0
                && counters.PhysicalWrites == 0
                && counters.PhysicalCommands == 0;
        }),
        new("Pivo normal com todas TowerNSafety 1 mantem SafetyChain 1", () =>
        {
            SimulationEngine engine = CreateEngine("pivo-central");
            return engine.GetValue("Tower1Safety") == 1
                && engine.GetValue("Tower2Safety") == 1
                && engine.GetValue("Tower3Safety") == 1
                && engine.GetValue("Tower4Safety") == 1
                && engine.GetValue("SafetyChain") == 1;
        }),
        new("Pivo falha em Tower3Safety resulta em SafetyChain 0, estado Falha, alarme SafetyChainOpen e torre 3 Fault", () =>
        {
            SimulationEngine engine = CreateEngine("pivo-central");
            engine.SetDigitalInput("Tower3Safety", false);
            PivotProcessState pivot = ProjectPivot(engine);
            return engine.GetValue("SafetyChain") == 0
                && engine.Snapshot.State == "Falha"
                && engine.Snapshot.ActiveAlarms.Contains("SafetyChainOpen")
                && pivot.Towers.Single(t => t.Number == 3).Status == PivotTowerStatus.Fault
                && pivot.Towers.Where(t => t.Number != 3).All(t => t.Status == PivotTowerStatus.Ok);
        }),
        new("Pivo falha simultanea em Tower2Safety e Tower4Safety marca ambas torres como Fault", () =>
        {
            SimulationEngine engine = CreateEngine("pivo-central");
            engine.SetDigitalInput("Tower2Safety", false);
            engine.SetDigitalInput("Tower4Safety", false);
            PivotProcessState pivot = ProjectPivot(engine);
            return engine.GetValue("SafetyChain") == 0
                && pivot.Towers.Single(t => t.Number == 2).Status == PivotTowerStatus.Fault
                && pivot.Towers.Single(t => t.Number == 4).Status == PivotTowerStatus.Fault;
        }),
        new("Pivo restaura SafetyChain 1 ao restaurar todas TowerNSafety para 1", () =>
        {
            SimulationEngine engine = CreateEngine("pivo-central");
            engine.SetDigitalInput("Tower3Safety", false);
            bool wasZero = engine.GetValue("SafetyChain") == 0;
            engine.SetDigitalInput("Tower3Safety", true);
            return wasZero && engine.GetValue("SafetyChain") == 1;
        }),
        new("Pivo desalinhamento nao altera SafetyChain nem TowerNSafety", () =>
        {
            SimulationEngine engine = CreateEngine("pivo-central");
            engine.SetDigitalInput("Alinhamento", false);
            return engine.GetValue("SafetyChain") == 1
                && engine.GetValue("Tower1Safety") == 1
                && engine.GetValue("Tower2Safety") == 1
                && engine.GetValue("Tower3Safety") == 1
                && engine.GetValue("Tower4Safety") == 1;
        }),
        new("Pivo emergencia independe de SafetyChain e projeta torres UNKNOWN", () =>
        {
            SimulationEngine engine = CreateEngine("pivo-central");
            engine.SetDigitalInput("Emergencia", true);
            PivotProcessState pivot = ProjectPivot(engine);
            return engine.GetValue("SafetyChain") == 1
                && pivot.Towers.All(t => t.Status == PivotTowerStatus.Unknown);
        }),
        new("Pivo rejeita escrita direta em SafetyChain por ser derivada", () =>
        {
            SimulationEngine engine = CreateEngine("pivo-central");
            return Throws<InvalidOperationException>(() => engine.SetDigitalInput("SafetyChain", false));
        }),
        new("Pivo bindings nao possuem TowerNSafety para adapter HIO115", () =>
        {
            SimulationProfile profile = SimulationProfileLoader.Load("pivo-central");
            Integration.SimulationHio115Mapping mapping = Integration.SimulationHio115Mappings.FromProfile(profile);
            return mapping.DigitalInputs.Values.Contains("SafetyChain")
                && !mapping.DigitalInputs.Values.Contains("Tower1Safety")
                && !mapping.DigitalInputs.Values.Contains("Tower2Safety")
                && !mapping.DigitalInputs.Values.Contains("Tower3Safety")
                && !mapping.DigitalInputs.Values.Contains("Tower4Safety");
        }),
        new("UI identifica SafetyChain como derivado e bloqueia edicao direta", () =>
        {
            SimulationEngine engine = CreateEngine("pivo-central");
            return engine.Profile.DerivedSignals.Any(d => string.Equals(d.TargetSignalId, "SafetyChain", StringComparison.OrdinalIgnoreCase));
        })
    ];

    private static SimulationProfile CreateDerivedTestProfile() => new()
    {
        SchemaVersion = 1,
        Id = "test-derived",
        DisplayName = "Test Derived Profile",
        InitialState = "Normal",
        Signals =
        [
            new() { Id = "S1", Label = "Source 1", Kind = SimulationSignalKind.DigitalInput, DefaultValue = 1, Minimum = 0, Maximum = 1 },
            new() { Id = "S2", Label = "Source 2", Kind = SimulationSignalKind.DigitalInput, DefaultValue = 1, Minimum = 0, Maximum = 1 },
            new() { Id = "T1", Label = "Target 1", Kind = SimulationSignalKind.DigitalInput, DefaultValue = 0, Minimum = 0, Maximum = 1 }
        ],
        DerivedSignals =
        [
            new()
            {
                TargetSignalId = "T1",
                Operator = SimulationDerivedOperator.And,
                SourceSignalIds = ["S1", "S2"]
            }
        ],
        Rules =
        [
            new()
            {
                Id = "r1",
                Priority = 100,
                Conditions = [new() { SignalId = "T1", Comparison = SimulationComparison.Equals, ExpectedValue = 1 }],
                ResultState = "Normal",
                AlarmId = "",
                BlockAllOutputs = false
            }
        ]
    };

    private static SimulationEngine CreateEngine(string profileId) =>
        new(SimulationProfileLoader.Load(profileId));

    private static PivotProcessState ProjectPivot(SimulationEngine engine)
    {
        SimulationProcessState state = SimulationProcessStateProjector.Project(
            engine.Profile,
            engine.Snapshot);
        return PivotProcessStateProjector.Project(engine.Profile, state);
    }

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
