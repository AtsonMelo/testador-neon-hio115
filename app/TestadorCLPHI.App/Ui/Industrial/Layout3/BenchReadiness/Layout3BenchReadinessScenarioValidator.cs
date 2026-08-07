namespace TestadorCLPHI.App.Ui.Industrial.Layout3;

internal static class Layout3BenchReadinessScenarioValidator
{
    private sealed record ConfigurationScenario(
        string Name,
        bool ExpectedReady,
        Action<Layout3BenchReadinessConfiguration, ISet<string>> Arrange);

    private sealed record PolicyScenario(string Name, Func<bool> Validate);

    public static int Validate(TextWriter output, TextWriter error)
    {
        ArgumentNullException.ThrowIfNull(output);
        ArgumentNullException.ThrowIfNull(error);

        int passed = 0;
        IReadOnlyList<ConfigurationScenario> configurationScenarios = BuildConfigurationScenarios();
        IReadOnlyList<PolicyScenario> policyScenarios = BuildPolicyScenarios();

        output.WriteLine("SELF-TESTS OFFLINE - LAYOUT 3 BENCH READINESS");
        output.WriteLine("Nenhum transporte, porta COM ou equipamento e acessado.");
        output.WriteLine();

        foreach (ConfigurationScenario scenario in configurationScenarios)
        {
            Layout3BenchReadinessConfiguration configuration = CreateCompleteConfiguration();
            ISet<string> documents = CreateCompleteDocumentSet();
            scenario.Arrange(configuration, documents);
            Layout3BenchReadinessEvaluationResult result =
                Layout3BenchReadinessEvaluator.Evaluate(configuration, documents);
            bool valid = result.IsReady == scenario.ExpectedReady;
            output.WriteLine(valid ? $"[OK] {scenario.Name}" : $"[ERRO] {scenario.Name}");
            if (!valid)
            {
                foreach (string failure in result.Failures)
                {
                    output.WriteLine($"  - {failure}");
                }
            }

            passed += valid ? 1 : 0;
        }

        foreach (PolicyScenario scenario in policyScenarios)
        {
            bool valid = scenario.Validate();
            output.WriteLine(valid ? $"[OK] {scenario.Name}" : $"[ERRO] {scenario.Name}");
            passed += valid ? 1 : 0;
        }

        int total = configurationScenarios.Count + policyScenarios.Count;
        output.WriteLine();
        output.WriteLine($"Cenarios aprovados: {passed}/{total}");
        WriteZeroCounters(output);
        output.WriteLine(passed == total ? "Resultado: OK" : "Resultado: ERRO");
        if (passed != total)
        {
            error.WriteLine("Testes locais do validador de prontidao falharam.");
        }

        return passed == total ? 0 : 1;
    }

    internal static void WriteZeroCounters(TextWriter output)
    {
        output.WriteLine("Conexoes reais: 0");
        output.WriteLine("Leituras reais: 0");
        output.WriteLine("Escritas reais: 0");
        output.WriteLine("Comandos fisicos executados: 0");
    }

    private static IReadOnlyList<ConfigurationScenario> BuildConfigurationScenarios() =>
    [
        new("configuracao RTU completa", true, static (_, _) => { }),
        new("perfil RS485 valido", true, static (configuration, _) =>
        {
            configuration.Connection!.Rtu!.PhysicalLayer = "RS485";
            configuration.Connection.Rtu.ControllerInterface = "ITF-B";
            configuration.Connection.Rtu.ControllerInterfaceExact = "ITF-B";
        }),
        new("COM ausente", false, static (configuration, _) =>
            configuration.Connection!.Rtu!.SerialPortName = string.Empty),
        new("baud invalido", false, static (configuration, _) =>
            configuration.Connection!.Rtu!.BaudRate = 0),
        new("endereco inicial zero", false, static (configuration, _) =>
            configuration.Connection!.Discovery!.StartAddress = 0),
        new("endereco final 248", false, static (configuration, _) =>
            configuration.Connection!.Discovery!.EndAddress = 248),
        new("inicio maior que fim", false, static (configuration, _) =>
        {
            configuration.Connection!.Discovery!.StartAddress = 20;
            configuration.Connection.Discovery.EndAddress = 10;
        }),
        new("timeout invalido", false, static (configuration, _) =>
            configuration.Connection!.Rtu!.TimeoutMilliseconds = 0),
        new("intervalo invalido", false, static (configuration, _) =>
            configuration.Connection!.Rtu!.AttemptIntervalMilliseconds = 0),
        new("protocolo ativo nao confirmado", false, static (configuration, _) =>
            configuration.Connection!.ActiveProtocolConfirmed = false),
        new("identidade fisica conflitante", false, static (configuration, _) =>
        {
            configuration.Equipment!.PhysicalIdentification!.IdentityComparisonStatus = "CONFLITANTE";
            configuration.Equipment.PhysicalIdentification.IdentityRelationshipStatus = "NÃO CONFIRMADA";
        }),
        new("declaracoes sem evidencias", false, static (configuration, _) =>
        {
            configuration.Bench!.ResponsibleDeclarations!.EvidenceStatus = "DECLARADO PELO RESPONSÁVEL";
            configuration.Bench.ResponsibleDeclarations.EvidenceReference = string.Empty;
        }),
        new("mapa ausente", false, static (configuration, _) =>
            configuration.ReadPolicy!.RegisterMapReference = string.Empty),
        new("allow-list de leitura vazia", false, static (configuration, _) =>
            configuration.ReadPolicy!.AllowedRegisters.Clear()),
        new("registrador de leitura fora da allow-list", false, static (configuration, _) =>
            configuration.ReadPolicy!.AllowedRegisters[0].DocumentedReference = 39999),
        new("limite de leituras invalido", false, static (configuration, _) =>
            configuration.ReadPolicy!.MaximumReads = 0),
        new("comunicacao real habilitada", false, static (configuration, _) =>
            configuration.Safety!.RealCommunicationEnabled = true),
        new("documento ausente", false, static (_, documents) =>
            documents.Remove(Layout3BenchReadinessEvaluator.RequiredDocuments[0])),
        new("status conflitante", false, static (configuration, _) =>
            configuration.ApprovalStatuses!["connectionParameters"] = "CONFLITANTE"),
        new("transporte nao RTU rejeitado", false, static (configuration, _) =>
            configuration.Connection!.SelectedTransport = "UNSUPPORTED"),
        new("endereco manual zero", false, static (configuration, _) =>
            configuration.Connection!.Rtu!.ManualAddress = 0),
        new("duas tentativas por endereco", false, static (configuration, _) =>
            configuration.Connection!.Discovery!.MaximumAttemptsPerAddress = 2),
        new("Gate D offline nao autorizado", false, static (configuration, _) =>
            configuration.Safety!.OfflineGateDAuthorized = false),
        new("transporte fisico nao autorizado", false, static (configuration, _) =>
            configuration.Safety!.PhysicalTransportAuthorized = false),
        new("Gate fisico de saida nao autorizado", false, static (configuration, _) =>
            configuration.Safety!.PhysicalOutputGateAuthorized = false),
        new("duracao maxima ausente", false, static (configuration, _) =>
            configuration.OutputPolicy!.MaximumActivationDurationMilliseconds = null),
        new("DO03 ausente", false, static (configuration, _) =>
            configuration.OutputPolicy!.AllowedOutputs.RemoveAt(3)),
        new("PWM nao bloqueado", false, static (configuration, _) =>
            configuration.OutputPolicy!.PwmBlocked = false),
        new("registrador reservado sem bloqueio", false, static (configuration, _) =>
            configuration.OutputPolicy!.BlockedRegisters.RemoveAt(0)),
        new("API generica marcada como exposta", false, static (configuration, _) =>
            configuration.OutputPolicy!.GenericAddressApiExposed = true)
    ];

    private static IReadOnlyList<PolicyScenario> BuildPolicyScenarios()
    {
        Layout3ObservedSignature expected = ExpectedSignature();
        Layout3OutputAuthorization authorized = AuthorizedOutput();
        return
        [
            new("identificacao: equipamento correto", () =>
                Layout3BenchWorkflowPolicy.Classify(expected) == Layout3EquipmentIdentificationState.Identified),
            new("identificacao: ID divergente", () =>
                Layout3BenchWorkflowPolicy.Classify(expected with { ProgramId = 1 }) == Layout3EquipmentIdentificationState.SignatureMismatch),
            new("identificacao: CRC divergente", () =>
                Layout3BenchWorkflowPolicy.Classify(expected with { ProgramCrc = 1 }) == Layout3EquipmentIdentificationState.SignatureMismatch),
            new("identificacao: firmware divergente", () =>
                Layout3BenchWorkflowPolicy.Classify(expected with { FirmwareVersion = "0.0.0" }) == Layout3EquipmentIdentificationState.SignatureMismatch),
            new("identificacao: F21 critico", () =>
                Layout3BenchWorkflowPolicy.Classify(expected with { GeneralFailureStatus = 1 << 10 }) == Layout3EquipmentIdentificationState.CriticalFailure),
            new("identificacao: sem resposta", () =>
                Layout3BenchWorkflowPolicy.Classify(expected with { Responded = false }) == Layout3EquipmentIdentificationState.NoResponse),
            new("identificacao: cancelado", () =>
                Layout3BenchWorkflowPolicy.Classify(expected with { Cancelled = true }) == Layout3EquipmentIdentificationState.Cancelled),
            new("identificacao: respondeu sem assinatura", () =>
                Layout3BenchWorkflowPolicy.Classify(expected with { ProgramId = null }) == Layout3EquipmentIdentificationState.RespondedButNotRecognized),
            new("entradas: DI valida", () =>
                Layout3BenchWorkflowPolicy.IsReadReferenceAllowed(Layout3BenchMode.InputTest, 31120)),
            new("entradas: AI valida", () =>
                Layout3BenchWorkflowPolicy.IsReadReferenceAllowed(Layout3BenchMode.InputTest, 31132)),
            new("entradas: registrador fora da allow-list", () =>
                !Layout3BenchWorkflowPolicy.IsReadReferenceAllowed(Layout3BenchMode.InputTest, 31135)),
            new("saidas: DO00 permitida", () => OutputChannelIsClosed(Layout3OutputChannel.DO00, 31128)),
            new("saidas: DO01 permitida", () => OutputChannelIsClosed(Layout3OutputChannel.DO01, 31129)),
            new("saidas: DO02 permitida", () => OutputChannelIsClosed(Layout3OutputChannel.DO02, 31130)),
            new("saidas: DO03 permitida", () => OutputChannelIsClosed(Layout3OutputChannel.DO03, 31131)),
            new("saidas: reservado 31137 bloqueado", () => !Layout3BenchWorkflowPolicy.IsOutputReferenceAllowed(31137)),
            new("saidas: reservado 31140 bloqueado", () => !Layout3BenchWorkflowPolicy.IsOutputReferenceAllowed(31140)),
            new("saidas: reservado 31143 bloqueado", () => !Layout3BenchWorkflowPolicy.IsOutputReferenceAllowed(31143)),
            new("saidas: PWM 31144 bloqueado", () => !Layout3BenchWorkflowPolicy.IsOutputReferenceAllowed(31144)),
            new("saidas: PWM 31145 bloqueado", () => !Layout3BenchWorkflowPolicy.IsOutputReferenceAllowed(31145)),
            new("saidas: endereco arbitrario bloqueado", () => !Layout3BenchWorkflowPolicy.IsOutputReferenceAllowed(32000)),
            new("saidas: bloqueada durante identificacao", () =>
                !Layout3BenchWorkflowPolicy.CanActivateOutput(Layout3BenchMode.Identification, authorized, null, false, false, 100, 1000)),
            new("saidas: bloqueada durante teste de entradas", () =>
                !Layout3BenchWorkflowPolicy.CanActivateOutput(Layout3BenchMode.InputTest, authorized, null, false, false, 100, 1000)),
            new("saidas: duas simultaneas bloqueadas", () =>
                !Layout3BenchWorkflowPolicy.CanActivateOutput(Layout3BenchMode.SupervisedOutputTest, authorized, Layout3OutputChannel.DO00, false, false, 100, 1000)),
            new("saidas: timeout bloqueia acionamento", () =>
                !Layout3BenchWorkflowPolicy.CanActivateOutput(Layout3BenchMode.SupervisedOutputTest, authorized, null, false, true, 100, 1000)),
            new("saidas: cancelamento bloqueia acionamento", () =>
                !Layout3BenchWorkflowPolicy.CanActivateOutput(Layout3BenchMode.SupervisedOutputTest, authorized, null, true, false, 100, 1000)),
            new("saidas: desligamento exigido ao terminar", () =>
                Layout3BenchWorkflowPolicy.MustTurnOffAfterActivation(true)),
            new("saidas: limite maximo de duracao", () =>
                !Layout3BenchWorkflowPolicy.CanActivateOutput(Layout3BenchMode.SupervisedOutputTest, authorized, null, false, false, 1001, 1000)),
            new("saidas: autorizacao incompleta falha fechada", () =>
                !Layout3BenchWorkflowPolicy.CanEnableSupervisedOutput(authorized with { PhysicalGateAuthorized = false }))
        ];
    }

    private static bool OutputChannelIsClosed(Layout3OutputChannel channel, int expectedReference)
    {
        int reference = Layout3BenchWorkflowPolicy.GetOutputDocumentedReference(channel);
        return reference == expectedReference && Layout3BenchWorkflowPolicy.IsOutputReferenceAllowed(reference);
    }

    private static Layout3ObservedSignature ExpectedSignature() => new(
        true,
        Layout3BenchWorkflowPolicy.ExpectedFirmwareFamily,
        Layout3BenchWorkflowPolicy.ExpectedFirmwareVersion,
        Layout3BenchWorkflowPolicy.ExpectedProgramId,
        Layout3BenchWorkflowPolicy.ExpectedProgramCrc,
        0,
        false);

    private static Layout3OutputAuthorization AuthorizedOutput() => new(true, true, true, true, true, true);

    private static ISet<string> CreateCompleteDocumentSet() => new HashSet<string>(
        Layout3BenchReadinessEvaluator.RequiredDocuments,
        StringComparer.OrdinalIgnoreCase);

    private static Layout3BenchReadinessConfiguration CreateCompleteConfiguration() => new()
    {
        SchemaVersion = 3,
        Equipment = CreateCompleteEquipment(),
        Connection = CreateCompleteConnection(),
        Identification = CreateCompleteIdentification(),
        ReadPolicy = new Layout3BenchReadPolicyConfiguration
        {
            RegisterMapReference = "docs/test-map.md",
            MaximumReads = 14,
            AllowedRegisters = CreateReadAllowList()
        },
        OutputPolicy = CreateCompleteOutputPolicy(),
        Safety = new Layout3BenchSafetyConfiguration
        {
            FeatureEnabled = false,
            RealCommunicationEnabled = false,
            WritesEnabled = false,
            OutputModeEnabled = false,
            PollingEnabled = false,
            AutomaticReconnectEnabled = false,
            SingleShotOnly = true,
            OfflineGateDAuthorized = true,
            PhysicalTransportAuthorized = true,
            PhysicalOutputGateAuthorized = true
        },
        Bench = new Layout3BenchConditionsConfiguration
        {
            BackupReference = "evidence/test-backup.prj",
            BackupSha256 = new string('a', 64),
            SupplyVoltage = "TEST-VOLTAGE",
            MeasuredVoltageEvidence = "evidence/test-voltage.txt",
            Responsible = "TEST-RESPONSIBLE",
            MachineState = "TEST-SAFE",
            GroundingConfirmed = true,
            ChannelIsolated = true,
            OutputsDeenergizedOrIsolated = true,
            MachinePreventedFromOperating = true,
            EmergencyStopIdentified = true,
            QuickDisconnectDefined = true,
            ResponsibleDeclarations = new Layout3ResponsibleDeclarations
            {
                DeclaredBy = "TEST-RESPONSIBLE",
                EvidenceStatus = "CONFIRMADO",
                EvidenceReference = "evidence/test-declarations.txt",
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
            ["identificationMap"] = "CONFIRMADO",
            ["registerMap"] = "CONFIRMADO",
            ["registerAllowList"] = "CONFIRMADO",
            ["outputAllowList"] = "CONFIRMADO",
            ["electricalSafety"] = "CONFIRMADO",
            ["benchSafety"] = "CONFIRMADO"
        }
    };

    private static Layout3BenchEquipmentConfiguration CreateCompleteEquipment() => new()
    {
        PlcModel = "TEST-PLC",
        ControllerCpu = "TEST-CPU",
        CpuSlot = 0,
        MaximumModules = 2,
        DetectedModules = 2,
        AvailableInterfaces = [new() { Name = "TEST-ITF", PhysicalLayer = "RS232-C" }],
        ControllerStatus = new()
        {
            HardwareRevisionDisplayed = 1,
            FirmwareRevisionDisplayed = 1,
            FunctionalStatus = "TEST-OK",
            StartupStatus = "TEST-OK",
            OperationStatus = "TEST-OK",
            IntermittentStatus = "TEST-OK",
            ConfigurationStatus = "TEST-OK"
        },
        ModuleModel = "TEST-MODULE",
        ModuleSlot = 1,
        ModuleStatus = new()
        {
            HardwareRevisionDisplayed = 1,
            FirmwareRevisionDisplayed = 1,
            FunctionalStatus = "TEST-OK",
            DigitalInputs = new() { Count = 8, Range = "DI00-DI07" },
            DigitalOutputs = new() { Count = 4, Range = "DO00-DO03" },
            AnalogInputs = new() { Count = 3, Range = "AI00-AI02" },
            FastCounters = new() { Count = 3, Range = "FCT0-FCT2" },
            Pwm = new() { Count = 1, Range = "PWM00" },
            AnalogInputPresentation = "TEST-ANALOG"
        },
        Firmware = "TEST-CONFIRMED",
        LabelPhotoEvidence = "evidence/test-label.jpg",
        HiStudioVersion = "TEST-HISTUDIO",
        ObservedProgram = new()
        {
            Condition = "TEST-RUNNING",
            Name = "TEST-PROGRAM",
            Version = 1,
            Identifier = 2,
            Crc = 3,
            StartupMode = "TEST-STARTUP"
        },
        LiveDataEvidence = new()
        {
            RemoteEquipmentCondition = "TEST-OFFLINE",
            HardwareBaseCondition = "TEST-NOT-DEFINED",
            DigitalInputStateConfirmed = false,
            DigitalOutputStateConfirmed = false,
            AnalogValuesConfirmed = false,
            CounterValuesConfirmed = false,
            PwmStateConfirmed = false
        },
        PhysicalIdentification = new()
        {
            FrontIdentification = "TEST-FRONT",
            DisplayedManufacturer = "TEST-MANUFACTURER",
            FrontModel = "TEST-MODEL",
            SerialNumber = "TEST-SERIAL",
            PartNumber = "TEST-PART",
            AdditionalIdentification = "TEST-SLOT",
            AdditionalIdentificationAssessment = "TEST-COMPATIBLE",
            NominalSupplyIndication = "TEST-SUPPLY",
            CurrentConnector = "TEST-CONNECTOR",
            Rs485Terminals = "TEST-D+/D-",
            Rs485TerminationSwitchPresent = true,
            ObservationStatus = "OBSERVADO",
            HiStudioIdentity = "TEST-HISTUDIO-IDENTITY",
            PhysicalFrontIdentity = "TEST-FRONT-IDENTITY",
            IdentityComparisonStatus = "CONFIRMADO",
            IdentityRelationshipStatus = "CONFIRMADA"
        }
    };

    private static Layout3BenchConnectionConfiguration CreateCompleteConnection() => new()
    {
        SelectedTransport = "RTU",
        ActiveProtocolConfirmed = true,
        ObservedTransport = "serial",
        Rtu = new()
        {
            Driver = "TEST-DRIVER",
            Channel = "TEST-CHANNEL",
            SerialPortName = "COM-FAKE",
            PhysicalLayer = "RS232",
            ControllerInterface = "ITF-A1_OR_ITF-A2",
            ControllerInterfaceExact = "UNKNOWN",
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
            ManualAddress = 1,
            TimeoutMilliseconds = 500,
            MaximumAttempts = 1,
            AttemptIntervalMilliseconds = 100
        },
        Addressing = new()
        {
            BroadcastAddress = 0,
            StandardMinimum = 1,
            StandardMaximum = 247,
            ReservedMinimum = 248,
            ReservedMaximum = 255,
            HistoricalObservedAddress = 10,
            ReservedAddressesEnabled = false
        },
        Discovery = new()
        {
            Enabled = false,
            ExplicitStartRequired = true,
            StartAddress = 1,
            EndAddress = 247,
            MaximumAttempts = 247,
            MaximumAttemptsPerAddress = 1,
            AttemptIntervalMilliseconds = 100,
            SequentialAscending = true,
            ContinuousRepeatEnabled = false,
            AutomaticReconnectEnabled = false,
            ImmediateCancellationEnabled = true,
            StopOnValidSignature = true,
            FunctionCode = 3,
            WritesAllowed = false,
            CoilsAllowed = false,
            UseHioIoForDiscovery = false
        }
    };

    private static Layout3EquipmentIdentificationConfiguration CreateCompleteIdentification() => new()
    {
        ExpectedFirmwareFamily = Layout3BenchWorkflowPolicy.ExpectedFirmwareFamily,
        ExpectedFirmwareVersion = Layout3BenchWorkflowPolicy.ExpectedFirmwareVersion,
        ExpectedProgramId = Layout3BenchWorkflowPolicy.ExpectedProgramId,
        ExpectedProgramCrc = Layout3BenchWorkflowPolicy.ExpectedProgramCrc,
        ExpectedController = "NEON5-1S",
        ExpectedCpu = "CPU450",
        ExpectedModule = "HIO115",
        ExpectedModuleSlot = 1,
        Registers =
        [
            IdentificationRegister("FIRMWARE_FAMILY", "F10", null, "PENDENTE"),
            IdentificationRegister("FIRMWARE_VERSION", "F11", null, "PENDENTE"),
            IdentificationRegister("PROG_ID", "F12", 30012, "CONFIRMADO"),
            IdentificationRegister("PROG_CRC", "F13", 30013, "CONFIRMADO"),
            IdentificationRegister("DEV_GFAIL_STS", "F21", 30021, "CONFIRMADO")
        ],
        CriticalF21Bits =
        [
            CriticalBit(0, "GFS_BLOCK_FAIL"),
            CriticalBit(1, "GFS_NBLOCK_FAIL"),
            CriticalBit(2, "GFS_FS_FAIL"),
            CriticalBit(3, "GFS_ETH_FAIL"),
            CriticalBit(8, "GFS_INIT_FAIL"),
            CriticalBit(9, "GFS_IDENT_FAIL"),
            CriticalBit(10, "GFS_OPER_FAIL"),
            CriticalBit(11, "GFS_UNMATCH"),
            CriticalBit(12, "GFS_INV_PROG"),
            CriticalBit(13, "GFS_INV_FIRM"),
            CriticalBit(14, "GFS_NVR_FAIL")
        ]
    };

    private static Layout3IdentificationRegisterConfiguration IdentificationRegister(
        string name,
        string symbol,
        int? reference,
        string status) => new()
    {
        Name = name,
        SymbolicReference = symbol,
        DocumentedReference = reference,
        ProtocolDataAddress = null,
        Access = "R",
        Status = status
    };

    private static Layout3F21BitConfiguration CriticalBit(int bit, string name) => new()
    {
        Bit = bit,
        Name = name,
        Critical = true
    };

    private static List<Layout3BenchAllowedRegister> CreateReadAllowList() =>
    [
        ReadRegister("PROG_ID", "F12", 30012, "identification"),
        ReadRegister("PROG_CRC", "F13", 30013, "identification"),
        ReadRegister("DEV_GFAIL_STS", "F21", 30021, "identification"),
        ReadRegister("DI00", "F1120", 31120, "input_test"),
        ReadRegister("DI01", "F1121", 31121, "input_test"),
        ReadRegister("DI02", "F1122", 31122, "input_test"),
        ReadRegister("DI03", "F1123", 31123, "input_test"),
        ReadRegister("DI04", "F1124", 31124, "input_test"),
        ReadRegister("DI05", "F1125", 31125, "input_test"),
        ReadRegister("DI06", "F1126", 31126, "input_test"),
        ReadRegister("DI07", "F1127", 31127, "input_test"),
        ReadRegister("AI00", "F1132", 31132, "input_test"),
        ReadRegister("AI01", "F1133", 31133, "input_test"),
        ReadRegister("AI02", "F1134", 31134, "input_test")
    ];

    private static Layout3BenchAllowedRegister ReadRegister(
        string name,
        string symbol,
        int reference,
        string mode) => new()
    {
        Name = name,
        SymbolicReference = symbol,
        DocumentedReference = reference,
        Mode = mode,
        Access = "R",
        ApprovalEvidence = "docs/test-map.md"
    };

    private static Layout3BenchOutputPolicyConfiguration CreateCompleteOutputPolicy() => new()
    {
        Enabled = false,
        GenericAddressApiExposed = false,
        OneOutputAtATime = true,
        ExplicitCommandRequired = true,
        MomentaryModeRequired = true,
        MaximumActivationDurationMilliseconds = 1000,
        CancellationRequired = true,
        TurnOffAtEndRequired = true,
        ValidateReturnRequired = true,
        PwmBlocked = true,
        ReservedRegistersBlocked = true,
        ArbitraryRegistersBlocked = true,
        AllowedOutputs =
        [
            Output("DO00", "F1128", 31128),
            Output("DO01", "F1129", 31129),
            Output("DO02", "F1130", 31130),
            Output("DO03", "F1131", 31131)
        ],
        BlockedRegisters =
        [
            Blocked("RESERVED_31137", "F1137", 31137, "reserved"),
            Blocked("RESERVED_31140", "F1140", 31140, "reserved"),
            Blocked("RESERVED_31143", "F1143", 31143, "reserved"),
            Blocked("PWM_FREQUENCY", "F1144", 31144, "pwm"),
            Blocked("PWM_DUTY", "F1145", 31145, "pwm")
        ]
    };

    private static Layout3BenchAllowedOutput Output(string channel, string symbol, int reference) => new()
    {
        Channel = channel,
        SymbolicReference = symbol,
        DocumentedReference = reference,
        Access = "R/W",
        ApprovalEvidence = "docs/test-map.md"
    };

    private static Layout3BenchBlockedRegister Blocked(
        string name,
        string symbol,
        int reference,
        string reason) => new()
    {
        Name = name,
        SymbolicReference = symbol,
        DocumentedReference = reference,
        Reason = reason
    };
}
