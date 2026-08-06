namespace TestadorCLPHI.App.Ui.Industrial.Layout3;

internal static class Layout3BenchReadinessScenarioValidator
{
    private sealed record Scenario(
        string Name,
        bool ExpectedReady,
        Action<Layout3BenchReadinessConfiguration, ISet<string>> Arrange);

    public static int Validate(TextWriter output, TextWriter error)
    {
        ArgumentNullException.ThrowIfNull(output);
        ArgumentNullException.ThrowIfNull(error);

        IReadOnlyList<Scenario> scenarios = BuildScenarios();
        int passed = 0;

        output.WriteLine("Testes locais do validador de prontidao da bancada Layout 3");
        output.WriteLine("Escopo: configuracoes sinteticas + documentos fake em memoria.");
        output.WriteLine("Nenhum endereco sintetico e aberto; nenhum transporte e instanciado.");
        output.WriteLine();

        foreach (Scenario scenario in scenarios)
        {
            Layout3BenchReadinessConfiguration configuration = CreateCompleteConfiguration();
            ISet<string> documents = CreateCompleteDocumentSet();
            scenario.Arrange(configuration, documents);

            Layout3BenchReadinessEvaluationResult result =
                Layout3BenchReadinessEvaluator.Evaluate(configuration, documents);
            bool matchesExpectation = result.IsReady == scenario.ExpectedReady;

            if (matchesExpectation)
            {
                passed++;
                output.WriteLine($"[OK] {scenario.Name}");
                continue;
            }

            output.WriteLine($"[ERRO] {scenario.Name}");
            output.WriteLine(
                $"  Esperado ready={scenario.ExpectedReady}; obtido ready={result.IsReady}.");
            foreach (string failure in result.Failures)
            {
                output.WriteLine($"  - {failure}");
            }
        }

        output.WriteLine();
        output.WriteLine($"Cenarios aprovados: {passed}/{scenarios.Count}");
        WriteZeroCounters(output);

        bool isValid = passed == scenarios.Count;
        output.WriteLine(isValid ? "Resultado: OK" : "Resultado: ERRO");
        if (!isValid)
        {
            error.WriteLine("Testes locais do validador de prontidao falharam.");
        }

        return isValid ? 0 : 1;
    }

    internal static void WriteZeroCounters(TextWriter output)
    {
        output.WriteLine("Conexoes reais: 0");
        output.WriteLine("Leituras reais: 0");
        output.WriteLine("Escritas reais: 0");
        output.WriteLine("Comandos fisicos executados: 0");
    }

    private static IReadOnlyList<Scenario> BuildScenarios()
    {
        return
        [
            new("configuracao completa", true, static (_, _) => { }),
            new("IP ausente", false, static (configuration, _) =>
                configuration.Connection!.EquipmentIpAddress = string.Empty),
            new("protocolo ausente", false, static (configuration, _) =>
                configuration.Connection!.Protocol = string.Empty),
            new("mapa ausente", false, static (configuration, _) =>
                configuration.ReadPolicy!.RegisterMapReference = string.Empty),
            new("allow-list vazia", false, static (configuration, _) =>
                configuration.ReadPolicy!.AllowedRegisters.Clear()),
            new("registrador de escrita", false, static (configuration, _) =>
                configuration.ReadPolicy!.AllowedRegisters[0].Access = "write"),
            new("timeout invalido", false, static (configuration, _) =>
                configuration.ReadPolicy!.TimeoutMilliseconds = 0),
            new("limite de leituras invalido", false, static (configuration, _) =>
                configuration.ReadPolicy!.MaximumReads = 0),
            new("comunicacao habilitada indevidamente", false, static (configuration, _) =>
                configuration.Safety!.RealCommunicationEnabled = true),
            new("documento ausente", false, static (_, documents) =>
                documents.Remove(Layout3BenchReadinessEvaluator.RequiredDocuments[0])),
            new("configuracao conflitante", false, static (configuration, _) =>
            {
                configuration.Connection!.SerialPortName = "COM-FAKE";
                configuration.ApprovalStatuses!["connectionParameters"] = "CONFLITANTE";
            })
        ];
    }

    private static ISet<string> CreateCompleteDocumentSet()
    {
        return new HashSet<string>(
            Layout3BenchReadinessEvaluator.RequiredDocuments,
            StringComparer.OrdinalIgnoreCase);
    }

    private static Layout3BenchReadinessConfiguration CreateCompleteConfiguration()
    {
        return new Layout3BenchReadinessConfiguration
        {
            SchemaVersion = 1,
            Equipment = new Layout3BenchEquipmentConfiguration
            {
                PlcModel = "TEST-PLC",
                ModuleModel = "TEST-MODULE",
                Firmware = "TEST-FIRMWARE",
                LabelPhotoEvidence = "evidence/test-label-photo.txt"
            },
            Connection = new Layout3BenchConnectionConfiguration
            {
                Protocol = "modbus_tcp",
                Transport = "ethernet",
                Topology = "isolated-test-network",
                EquipmentIpAddress = "192.0.2.10",
                PcIpAddress = "192.0.2.20",
                TcpPort = 1502,
                DeviceAddress = 1
            },
            ReadPolicy = new Layout3BenchReadPolicyConfiguration
            {
                RegisterMapReference = "evidence/test-register-map.txt",
                TimeoutMilliseconds = 500,
                MaximumReads = 2,
                AllowedRegisters =
                [
                    new Layout3BenchAllowedRegister
                    {
                        Name = "TEST_STATUS_A",
                        Area = "input_register",
                        Address = 100,
                        Access = "read",
                        ApprovalEvidence = "evidence/test-register-map.txt#status-a"
                    },
                    new Layout3BenchAllowedRegister
                    {
                        Name = "TEST_STATUS_B",
                        Area = "holding_register",
                        Address = 101,
                        Access = "read",
                        ApprovalEvidence = "evidence/test-register-map.txt#status-b"
                    }
                ]
            },
            Safety = new Layout3BenchSafetyConfiguration
            {
                FeatureEnabled = false,
                RealCommunicationEnabled = false,
                WritesEnabled = false,
                PollingEnabled = false,
                AutomaticReconnectEnabled = false,
                SingleShotOnly = true
            },
            Bench = new Layout3BenchConditionsConfiguration
            {
                BackupReference = "evidence/test-backup.txt",
                SupplyVoltage = "TEST-VOLTAGE",
                Responsible = "TEST-RESPONSIBLE",
                MachineState = "TEST-SAFE-STATE",
                GroundingConfirmed = true,
                NetworkIsolated = true,
                OutputsDeenergizedOrIsolated = true,
                MachinePreventedFromOperating = true,
                EmergencyStopIdentified = true,
                QuickDisconnectDefined = true
            },
            ApprovalStatuses = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                ["equipmentIdentity"] = "CONFIRMADO",
                ["moduleIdentity"] = "CONFIRMADO",
                ["protocol"] = "CONFIRMADO",
                ["connectionParameters"] = "CONFIRMADO",
                ["registerMap"] = "CONFIRMADO",
                ["registerAllowList"] = "CONFIRMADO",
                ["electricalSafety"] = "CONFIRMADO",
                ["benchSafety"] = "CONFIRMADO"
            }
        };
    }
}
