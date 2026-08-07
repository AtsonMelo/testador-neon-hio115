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
            new("perfil RTU completo", true, static (_, _) => { }),
            new("perfil RTU RS485 completo", true, static (configuration, _) =>
            {
                configuration.Connection!.Rtu!.PhysicalLayer = "RS485";
                configuration.Connection.Rtu.ControllerInterface = "TEST-ITF-B";
            }),
            new("perfil RS232 aceita interface A1 ou A2 nao identificada", true, static (configuration, _) =>
            {
                configuration.Connection!.Rtu!.ControllerInterface = "ITF-A1_OR_ITF-A2";
                configuration.Connection.Rtu.ControllerInterfaceStatus = "NÃO IDENTIFICADA";
            }),
            new("perfil RTU ignora TCP incompleto", true, static (configuration, _) =>
                configuration.Connection!.Tcp = new Layout3ModbusTcpProfileConfiguration()),
            new("perfil TCP completo", true, static (configuration, _) =>
                SelectTcpProfile(configuration)),
            new("perfil TCP ignora RTU incompleto", true, static (configuration, _) =>
            {
                SelectTcpProfile(configuration);
                configuration.Connection!.Rtu = null;
            }),
            new("IP TCP ausente", false, static (configuration, _) =>
            {
                SelectTcpProfile(configuration);
                configuration.Connection!.Tcp!.EquipmentIpAddress = string.Empty;
            }),
            new("porta TCP ausente", false, static (configuration, _) =>
            {
                SelectTcpProfile(configuration);
                configuration.Connection!.Tcp!.TcpPort = null;
            }),
            new("topologia TCP nao isolada", false, static (configuration, _) =>
            {
                SelectTcpProfile(configuration);
                configuration.Connection!.Tcp!.Topology = string.Empty;
            }),
            new("protocolo ativo nao confirmado", false, static (configuration, _) =>
                configuration.Connection!.ActiveProtocolConfirmed = false),
            new("identidade fisica conflitante", false, static (configuration, _) =>
            {
                configuration.Equipment!.PhysicalIdentification!.IdentityComparisonStatus = "CONFLITANTE";
                configuration.Equipment.PhysicalIdentification.IdentityRelationshipStatus = "NÃO CONFIRMADA";
            }),
            new("declaracao do responsavel sem evidencia", false, static (configuration, _) =>
            {
                configuration.Bench!.ResponsibleDeclarations!.EvidenceStatus = "DECLARADO PELO RESPONSÁVEL";
                configuration.Bench.ResponsibleDeclarations.EvidenceReference = string.Empty;
            }),
            new("mapa ausente", false, static (configuration, _) =>
                configuration.ReadPolicy!.RegisterMapReference = string.Empty),
            new("allow-list vazia", false, static (configuration, _) =>
                configuration.ReadPolicy!.AllowedRegisters.Clear()),
            new("registrador de escrita", false, static (configuration, _) =>
                configuration.ReadPolicy!.AllowedRegisters[0].Access = "write"),
            new("timeout RTU invalido", false, static (configuration, _) =>
                configuration.Connection!.Rtu!.TimeoutMilliseconds = 0),
            new("limite de leituras invalido", false, static (configuration, _) =>
                configuration.ReadPolicy!.MaximumReads = 0),
            new("comunicacao habilitada indevidamente", false, static (configuration, _) =>
                configuration.Safety!.RealCommunicationEnabled = true),
            new("documento ausente", false, static (_, documents) =>
                documents.Remove(Layout3BenchReadinessEvaluator.RequiredDocuments[0])),
            new("configuracao conflitante", false, static (configuration, _) =>
                configuration.ApprovalStatuses!["connectionParameters"] = "CONFLITANTE"),
            new("perfil nao suportado", false, static (configuration, _) =>
                configuration.Connection!.SelectedProfile = "TEST-UNSUPPORTED"),
            new("endereco broadcast proibido", false, static (configuration, _) =>
                configuration.Connection!.Rtu!.DeviceAddress = 0),
            new("endereco reservado sem aprovacao avancada", false, static (configuration, _) =>
                configuration.Connection!.Rtu!.DeviceAddress = 248),
            new("endereco reservado com aprovacao avancada", true, static (configuration, _) =>
            {
                configuration.Connection!.Rtu!.DeviceAddress = 248;
                Layout3ReservedAddressAccessConfiguration advanced =
                    configuration.Connection.Addressing!.AdvancedReservedAccess!;
                advanced.Enabled = true;
                advanced.ManualSingleAddress = 248;
                advanced.WarningAcknowledged = true;
                advanced.ExplicitApprovalReference = "TEST-APPROVAL";
            }),
            new("descoberta inclui endereco 255", false, static (configuration, _) =>
            {
                configuration.Connection!.Discovery!.RangeStart = 255;
                configuration.Connection.Discovery.RangeEnd = 255;
                configuration.Connection.Discovery.AddressAllowList = [255];
            }),
            new("descoberta repete tentativa", false, static (configuration, _) =>
                configuration.Connection!.Discovery!.MaximumAttemptsPerAddress = 2),
            new("Gate D nao autorizado", false, static (configuration, _) =>
                configuration.Safety!.GateDAuthorized = false),
            new("descoberta usa IO do HIO115", false, static (configuration, _) =>
                configuration.Connection!.Discovery!.UseHioIoForDiscovery = true)
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
            SchemaVersion = 2,
            Equipment = new Layout3BenchEquipmentConfiguration
            {
                PlcModel = "TEST-PLC",
                ControllerCpu = "TEST-CPU",
                CpuSlot = 0,
                MaximumModules = 2,
                DetectedModules = 2,
                AvailableInterfaces =
                [
                    new Layout3AvailableInterfaceEvidence
                    {
                        Name = "TEST-ITF",
                        PhysicalLayer = "TEST-PHYSICAL-LAYER"
                    }
                ],
                ControllerStatus = new Layout3ControllerStatusEvidence
                {
                    HardwareRevisionDisplayed = 1,
                    FirmwareRevisionDisplayed = 1,
                    FunctionalStatus = "TEST-OPERATIONAL",
                    StartupStatus = "TEST-NO-FAULT",
                    OperationStatus = "TEST-NO-FAULT",
                    IntermittentStatus = "TEST-NO-FAULT",
                    ConfigurationStatus = "TEST-NO-FAULT"
                },
                ModuleModel = "TEST-MODULE",
                ModuleSlot = 1,
                ModuleStatus = new Layout3ModuleStatusEvidence
                {
                    HardwareRevisionDisplayed = 1,
                    FirmwareRevisionDisplayed = 1,
                    FunctionalStatus = "TEST-OPERATIONAL",
                    DigitalInputs = CreateTestChannelGroup("TEST-DI"),
                    DigitalOutputs = CreateTestChannelGroup("TEST-DO"),
                    AnalogInputs = CreateTestChannelGroup("TEST-AI"),
                    FastCounters = CreateTestChannelGroup("TEST-FCT"),
                    Pwm = CreateTestChannelGroup("TEST-PWM"),
                    AnalogInputPresentation = "TEST-ANALOG-PRESENTATION"
                },
                Firmware = "TEST-FIRMWARE",
                LabelPhotoEvidence = "evidence/test-label-photo.txt",
                HiStudioVersion = "TEST-HISTUDIO",
                ObservedProgram = new Layout3ObservedProgramConfiguration
                {
                    Condition = "TEST-RUNNING",
                    Name = "TEST-PROGRAM",
                    Version = 1,
                    Identifier = 2,
                    Crc = 3,
                    StartupMode = "TEST-STARTUP"
                },
                LiveDataEvidence = new Layout3LiveDataEvidence
                {
                    RemoteEquipmentCondition = "TEST-OFFLINE",
                    HardwareBaseCondition = "TEST-NOT-DEFINED",
                    DigitalInputStateConfirmed = false,
                    DigitalOutputStateConfirmed = false,
                    AnalogValuesConfirmed = false,
                    CounterValuesConfirmed = false,
                    PwmStateConfirmed = false
                },
                PhysicalIdentification = new Layout3PhysicalIdentificationEvidence
                {
                    FrontIdentification = "TEST-FRONT",
                    DisplayedManufacturer = "TEST-MANUFACTURER",
                    FrontModel = "TEST-PHYSICAL-MODEL",
                    SerialNumber = "TEST-SERIAL",
                    PartNumber = "TEST-PART",
                    AdditionalIdentification = "TEST-ADDITIONAL",
                    AdditionalIdentificationAssessment = "TEST-COMPATIBLE",
                    NominalSupplyIndication = "TEST-NOMINAL-SUPPLY",
                    CurrentConnector = "TEST-CONNECTOR",
                    Rs485Terminals = "TEST-D+ / TEST-D-",
                    Rs485TerminationSwitchPresent = true,
                    ObservationStatus = "OBSERVADO",
                    HiStudioIdentity = "TEST-HISTUDIO-IDENTITY",
                    PhysicalFrontIdentity = "TEST-PHYSICAL-IDENTITY",
                    IdentityComparisonStatus = "CONFIRMADO",
                    IdentityRelationshipStatus = "CONFIRMADA"
                }
            },
            Connection = new Layout3BenchConnectionConfiguration
            {
                SelectedProfile = "RTU",
                SupportedProfiles = ["RTU", "TCP"],
                ActiveProtocolConfirmed = true,
                ObservedTransport = "TEST-SERIAL",
                Rtu = new Layout3ModbusRtuProfileConfiguration
                {
                    Driver = "TEST-DRIVER",
                    Channel = "TEST-CHANNEL",
                    SerialPortName = "COM-FAKE",
                    PhysicalLayer = "RS232",
                    ControllerInterface = "TEST-ITF-A",
                    ControllerInterfaceStatus = "TEST-IDENTIFIED",
                    PhysicalConnector = "TEST-DB9",
                    BaudRate = 38400,
                    DataBits = 8,
                    Parity = "None",
                    StopBits = "One",
                    InterCharacterTimeoutMilliseconds = 50,
                    TransmissionDelayMilliseconds = 2,
                    CarrierRemovalDelayMilliseconds = 0,
                    MaximumFrameSize = 256,
                    AddressRemappingEnabled = false,
                    DeviceAddress = 10,
                    TimeoutMilliseconds = 500,
                    MaximumAttempts = 1,
                    AttemptIntervalMilliseconds = 250
                },
                Tcp = new Layout3ModbusTcpProfileConfiguration
                {
                    EquipmentIpAddress = "192.0.2.10",
                    TcpPort = 1502,
                    Topology = "isolated",
                    DeviceAddress = 10,
                    TimeoutMilliseconds = 500,
                    MaximumAttempts = 1
                },
                Addressing = new Layout3ModbusAddressPolicyConfiguration
                {
                    RepresentableMinimum = 1,
                    RepresentableMaximum = 255,
                    StandardDiscoveryMinimum = 1,
                    StandardDiscoveryMaximum = 247,
                    ReservedMinimum = 248,
                    ReservedMaximum = 255,
                    BroadcastAddress = 0,
                    NeverAutomaticallyProbeAddress = 255,
                    CurrentKnownAddress = 10,
                    AdvancedReservedAccess = new Layout3ReservedAddressAccessConfiguration
                    {
                        Enabled = false,
                        ManualSingleAddress = null,
                        WarningAcknowledged = false,
                        ExplicitApprovalReference = string.Empty
                    }
                },
                Discovery = new Layout3ModbusDiscoveryConfiguration
                {
                    Enabled = false,
                    ExplicitStartRequired = true,
                    RangeStart = 10,
                    RangeEnd = 10,
                    AddressAllowList = [10],
                    MaximumAttemptsPerAddress = 1,
                    AttemptIntervalMilliseconds = 250,
                    ContinuousRepeatEnabled = false,
                    ImmediateCancellationEnabled = true,
                    FunctionCode = 3,
                    WritesAllowed = false,
                    CoilsAllowed = false,
                    RequireAllIdentificationValues = true,
                    UseHioIoForDiscovery = false,
                    IdentificationCandidates =
                    [
                        new Layout3DiscoveryIdentificationCandidate
                        {
                            Name = "F12",
                            DisplayReference = "30012",
                            ProtocolDataAddress = null,
                            Access = "R",
                            ExpectedValue = 31134
                        },
                        new Layout3DiscoveryIdentificationCandidate
                        {
                            Name = "F13",
                            DisplayReference = "30013",
                            ProtocolDataAddress = null,
                            Access = "R",
                            ExpectedValue = 23248
                        }
                    ]
                }
            },
            ReadPolicy = new Layout3BenchReadPolicyConfiguration
            {
                RegisterMapReference = "evidence/test-register-map.txt",
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
                SingleShotOnly = true,
                GateDAuthorized = true
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
                QuickDisconnectDefined = true,
                ResponsibleDeclarations = new Layout3ResponsibleDeclarations
                {
                    DeclaredBy = "TEST-RESPONSIBLE",
                    EvidenceStatus = "CONFIRMADO",
                    EvidenceReference = "evidence/test-responsible-declarations.txt",
                    ResponsiblePresent = true,
                    GroundingOk = true,
                    OutputsDeenergizedOrIsolatedOk = true,
                    MachinePreventedFromOperatingOk = true,
                    MachineSafeStateOk = true,
                    EmergencyStopOk = true,
                    QuickDisconnectOk = true,
                    ProgramBackupOk = true
                }
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

    private static Layout3ChannelGroupEvidence CreateTestChannelGroup(string range)
    {
        return new Layout3ChannelGroupEvidence
        {
            Count = 1,
            Range = range
        };
    }

    private static void SelectTcpProfile(Layout3BenchReadinessConfiguration configuration)
    {
        configuration.Connection!.SelectedProfile = "TCP";
        configuration.Connection.ObservedTransport = "TEST-ETHERNET";
    }
}
