using TestadorCLPHI.App.Industrial.Platform.Safety;

namespace TestadorCLPHI.App.Ui.Industrial.Layout3;

internal static class Layout3BenchReadinessEvaluator
{
    public static readonly IReadOnlyList<string> RequiredDocuments =
    [
        "docs/layout-3-fase-3-9-pre-bancada-clp.md",
        "docs/layout-3-checklist-bancada-readonly.md",
        "docs/layout-3-plano-rollback-bancada.md",
        "docs/layout-3-matriz-riscos-bancada.md",
        "docs/layout-3-ficha-parametros-conexao.md",
        "docs/layout-3-plano-evidencias-teste.md",
        "docs/layout-3-arquitetura-transporte-readonly-planejada.md"
    ];

    private static readonly string[] RequiredApprovalStatuses =
    [
        "equipmentIdentity",
        "moduleIdentity",
        "protocol",
        "connectionParameters",
        "identificationMap",
        "registerMap",
        "registerAllowList",
        "outputAllowList",
        "electricalSafety",
        "benchSafety"
    ];

    private static readonly string[] AllowedPhysicalLayers = ["RS232", "RS485"];
    private static readonly string[] AllowedParityValues = ["None", "Even", "Odd", "Mark", "Space"];
    private static readonly string[] AllowedStopBitsValues = ["One", "OnePointFive", "Two"];

    private static readonly (string Name, string Symbol, int Reference)[] RequiredReadRegisters =
    [
        ("PROG_ID", "F12", 30012),
        ("PROG_CRC", "F13", 30013),
        ("DEV_GFAIL_STS", "F21", 30021),
        ("DI00", "F1120", 31120),
        ("DI01", "F1121", 31121),
        ("DI02", "F1122", 31122),
        ("DI03", "F1123", 31123),
        ("DI04", "F1124", 31124),
        ("DI05", "F1125", 31125),
        ("DI06", "F1126", 31126),
        ("DI07", "F1127", 31127),
        ("AI00", "F1132", 31132),
        ("AI01", "F1133", 31133),
        ("AI02", "F1134", 31134)
    ];

    private static readonly (string Channel, string Symbol, int Reference)[] RequiredOutputs =
    [
        ("DO00", "F1128", 31128),
        ("DO01", "F1129", 31129),
        ("DO02", "F1130", 31130),
        ("DO03", "F1131", 31131)
    ];

    private static readonly (string Symbol, int Reference)[] RequiredBlockedRegisters =
    [
        ("F1137", 31137),
        ("F1140", 31140),
        ("F1143", 31143),
        ("F1144", 31144),
        ("F1145", 31145)
    ];

    private static readonly (int Bit, string Name)[] RequiredCriticalF21Bits =
    [
        (0, "GFS_BLOCK_FAIL"),
        (1, "GFS_NBLOCK_FAIL"),
        (2, "GFS_FS_FAIL"),
        (3, "GFS_ETH_FAIL"),
        (8, "GFS_INIT_FAIL"),
        (9, "GFS_IDENT_FAIL"),
        (10, "GFS_OPER_FAIL"),
        (11, "GFS_UNMATCH"),
        (12, "GFS_INV_PROG"),
        (13, "GFS_INV_FIRM"),
        (14, "GFS_NVR_FAIL")
    ];

    public static Layout3BenchReadinessEvaluationResult Evaluate(
        Layout3BenchReadinessConfiguration configuration,
        ISet<string> availableDocuments)
    {
        ArgumentNullException.ThrowIfNull(configuration);
        ArgumentNullException.ThrowIfNull(availableDocuments);

        List<string> failures = [];
        if (configuration.SchemaVersion != 3)
        {
            failures.Add($"schemaVersion deve ser 3; valor atual: {configuration.SchemaVersion}.");
        }

        ValidateEquipment(configuration.Equipment, failures);
        ValidateConnection(configuration.Connection, failures);
        ValidateIdentification(configuration.Identification, failures);
        ValidateReadPolicy(configuration.ReadPolicy, failures);
        ValidateOutputPolicy(configuration.OutputPolicy, failures);
        ValidateSafety(configuration.Safety, failures);
        ValidateBench(configuration.Bench, failures);
        ValidateApprovals(configuration.ApprovalStatuses, failures);
        ValidateDocuments(availableDocuments, failures);
        return new Layout3BenchReadinessEvaluationResult(failures);
    }

    private static void ValidateConnection(
        Layout3BenchConnectionConfiguration? connection,
        ICollection<string> failures)
    {
        if (connection is null)
        {
            failures.Add("Secao connection ausente.");
            return;
        }

        if (!string.Equals(connection.SelectedTransport, "RTU", StringComparison.OrdinalIgnoreCase))
        {
            failures.Add("Transporte selecionado deve ser exclusivamente RTU nesta fase.");
        }

        RequireText(connection.ObservedTransport, "Transporte observado ausente.", failures);
        if (connection.ActiveProtocolConfirmed is not true)
        {
            failures.Add("Protocolo Modbus RTU realmente ativado no canal ainda nao foi confirmado.");
        }

        ValidateRtu(connection.Rtu, failures);
        ValidateAddressing(connection.Addressing, failures);
        ValidateDiscovery(connection.Discovery, failures);
    }

    private static void ValidateRtu(Layout3ModbusRtuProfileConfiguration? profile, ICollection<string> failures)
    {
        if (profile is null)
        {
            failures.Add("Perfil RTU ausente.");
            return;
        }

        RequireText(profile.SerialPortName, "Porta COM ausente no perfil RTU.", failures);
        if (!AllowedPhysicalLayers.Contains(profile.PhysicalLayer, StringComparer.OrdinalIgnoreCase))
        {
            failures.Add("Camada fisica RTU deve ser RS232 ou RS485.");
        }

        RequireText(profile.ControllerInterface, "Interface do controlador ausente.", failures);
        RequireText(profile.ControllerInterfaceExact, "Estado da interface exata ausente.", failures);
        RequireText(profile.PhysicalConnector, "Conector fisico ausente.", failures);
        if (profile.BaudRate is null or <= 0)
        {
            failures.Add("Baud rate ausente ou invalido.");
        }

        if (profile.DataBits is null or < 5 or > 8)
        {
            failures.Add("Data bits deve permanecer em 5..8.");
        }

        if (!AllowedParityValues.Contains(profile.Parity, StringComparer.OrdinalIgnoreCase))
        {
            failures.Add("Paridade RTU ausente ou invalida.");
        }

        if (!AllowedStopBitsValues.Contains(profile.StopBits, StringComparer.OrdinalIgnoreCase))
        {
            failures.Add("Stop bits RTU ausente ou invalido.");
        }

        if (profile.ManualAddress is null or < 1 or > 247)
        {
            failures.Add("Endereco manual RTU deve permanecer em 1..247.");
        }

        if (profile.TimeoutMilliseconds is null or < 100 or > 5000)
        {
            failures.Add("Timeout RTU deve permanecer em 100..5000 ms.");
        }

        if (profile.MaximumAttempts != 1)
        {
            failures.Add("Perfil RTU deve limitar o maximo de tentativas por operacao a 1.");
        }

        if (profile.AttemptIntervalMilliseconds is null or <= 0)
        {
            failures.Add("Intervalo entre tentativas RTU ausente ou invalido.");
        }
    }

    private static void ValidateAddressing(
        Layout3ModbusAddressPolicyConfiguration? policy,
        ICollection<string> failures)
    {
        if (policy is null)
        {
            failures.Add("Politica de enderecos ausente.");
            return;
        }

        if (policy.BroadcastAddress != 0
            || policy.StandardMinimum != 1
            || policy.StandardMaximum != 247
            || policy.ReservedMinimum != 248
            || policy.ReservedMaximum != 255)
        {
            failures.Add("Politica de endereco deve proibir 0, permitir 1..247 e reservar 248..255.");
        }

        if (policy.HistoricalObservedAddress != 10)
        {
            failures.Add("Endereco 10 deve permanecer apenas como evidencia historica observada.");
        }

        RequireFalse(policy.ReservedAddressesEnabled, "Enderecos 248..255 devem permanecer bloqueados.", failures);
    }

    private static void ValidateDiscovery(
        Layout3ModbusDiscoveryConfiguration? discovery,
        ICollection<string> failures)
    {
        if (discovery is null)
        {
            failures.Add("Desenho offline da descoberta ausente.");
            return;
        }

        RequireFalse(discovery.Enabled, "Descoberta deve permanecer desligada por padrao.", failures);
        RequireTrue(discovery.ExplicitStartRequired, "Descoberta deve exigir comando explicito.", failures);
        RequireTrue(discovery.SequentialAscending, "Descoberta deve seguir ordem crescente.", failures);
        RequireFalse(discovery.ContinuousRepeatEnabled, "Descoberta nao pode repetir continuamente.", failures);
        RequireFalse(discovery.AutomaticReconnectEnabled, "Descoberta nao pode reconectar automaticamente.", failures);
        RequireTrue(discovery.ImmediateCancellationEnabled, "Descoberta deve permitir cancelamento imediato.", failures);
        RequireTrue(discovery.StopOnValidSignature, "Descoberta deve parar ao encontrar assinatura valida.", failures);
        RequireFalse(discovery.WritesAllowed, "Descoberta nao pode permitir escrita.", failures);
        RequireFalse(discovery.CoilsAllowed, "Descoberta nao pode permitir coils.", failures);
        RequireFalse(discovery.UseHioIoForDiscovery, "I/O do HIO115 nao pode identificar endereco.", failures);

        if (discovery.StartAddress is null or < 1
            || discovery.EndAddress is null or > 247
            || discovery.StartAddress > discovery.EndAddress)
        {
            failures.Add("Faixa de descoberta deve ser crescente e permanecer em 1..247.");
        }

        if (discovery.MaximumAttemptsPerAddress != 1)
        {
            failures.Add("Descoberta deve executar no maximo uma tentativa por endereco.");
        }

        int? expectedMaximum = discovery.StartAddress is null || discovery.EndAddress is null
            ? null
            : discovery.EndAddress - discovery.StartAddress + 1;
        if (discovery.MaximumAttempts is null or <= 0
            || expectedMaximum is not null && discovery.MaximumAttempts > expectedMaximum)
        {
            failures.Add("Maximo de tentativas excede a faixa de descoberta.");
        }

        if (discovery.AttemptIntervalMilliseconds is null or <= 0)
        {
            failures.Add("Intervalo da descoberta ausente ou invalido.");
        }

        if (discovery.FunctionCode != 3)
        {
            failures.Add("Identificacao planejada deve usar somente FC03.");
        }
    }

    private static void ValidateIdentification(
        Layout3EquipmentIdentificationConfiguration? identification,
        ICollection<string> failures)
    {
        if (identification is null)
        {
            failures.Add("Plano de identificacao ausente.");
            return;
        }

        if (!string.Equals(identification.ExpectedFirmwareFamily, Layout3BenchWorkflowPolicy.ExpectedFirmwareFamily, StringComparison.Ordinal)
            || !string.Equals(identification.ExpectedFirmwareVersion, Layout3BenchWorkflowPolicy.ExpectedFirmwareVersion, StringComparison.Ordinal)
            || identification.ExpectedProgramId != Layout3BenchWorkflowPolicy.ExpectedProgramId
            || identification.ExpectedProgramCrc != Layout3BenchWorkflowPolicy.ExpectedProgramCrc)
        {
            failures.Add("Assinatura esperada de firmware/programa esta divergente.");
        }

        RequireText(identification.ExpectedController, "Controlador esperado ausente.", failures);
        RequireText(identification.ExpectedCpu, "CPU esperada ausente.", failures);
        RequireText(identification.ExpectedModule, "Modulo esperado ausente.", failures);
        if (identification.ExpectedModuleSlot is null or < 0)
        {
            failures.Add("Slot esperado do modulo ausente ou invalido.");
        }

        ValidateIdentificationRegister(identification.Registers, "F10", null, false, failures);
        ValidateIdentificationRegister(identification.Registers, "F11", null, false, failures);
        ValidateIdentificationRegister(identification.Registers, "F12", 30012, true, failures);
        ValidateIdentificationRegister(identification.Registers, "F13", 30013, true, failures);
        ValidateIdentificationRegister(identification.Registers, "F21", 30021, true, failures);

        if (identification.CriticalF21Bits.Count != RequiredCriticalF21Bits.Length)
        {
            failures.Add("Classificacao critica de F21 deve conter exatamente os 11 bits documentados.");
        }

        foreach ((int bit, string name) in RequiredCriticalF21Bits)
        {
            Layout3F21BitConfiguration? item = identification.CriticalF21Bits.FirstOrDefault(entry => entry.Bit == bit);
            if (item is null || !string.Equals(item.Name, name, StringComparison.Ordinal) || item.Critical is not true)
            {
                failures.Add($"Bit F21 {bit}/{name} deve estar classificado como critico.");
            }
        }
    }

    private static void ValidateIdentificationRegister(
        IReadOnlyList<Layout3IdentificationRegisterConfiguration> registers,
        string symbolicReference,
        int? documentedReference,
        bool confirmed,
        ICollection<string> failures)
    {
        Layout3IdentificationRegisterConfiguration? register = registers.FirstOrDefault(
            item => string.Equals(item.SymbolicReference, symbolicReference, StringComparison.OrdinalIgnoreCase));
        if (register is null
            || !string.Equals(register.Access, "R", StringComparison.OrdinalIgnoreCase)
            || register.DocumentedReference != documentedReference)
        {
            failures.Add($"Referencia de identificacao {symbolicReference} ausente ou invalida.");
            return;
        }

        string expectedStatus = confirmed ? "CONFIRMADO" : "PENDENTE";
        if (!string.Equals(register.Status, expectedStatus, StringComparison.OrdinalIgnoreCase))
        {
            failures.Add($"Status de {symbolicReference} deve ser {expectedStatus}.");
        }

        if (register.ProtocolDataAddress is not null)
        {
            failures.Add($"Endereco PDU de {symbolicReference} deve permanecer vazio ate o Gate D.");
        }
    }

    private static void ValidateReadPolicy(
        Layout3BenchReadPolicyConfiguration? policy,
        ICollection<string> failures)
    {
        if (policy is null)
        {
            failures.Add("Politica de leitura ausente.");
            return;
        }

        RequireText(policy.RegisterMapReference, "Referencia documental do mapa ausente.", failures);
        if (policy.MaximumReads != RequiredReadRegisters.Length)
        {
            failures.Add($"Limite maximo de leituras deve ser {RequiredReadRegisters.Length}.");
        }

        if (policy.AllowedRegisters.Count != RequiredReadRegisters.Length)
        {
            failures.Add("Allow-list de leitura deve conter exatamente identificacao, DI00..DI07 e AI00..AI02.");
        }

        foreach ((string name, string symbol, int reference) in RequiredReadRegisters)
        {
            Layout3BenchAllowedRegister? item = policy.AllowedRegisters.FirstOrDefault(
                entry => entry.DocumentedReference == reference);
            if (item is null
                || !string.Equals(item.Name, name, StringComparison.Ordinal)
                || !string.Equals(item.SymbolicReference, symbol, StringComparison.Ordinal)
                || !string.Equals(item.Access, "R", StringComparison.OrdinalIgnoreCase)
                || string.IsNullOrWhiteSpace(item.ApprovalEvidence))
            {
                failures.Add($"Allow-list de leitura invalida para {name}/{symbol}/{reference}.");
            }
        }

        if (policy.AllowedRegisters.Select(item => item.DocumentedReference).Distinct().Count()
            != policy.AllowedRegisters.Count)
        {
            failures.Add("Allow-list de leitura possui referencias duplicadas.");
        }
    }

    private static void ValidateOutputPolicy(
        Layout3BenchOutputPolicyConfiguration? policy,
        ICollection<string> failures)
    {
        if (policy is null)
        {
            failures.Add("Politica supervisionada de saidas ausente.");
            return;
        }

        RequireFalse(policy.Enabled, "Modo de saidas deve permanecer desabilitado nesta fase.", failures);
        RequireFalse(policy.GenericAddressApiExposed, "API generica por endereco nao pode ser exposta.", failures);
        RequireTrue(policy.OneOutputAtATime, "Politica deve limitar a uma saida por vez.", failures);
        RequireTrue(policy.ExplicitCommandRequired, "Cada saida deve exigir comando explicito.", failures);
        RequireTrue(policy.MomentaryModeRequired, "Modo momentaneo deve ser obrigatorio.", failures);
        RequireTrue(policy.CancellationRequired, "Cancelamento deve ser obrigatorio.", failures);
        RequireTrue(policy.TurnOffAtEndRequired, "Desligamento ao final deve ser obrigatorio.", failures);
        RequireTrue(policy.ValidateReturnRequired, "Retorno apos desligamento deve ser validado.", failures);
        RequireTrue(policy.PwmBlocked, "PWM deve permanecer bloqueado.", failures);
        RequireTrue(policy.ReservedRegistersBlocked, "Registradores reservados devem permanecer bloqueados.", failures);
        RequireTrue(policy.ArbitraryRegistersBlocked, "Registradores arbitrarios devem permanecer bloqueados.", failures);

        if (policy.MaximumActivationDurationMilliseconds is null or <= 0)
        {
            failures.Add("Duracao maxima de acionamento ainda nao foi definida.");
        }

        if (policy.AllowedOutputs.Count != RequiredOutputs.Length)
        {
            failures.Add("Allow-list de saida deve conter exatamente DO00..DO03.");
        }

        foreach ((string channel, string symbol, int reference) in RequiredOutputs)
        {
            Layout3BenchAllowedOutput? item = policy.AllowedOutputs.FirstOrDefault(
                entry => entry.DocumentedReference == reference);
            if (item is null
                || !string.Equals(item.Channel, channel, StringComparison.Ordinal)
                || !string.Equals(item.SymbolicReference, symbol, StringComparison.Ordinal)
                || !string.Equals(item.Access, "R/W", StringComparison.OrdinalIgnoreCase)
                || string.IsNullOrWhiteSpace(item.ApprovalEvidence))
            {
                failures.Add($"Allow-list de saida invalida para {channel}/{symbol}/{reference}.");
            }
        }

        foreach ((string symbol, int reference) in RequiredBlockedRegisters)
        {
            Layout3BenchBlockedRegister? item = policy.BlockedRegisters.FirstOrDefault(
                entry => entry.DocumentedReference == reference);
            if (item is null
                || !string.Equals(item.SymbolicReference, symbol, StringComparison.Ordinal)
                || string.IsNullOrWhiteSpace(item.Reason))
            {
                failures.Add($"Bloqueio ausente para {symbol}/{reference}.");
            }
        }
    }

    private static void ValidateSafety(Layout3BenchSafetyConfiguration? safety, ICollection<string> failures)
    {
        if (safety is null)
        {
            failures.Add("Secao safety ausente.");
            return;
        }

        RequireFalse(safety.FeatureEnabled, "Feature deve permanecer OFF.", failures);
        RequireFalse(safety.RealCommunicationEnabled, "Comunicacao real deve permanecer OFF.", failures);
        RequireFalse(safety.WritesEnabled, "Escrita deve permanecer OFF nesta fase.", failures);
        RequireFalse(safety.OutputModeEnabled, "Modo de saida deve permanecer OFF nesta fase.", failures);
        RequireFalse(safety.PollingEnabled, "Polling deve permanecer OFF.", failures);
        RequireFalse(safety.AutomaticReconnectEnabled, "Reconexao automatica deve permanecer OFF.", failures);
        RequireTrue(safety.SingleShotOnly, "Politica single-shot deve permanecer ON.", failures);
        if (safety.OfflineGateDAuthorized is not true)
        {
            failures.Add("Gate D offline para transporte RTU ainda nao autorizado.");
        }

        if (safety.PhysicalTransportAuthorized is not true)
        {
            failures.Add("Transporte fisico ainda nao autorizado.");
        }

        if (safety.PhysicalOutputGateAuthorized is not true)
        {
            failures.Add("Gate fisico especifico de saidas ainda nao autorizado.");
        }
    }

    private static void ValidateBench(Layout3BenchConditionsConfiguration? bench, ICollection<string> failures)
    {
        if (bench is null)
        {
            failures.Add("Secao bench ausente.");
            return;
        }

        RequireText(bench.BackupReference, "Caminho/nome do backup ausente.", failures);
        RequireText(bench.BackupSha256, "Hash SHA-256 do backup ausente.", failures);
        RequireText(bench.SupplyVoltage, "Tensao de alimentacao confirmada ausente.", failures);
        RequireText(bench.MeasuredVoltageEvidence, "Evidencia de tensao medida ausente.", failures);
        RequireText(bench.Responsible, "Responsavel da bancada ausente.", failures);
        RequireText(bench.MachineState, "Estado seguro da maquina ausente.", failures);
        RequireTrue(bench.GroundingConfirmed, "Aterramento nao confirmado por evidencia.", failures);
        RequireTrue(bench.ChannelIsolated, "Canal de bancada nao confirmado como isolado.", failures);
        RequireTrue(bench.OutputsDeenergizedOrIsolated, "Saidas nao confirmadas como isoladas.", failures);
        RequireTrue(bench.MachinePreventedFromOperating, "Maquina nao confirmada como impedida de operar.", failures);
        RequireTrue(bench.EmergencyStopIdentified, "Emergencia nao confirmada.", failures);
        RequireTrue(bench.QuickDisconnectDefined, "Desconexao rapida nao confirmada.", failures);
        ValidateResponsibleDeclarations(bench.ResponsibleDeclarations, failures);
    }

    private static void ValidateEquipment(Layout3BenchEquipmentConfiguration? equipment, ICollection<string> failures)
    {
        if (equipment is null)
        {
            failures.Add("Secao equipment ausente.");
            return;
        }

        RequireText(equipment.PlcModel, "Modelo do controlador ausente.", failures);
        RequireText(equipment.ControllerCpu, "CPU ausente.", failures);
        RequireText(equipment.ModuleModel, "Modulo ausente.", failures);
        if (equipment.CpuSlot is null or < 0 || equipment.ModuleSlot is null or < 0)
        {
            failures.Add("Slots de CPU/modulo ausentes ou invalidos.");
        }

        if (equipment.MaximumModules is null or <= 0
            || equipment.DetectedModules is null or < 0
            || equipment.DetectedModules > equipment.MaximumModules)
        {
            failures.Add("Quantidades de modulos ausentes ou invalidas.");
        }

        if (equipment.AvailableInterfaces.Count == 0)
        {
            failures.Add("Interfaces observadas ausentes.");
        }

        ValidateControllerStatus(equipment.ControllerStatus, failures);
        ValidateModuleStatus(equipment.ModuleStatus, failures);
        RequireText(equipment.Firmware, "Firmware confirmado da CPU ausente.", failures);
        RequireText(equipment.LabelPhotoEvidence, "Evidencia da etiqueta ausente.", failures);
        RequireText(equipment.HiStudioVersion, "Versao do HIstudio ausente.", failures);
        if (!string.IsNullOrWhiteSpace(equipment.ProbableFirmware)
            && !string.Equals(equipment.FirmwareEvidenceStatus, "PROVÁVEL", StringComparison.OrdinalIgnoreCase))
        {
            failures.Add("Firmware provavel deve permanecer classificado como PROVÁVEL.");
        }

        ValidateObservedProgram(equipment.ObservedProgram, failures);
        ValidateLiveDataEvidence(equipment.LiveDataEvidence, failures);
        ValidatePhysicalIdentification(equipment.PhysicalIdentification, failures);
    }

    private static void ValidateControllerStatus(Layout3ControllerStatusEvidence? status, ICollection<string> failures)
    {
        if (status is null
            || status.HardwareRevisionDisplayed is null or < 0
            || status.FirmwareRevisionDisplayed is null or < 0
            || string.IsNullOrWhiteSpace(status.FunctionalStatus)
            || string.IsNullOrWhiteSpace(status.StartupStatus)
            || string.IsNullOrWhiteSpace(status.OperationStatus)
            || string.IsNullOrWhiteSpace(status.IntermittentStatus)
            || string.IsNullOrWhiteSpace(status.ConfigurationStatus))
        {
            failures.Add("Evidencia de status da CPU incompleta.");
        }
    }

    private static void ValidateModuleStatus(Layout3ModuleStatusEvidence? status, ICollection<string> failures)
    {
        if (status is null
            || status.HardwareRevisionDisplayed is null or < 0
            || status.FirmwareRevisionDisplayed is null or < 0
            || string.IsNullOrWhiteSpace(status.FunctionalStatus)
            || status.DigitalInputs?.Count != 8
            || status.DigitalOutputs?.Count != 4
            || status.AnalogInputs?.Count != 3
            || status.FastCounters?.Count != 3
            || status.Pwm?.Count != 1
            || string.IsNullOrWhiteSpace(status.AnalogInputPresentation))
        {
            failures.Add("Evidencia estrutural do HIO115 incompleta.");
        }
    }

    private static void ValidateObservedProgram(Layout3ObservedProgramConfiguration? program, ICollection<string> failures)
    {
        if (program is null
            || string.IsNullOrWhiteSpace(program.Condition)
            || string.IsNullOrWhiteSpace(program.Name)
            || program.Version is null or < 0
            || program.Identifier is null or < 0
            || program.Crc is null or < 0
            || string.IsNullOrWhiteSpace(program.StartupMode))
        {
            failures.Add("Evidencia do programa observado incompleta.");
        }
    }

    private static void ValidateLiveDataEvidence(Layout3LiveDataEvidence? evidence, ICollection<string> failures)
    {
        if (evidence is null)
        {
            failures.Add("Classificacao de dados ao vivo ausente.");
            return;
        }

        RequireText(evidence.RemoteEquipmentCondition, "Condicao remota observada ausente.", failures);
        RequireText(evidence.HardwareBaseCondition, "Condicao da base de hardware ausente.", failures);
        RequireFalse(evidence.DigitalInputStateConfirmed, "Estado atual de DI nao foi confirmado.", failures);
        RequireFalse(evidence.DigitalOutputStateConfirmed, "Estado atual de DO nao foi confirmado.", failures);
        RequireFalse(evidence.AnalogValuesConfirmed, "Valores atuais de AI nao foram confirmados.", failures);
        RequireFalse(evidence.CounterValuesConfirmed, "Contadores atuais nao foram confirmados.", failures);
        RequireFalse(evidence.PwmStateConfirmed, "PWM atual nao foi confirmado.", failures);
    }

    private static void ValidatePhysicalIdentification(
        Layout3PhysicalIdentificationEvidence? identification,
        ICollection<string> failures)
    {
        if (identification is null)
        {
            failures.Add("Identificacao fisica frontal ausente.");
            return;
        }

        RequireText(identification.FrontIdentification, "Identificacao frontal ausente.", failures);
        RequireText(identification.DisplayedManufacturer, "Marca frontal ausente.", failures);
        RequireText(identification.FrontModel, "Modelo frontal ausente.", failures);
        RequireText(identification.SerialNumber, "Numero de serie ausente.", failures);
        RequireText(identification.PartNumber, "Part number ausente.", failures);
        RequireText(identification.NominalSupplyIndication, "Indicacao nominal de alimentacao ausente.", failures);
        RequireText(identification.HiStudioIdentity, "Identidade HIstudio ausente.", failures);
        RequireText(identification.PhysicalFrontIdentity, "Identidade frontal ausente.", failures);
        if (identification.Rs485TerminationSwitchPresent is null)
        {
            failures.Add("Presenca da terminacao RS-485 nao registrada.");
        }

        bool relationshipConfirmed = string.Equals(
                identification.IdentityComparisonStatus,
                "CONFIRMADO",
                StringComparison.OrdinalIgnoreCase)
            && string.Equals(
                identification.IdentityRelationshipStatus,
                "CONFIRMADA",
                StringComparison.OrdinalIgnoreCase);
        if (!relationshipConfirmed)
        {
            failures.Add("Relacao documental OMNI-PLC2/NEON5-1S ainda nao confirmada.");
        }
    }

    private static void ValidateResponsibleDeclarations(
        Layout3ResponsibleDeclarations? declarations,
        ICollection<string> failures)
    {
        if (declarations is null)
        {
            failures.Add("Declaracoes do responsavel ausentes.");
            return;
        }

        RequireText(declarations.DeclaredBy, "Nome do declarante ausente.", failures);
        RequireTrue(declarations.ResponsiblePresent, "Responsavel nao declarado como presente.", failures);
        RequireTrue(declarations.GroundingOk, "Aterramento nao declarado como OK.", failures);
        RequireTrue(declarations.OutputsDeenergizedOrIsolatedOk, "Saidas nao declaradas como isoladas.", failures);
        RequireTrue(declarations.MachinePreventedFromOperatingOk, "Maquina nao declarada como impedida.", failures);
        RequireTrue(declarations.MachineSafeStateOk, "Maquina nao declarada em estado seguro.", failures);
        RequireTrue(declarations.EmergencyStopOk, "Emergencia nao declarada como OK.", failures);
        RequireTrue(declarations.QuickDisconnectOk, "Desconexao rapida nao declarada como OK.", failures);
        RequireTrue(declarations.ProgramBackupOk, "Backup nao declarado como OK.", failures);
        if (!string.Equals(declarations.EvidenceStatus, "CONFIRMADO", StringComparison.OrdinalIgnoreCase))
        {
            failures.Add("Declaracoes do responsavel ainda nao possuem evidencias confirmadas.");
        }
        else
        {
            RequireText(declarations.EvidenceReference, "Referencia das evidencias do responsavel ausente.", failures);
        }
    }

    private static void ValidateApprovals(
        IReadOnlyDictionary<string, string>? statuses,
        ICollection<string> failures)
    {
        if (statuses is null)
        {
            failures.Add("Secao approvalStatuses ausente.");
            return;
        }

        foreach (string key in RequiredApprovalStatuses)
        {
            if (!statuses.TryGetValue(key, out string? status)
                || !string.Equals(status, "CONFIRMADO", StringComparison.OrdinalIgnoreCase))
            {
                failures.Add($"Status {key} deve ser CONFIRMADO; atual: {status ?? "AUSENTE"}.");
            }
        }
    }

    private static void ValidateDocuments(ISet<string> availableDocuments, ICollection<string> failures)
    {
        foreach (string document in RequiredDocuments)
        {
            if (!availableDocuments.Contains(document))
            {
                failures.Add($"Documento obrigatorio ausente: {document}.");
            }
        }
    }

    private static void RequireText(string? value, string failure, ICollection<string> failures)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            failures.Add(failure);
        }
    }

    private static void RequireFalse(bool? value, string failure, ICollection<string> failures)
    {
        if (value is not false)
        {
            failures.Add(failure);
        }
    }

    private static void RequireTrue(bool? value, string failure, ICollection<string> failures)
    {
        if (value is not true)
        {
            failures.Add(failure);
        }
    }
}
