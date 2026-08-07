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
        "registerMap",
        "registerAllowList",
        "electricalSafety",
        "benchSafety"
    ];

    private static readonly string[] AllowedRegisterAreas =
    [
        "holding_register",
        "input_register"
    ];

    private static readonly string[] AllowedParityValues =
    [
        "None",
        "Even",
        "Odd",
        "Mark",
        "Space"
    ];

    private static readonly string[] AllowedStopBitsValues =
    [
        "One",
        "OnePointFive",
        "Two"
    ];

    private static readonly string[] SupportedProfiles =
    [
        "RTU",
        "TCP"
    ];

    private static readonly string[] AllowedPhysicalLayers =
    [
        "RS232",
        "RS485"
    ];

    public static Layout3BenchReadinessEvaluationResult Evaluate(
        Layout3BenchReadinessConfiguration configuration,
        ISet<string> availableDocuments)
    {
        ArgumentNullException.ThrowIfNull(configuration);
        ArgumentNullException.ThrowIfNull(availableDocuments);

        List<string> failures = [];

        if (configuration.SchemaVersion != 2)
        {
            failures.Add($"schemaVersion deve ser 2; valor atual: {configuration.SchemaVersion}.");
        }

        ValidateEquipment(configuration.Equipment, failures);
        ValidateConnection(configuration.Connection, failures);
        ValidateReadPolicy(configuration.ReadPolicy, failures);
        ValidateSafety(configuration.Safety, failures);
        ValidateBench(configuration.Bench, failures);
        ValidateApprovals(configuration.ApprovalStatuses, failures);
        ValidateDocuments(availableDocuments, failures);

        return new Layout3BenchReadinessEvaluationResult(failures);
    }

    private static void ValidateEquipment(
        Layout3BenchEquipmentConfiguration? equipment,
        ICollection<string> failures)
    {
        if (equipment is null)
        {
            failures.Add("Secao equipment ausente.");
            return;
        }

        RequireText(equipment.PlcModel, "Modelo exato do CLP ausente.", failures);
        RequireText(equipment.ControllerCpu, "CPU do controlador ausente.", failures);
        if (equipment.CpuSlot is null or < 0)
        {
            failures.Add("Slot da CPU ausente ou invalido.");
        }

        if (equipment.MaximumModules is null or <= 0)
        {
            failures.Add("Quantidade maxima de modulos ausente ou invalida.");
        }

        if (equipment.DetectedModules is null or < 0)
        {
            failures.Add("Quantidade de modulos detectados ausente ou invalida.");
        }
        else if (equipment.MaximumModules is not null
            && equipment.DetectedModules > equipment.MaximumModules)
        {
            failures.Add("Quantidade de modulos detectados excede o maximo informado.");
        }

        ValidateAvailableInterfaces(equipment.AvailableInterfaces, failures);
        ValidateControllerStatus(equipment.ControllerStatus, failures);

        RequireText(equipment.ModuleModel, "Modelo do modulo ausente.", failures);
        if (equipment.ModuleSlot is null or < 0)
        {
            failures.Add("Slot do modulo ausente ou invalido.");
        }

        ValidateModuleStatus(equipment.ModuleStatus, failures);

        RequireText(equipment.Firmware, "Firmware ausente.", failures);
        RequireText(equipment.LabelPhotoEvidence, "Evidencia da foto/etiqueta ausente.", failures);
        RequireText(equipment.HiStudioVersion, "Versao do HIstudio ausente.", failures);

        if (!string.IsNullOrWhiteSpace(equipment.ProbableFirmware)
            && !string.Equals(
                equipment.FirmwareEvidenceStatus,
                "PROVÁVEL",
                StringComparison.OrdinalIgnoreCase))
        {
            failures.Add("Firmware provavel deve permanecer classificado como PROVÁVEL.");
        }

        ValidateObservedProgram(equipment.ObservedProgram, failures);
        ValidateLiveDataEvidence(equipment.LiveDataEvidence, failures);
        ValidatePhysicalIdentification(equipment.PhysicalIdentification, failures);
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

        ValidateSupportedProfiles(connection.SupportedProfiles, failures);
        RequireText(connection.SelectedProfile, "Selecao explicita de perfil RTU ou TCP ausente.", failures);
        RequireText(connection.ObservedTransport, "Transporte atualmente observado ausente.", failures);

        if (connection.ActiveProtocolConfirmed is not true)
        {
            failures.Add("Protocolo realmente ativado no canal ainda nao foi confirmado.");
        }

        ValidateAddressPolicy(connection.Addressing, failures);
        ValidateDiscovery(connection.Discovery, failures);

        if (string.Equals(connection.SelectedProfile, "RTU", StringComparison.OrdinalIgnoreCase))
        {
            ValidateRtuProfile(connection.Rtu, connection.Addressing, failures);
        }
        else if (string.Equals(connection.SelectedProfile, "TCP", StringComparison.OrdinalIgnoreCase))
        {
            ValidateTcpProfile(connection.Tcp, connection.Addressing, failures);
        }
        else if (!string.IsNullOrWhiteSpace(connection.SelectedProfile))
        {
            failures.Add($"Perfil selecionado nao suportado: {connection.SelectedProfile}.");
        }
    }

    private static void ValidateReadPolicy(
        Layout3BenchReadPolicyConfiguration? readPolicy,
        ICollection<string> failures)
    {
        if (readPolicy is null)
        {
            failures.Add("Secao readPolicy ausente.");
            return;
        }

        RequireText(readPolicy.RegisterMapReference, "Referencia do mapa de registradores ausente.", failures);

        if (readPolicy.MaximumReads <= 0)
        {
            failures.Add("Limite maximo de leituras deve ser maior que zero.");
        }

        if (readPolicy.AllowedRegisters.Count == 0)
        {
            failures.Add("Allow-list de registradores esta vazia.");
            return;
        }

        if (readPolicy.MaximumReads > readPolicy.AllowedRegisters.Count)
        {
            failures.Add("Limite maximo de leituras excede a quantidade da allow-list single-shot.");
        }

        HashSet<string> uniqueAddresses = new(StringComparer.OrdinalIgnoreCase);
        for (int index = 0; index < readPolicy.AllowedRegisters.Count; index++)
        {
            Layout3BenchAllowedRegister register = readPolicy.AllowedRegisters[index];
            string item = $"allowedRegisters[{index}]";

            RequireText(register.Name, $"{item}.name ausente.", failures);
            RequireText(register.ApprovalEvidence, $"{item}.approvalEvidence ausente.", failures);

            if (!AllowedRegisterAreas.Contains(register.Area, StringComparer.OrdinalIgnoreCase))
            {
                failures.Add($"{item}.area deve ser holding_register ou input_register; coils nao sao permitidas.");
            }

            if (register.Address is null or < 0 or > 65535)
            {
                failures.Add($"{item}.address ausente ou fora do intervalo 0..65535.");
            }

            if (!string.Equals(register.Access, "read", StringComparison.OrdinalIgnoreCase))
            {
                failures.Add($"{item}.access deve ser exclusivamente read; escrita nao e permitida.");
            }

            if (register.Address is not null && !string.IsNullOrWhiteSpace(register.Area))
            {
                string key = $"{register.Area.Trim()}:{register.Address.Value}";
                if (!uniqueAddresses.Add(key))
                {
                    failures.Add($"Registrador duplicado na allow-list: {key}.");
                }
            }
        }
    }

    private static void ValidateSafety(
        Layout3BenchSafetyConfiguration? safety,
        ICollection<string> failures)
    {
        if (safety is null)
        {
            failures.Add("Secao safety ausente.");
            return;
        }

        RequireFalse(safety.FeatureEnabled, "Feature flag deve permanecer OFF por padrao.", failures);
        RequireFalse(safety.RealCommunicationEnabled, "Comunicacao real deve permanecer OFF.", failures);
        RequireFalse(safety.WritesEnabled, "Escrita deve permanecer desabilitada.", failures);
        RequireFalse(safety.PollingEnabled, "Polling continuo deve permanecer desabilitado.", failures);
        RequireFalse(safety.AutomaticReconnectEnabled, "Reconexao automatica deve permanecer desabilitada.", failures);

        if (safety.SingleShotOnly is not true)
        {
            failures.Add("Politica single-shot deve estar explicitamente habilitada.");
        }

        if (safety.GateDAuthorized is not true)
        {
            failures.Add("Gate D para implementacao do transporte ainda nao autorizado.");
        }
    }

    private static void ValidateBench(
        Layout3BenchConditionsConfiguration? bench,
        ICollection<string> failures)
    {
        if (bench is null)
        {
            failures.Add("Secao bench ausente.");
            return;
        }

        RequireText(bench.BackupReference, "Referencia do backup do projeto ausente.", failures);
        RequireText(bench.SupplyVoltage, "Tensao de alimentacao confirmada ausente.", failures);
        RequireText(bench.Responsible, "Responsavel da bancada ausente.", failures);
        RequireText(bench.MachineState, "Estado seguro da maquina ausente.", failures);
        RequireTrue(bench.GroundingConfirmed, "Aterramento nao confirmado.", failures);
        RequireTrue(bench.NetworkIsolated, "Rede/canal de bancada nao confirmado como isolado.", failures);
        RequireTrue(bench.OutputsDeenergizedOrIsolated, "Saidas nao confirmadas como desenergizadas ou isoladas.", failures);
        RequireTrue(bench.MachinePreventedFromOperating, "Maquina nao confirmada como impedida de operar.", failures);
        RequireTrue(bench.EmergencyStopIdentified, "Botao de emergencia nao identificado.", failures);
        RequireTrue(bench.QuickDisconnectDefined, "Desconexao rapida nao definida.", failures);
        ValidateResponsibleDeclarations(bench.ResponsibleDeclarations, failures);
    }

    private static void ValidateApprovals(
        IReadOnlyDictionary<string, string>? approvalStatuses,
        ICollection<string> failures)
    {
        if (approvalStatuses is null)
        {
            failures.Add("Secao approvalStatuses ausente.");
            return;
        }

        foreach (string key in RequiredApprovalStatuses)
        {
            if (!approvalStatuses.TryGetValue(key, out string? status)
                || !string.Equals(status, "CONFIRMADO", StringComparison.OrdinalIgnoreCase))
            {
                failures.Add($"Status {key} deve ser CONFIRMADO; atual: {status ?? "AUSENTE"}.");
            }
        }
    }

    private static void ValidateDocuments(
        ISet<string> availableDocuments,
        ICollection<string> failures)
    {
        foreach (string document in RequiredDocuments)
        {
            if (!availableDocuments.Contains(document))
            {
                failures.Add($"Documento obrigatorio ausente: {document}.");
            }
        }
    }

    private static void ValidateSupportedProfiles(
        IReadOnlyList<string> profiles,
        ICollection<string> failures)
    {
        HashSet<string> unique = new(profiles, StringComparer.OrdinalIgnoreCase);
        if (profiles.Count != SupportedProfiles.Length || unique.Count != SupportedProfiles.Length)
        {
            failures.Add("supportedProfiles deve conter exatamente RTU e TCP, sem duplicatas.");
            return;
        }

        foreach (string profile in SupportedProfiles)
        {
            if (!unique.Contains(profile))
            {
                failures.Add($"Perfil obrigatorio ausente em supportedProfiles: {profile}.");
            }
        }
    }

    private static void ValidateRtuProfile(
        Layout3ModbusRtuProfileConfiguration? profile,
        Layout3ModbusAddressPolicyConfiguration? addressing,
        ICollection<string> failures)
    {
        if (profile is null)
        {
            failures.Add("Perfil RTU selecionado, mas secao rtu ausente.");
            return;
        }

        RequireText(profile.Driver, "Driver observado do perfil RTU ausente.", failures);
        RequireText(profile.Channel, "Canal observado do perfil RTU ausente.", failures);
        RequireText(profile.SerialPortName, "Porta COM ausente no perfil RTU.", failures);
        RequireText(profile.ControllerInterface, "Interface do controlador ausente no perfil RTU.", failures);
        RequireText(profile.ControllerInterfaceStatus, "Status da interface do controlador ausente.", failures);
        RequireText(profile.PhysicalConnector, "Conector fisico atual ausente no perfil RTU.", failures);

        if (!AllowedPhysicalLayers.Contains(profile.PhysicalLayer, StringComparer.OrdinalIgnoreCase))
        {
            failures.Add("Camada fisica RTU deve ser RS232 ou RS485.");
        }

        if (profile.BaudRate is null or <= 0)
        {
            failures.Add("Baud rate ausente ou invalido no perfil RTU.");
        }

        if (profile.DataBits is null or < 5 or > 8)
        {
            failures.Add("Data bits ausente ou fora do intervalo 5..8 no perfil RTU.");
        }

        if (!AllowedParityValues.Contains(profile.Parity, StringComparer.OrdinalIgnoreCase))
        {
            failures.Add("Paridade ausente ou invalida no perfil RTU.");
        }

        if (!AllowedStopBitsValues.Contains(profile.StopBits, StringComparer.OrdinalIgnoreCase))
        {
            failures.Add("Stop bits ausente ou invalido no perfil RTU.");
        }

        if (profile.InterCharacterTimeoutMilliseconds is null or < 0)
        {
            failures.Add("Timeout entre caracteres ausente ou invalido no perfil RTU.");
        }

        if (profile.TransmissionDelayMilliseconds is null or < 0)
        {
            failures.Add("Atraso para transmissao ausente ou invalido no perfil RTU.");
        }

        if (profile.CarrierRemovalDelayMilliseconds is null or < 0)
        {
            failures.Add("Atraso para remover portadora ausente ou invalido no perfil RTU.");
        }

        if (profile.MaximumFrameSize is null or <= 0)
        {
            failures.Add("Tamanho maximo do frame ausente ou invalido no perfil RTU.");
        }

        if (profile.AddressRemappingEnabled is null)
        {
            failures.Add("Remapeamento de endereco do perfil RTU deve estar definido.");
        }

        ValidateOperationAddress(profile.DeviceAddress, addressing, "RTU", failures);
        ValidateTimeoutAndAttempts(
            profile.TimeoutMilliseconds,
            profile.MaximumAttempts,
            "RTU",
            failures);

        if (profile.AttemptIntervalMilliseconds is null or <= 0)
        {
            failures.Add("Intervalo entre tentativas RTU ausente ou invalido.");
        }
    }

    private static void ValidateTcpProfile(
        Layout3ModbusTcpProfileConfiguration? profile,
        Layout3ModbusAddressPolicyConfiguration? addressing,
        ICollection<string> failures)
    {
        if (profile is null)
        {
            failures.Add("Perfil TCP selecionado, mas secao tcp ausente.");
            return;
        }

        if (!IsValidIpv4(profile.EquipmentIpAddress))
        {
            failures.Add("IP do controlador ausente ou invalido no perfil TCP.");
        }

        if (profile.TcpPort is null or < 1 or > 65535)
        {
            failures.Add("Porta TCP ausente ou fora do intervalo 1..65535.");
        }

        if (!string.Equals(profile.Topology, "isolated", StringComparison.OrdinalIgnoreCase))
        {
            failures.Add("Perfil TCP exige topologia isolada explicitamente confirmada.");
        }

        ValidateOperationAddress(profile.DeviceAddress, addressing, "TCP", failures);
        ValidateTimeoutAndAttempts(
            profile.TimeoutMilliseconds,
            profile.MaximumAttempts,
            "TCP",
            failures);
    }

    private static void ValidateTimeoutAndAttempts(
        int? timeoutMilliseconds,
        int? maximumAttempts,
        string profile,
        ICollection<string> failures)
    {
        if (timeoutMilliseconds is null or < 100 or > 5000)
        {
            failures.Add($"Timeout do perfil {profile} deve estar no intervalo 100..5000 ms.");
        }

        if (maximumAttempts != 1)
        {
            failures.Add($"Perfil {profile} deve limitar o maximo de tentativas a 1.");
        }
    }

    private static void ValidateOperationAddress(
        int? address,
        Layout3ModbusAddressPolicyConfiguration? addressing,
        string profile,
        ICollection<string> failures)
    {
        if (address is null or < 1 or > 255)
        {
            failures.Add($"Endereco do perfil {profile} deve estar no intervalo cadastravel 1..255; 0 e proibido.");
            return;
        }

        if (address <= 247)
        {
            return;
        }

        Layout3ReservedAddressAccessConfiguration? advanced = addressing?.AdvancedReservedAccess;
        bool approved = advanced?.Enabled is true
            && advanced.ManualSingleAddress == address
            && advanced.WarningAcknowledged is true
            && !string.IsNullOrWhiteSpace(advanced.ExplicitApprovalReference);
        if (!approved)
        {
            failures.Add(
                $"Endereco reservado {address} no perfil {profile} exige modo avancado, selecao manual unica, aviso e aprovacao explicita.");
        }
    }

    private static void ValidateAddressPolicy(
        Layout3ModbusAddressPolicyConfiguration? policy,
        ICollection<string> failures)
    {
        if (policy is null)
        {
            failures.Add("Politica de enderecos Modbus ausente.");
            return;
        }

        if (policy.RepresentableMinimum != 1 || policy.RepresentableMaximum != 255)
        {
            failures.Add("Faixa cadastravel deve ser exatamente 1..255.");
        }

        if (policy.StandardDiscoveryMinimum != 1 || policy.StandardDiscoveryMaximum != 247)
        {
            failures.Add("Faixa padrao de descoberta deve ser exatamente 1..247.");
        }

        if (policy.ReservedMinimum != 248 || policy.ReservedMaximum != 255)
        {
            failures.Add("Faixa reservada/vendor-specific deve ser exatamente 248..255.");
        }

        if (policy.BroadcastAddress != 0)
        {
            failures.Add("Endereco de broadcast proibido deve ser registrado como 0.");
        }

        if (policy.NeverAutomaticallyProbeAddress != 255)
        {
            failures.Add("Endereco 255 deve estar marcado como nunca sondado automaticamente.");
        }

        if (policy.CurrentKnownAddress is null or < 1 or > 255)
        {
            failures.Add("Endereco atual conhecido ausente ou fora do intervalo 1..255.");
        }

        Layout3ReservedAddressAccessConfiguration? advanced = policy.AdvancedReservedAccess;
        if (advanced is null)
        {
            failures.Add("Politica de acesso avancado a enderecos reservados ausente.");
            return;
        }

        if (advanced.Enabled is null)
        {
            failures.Add("Modo avancado para endereco reservado deve estar explicitamente definido.");
        }
        else if (advanced.Enabled is true)
        {
            if (advanced.ManualSingleAddress is null or < 248 or > 255)
            {
                failures.Add("Modo avancado exige selecao manual unica em 248..255.");
            }

            RequireTrue(advanced.WarningAcknowledged, "Aviso de endereco reservado nao reconhecido.", failures);
            RequireText(advanced.ExplicitApprovalReference, "Aprovacao explicita para endereco reservado ausente.", failures);
        }
        else if (advanced.ManualSingleAddress is not null
            || advanced.WarningAcknowledged is true
            || !string.IsNullOrWhiteSpace(advanced.ExplicitApprovalReference))
        {
            failures.Add("Dados de aprovacao reservada presentes com modo avancado desligado.");
        }
    }

    private static void ValidateDiscovery(
        Layout3ModbusDiscoveryConfiguration? discovery,
        ICollection<string> failures)
    {
        if (discovery is null)
        {
            failures.Add("Desenho offline de descoberta ausente.");
            return;
        }

        RequireFalse(discovery.Enabled, "Descoberta deve permanecer desligada por padrao.", failures);
        RequireTrue(discovery.ExplicitStartRequired, "Descoberta deve exigir comando explicito.", failures);
        RequireFalse(discovery.ContinuousRepeatEnabled, "Descoberta nao pode repetir continuamente.", failures);
        RequireTrue(discovery.ImmediateCancellationEnabled, "Descoberta deve permitir cancelamento imediato.", failures);
        RequireFalse(discovery.WritesAllowed, "Descoberta nao pode permitir escrita.", failures);
        RequireFalse(discovery.CoilsAllowed, "Descoberta nao pode permitir coils.", failures);
        RequireTrue(discovery.RequireAllIdentificationValues, "Identificacao deve exigir ID e CRC simultaneamente.", failures);
        RequireFalse(discovery.UseHioIoForDiscovery, "I/O do HIO115 nao pode ser usado para descoberta.", failures);

        if (discovery.FunctionCode != 3)
        {
            failures.Add("Descoberta planejada deve usar somente FC03.");
        }

        if (discovery.MaximumAttemptsPerAddress != 1)
        {
            failures.Add("Descoberta deve limitar a uma tentativa por endereco.");
        }

        if (discovery.Enabled is true
            && (discovery.AttemptIntervalMilliseconds is null or <= 0))
        {
            failures.Add("Descoberta habilitada exige intervalo positivo entre tentativas.");
        }

        if (discovery.RangeStart is null or < 1 or > 247
            || discovery.RangeEnd is null or < 1 or > 247
            || discovery.RangeStart > discovery.RangeEnd)
        {
            failures.Add("Faixa de descoberta deve ser crescente e permanecer em 1..247.");
        }

        HashSet<int> allowedAddresses = new(discovery.AddressAllowList);
        if (discovery.AddressAllowList.Count == 0
            || allowedAddresses.Count != discovery.AddressAllowList.Count
            || allowedAddresses.Any(address => address is < 1 or > 247))
        {
            failures.Add("Allow-list de descoberta deve conter enderecos unicos somente em 1..247.");
        }
        else if (discovery.RangeStart is not null && discovery.RangeEnd is not null)
        {
            for (int address = discovery.RangeStart.Value; address <= discovery.RangeEnd.Value; address++)
            {
                if (!allowedAddresses.Contains(address))
                {
                    failures.Add($"Endereco {address} da faixa nao esta na allow-list de descoberta.");
                    break;
                }
            }
        }

        ValidateIdentificationCandidates(discovery.IdentificationCandidates, failures);
    }

    private static void ValidateIdentificationCandidates(
        IReadOnlyList<Layout3DiscoveryIdentificationCandidate> candidates,
        ICollection<string> failures)
    {
        if (candidates.Count != 2)
        {
            failures.Add("Descoberta deve ter exatamente os candidatos F12/30012 e F13/30013.");
            return;
        }

        ValidateIdentificationCandidate(candidates, "F12", "30012", 31134, failures);
        ValidateIdentificationCandidate(candidates, "F13", "30013", 23248, failures);
    }

    private static void ValidateIdentificationCandidate(
        IReadOnlyList<Layout3DiscoveryIdentificationCandidate> candidates,
        string name,
        string displayReference,
        int expectedValue,
        ICollection<string> failures)
    {
        Layout3DiscoveryIdentificationCandidate? candidate = candidates.FirstOrDefault(
            item => string.Equals(item.Name, name, StringComparison.OrdinalIgnoreCase));
        if (candidate is null
            || !string.Equals(candidate.DisplayReference, displayReference, StringComparison.OrdinalIgnoreCase)
            || !string.Equals(candidate.Access, "R", StringComparison.OrdinalIgnoreCase)
            || candidate.ExpectedValue != expectedValue)
        {
            failures.Add($"Candidato {name}/{displayReference} deve ser somente leitura e esperar valor {expectedValue}.");
            return;
        }

        if (candidate.ProtocolDataAddress is not null)
        {
            failures.Add($"Endereco de dados de protocolo de {name}/{displayReference} deve permanecer vazio ate aprovacao do mapa.");
        }
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
        RequireText(identification.DisplayedManufacturer, "Fabricante/marca frontal ausente.", failures);
        RequireText(identification.FrontModel, "Modelo frontal ausente.", failures);
        RequireText(identification.SerialNumber, "Numero de serie frontal ausente.", failures);
        RequireText(identification.PartNumber, "Part number frontal ausente.", failures);
        RequireText(identification.AdditionalIdentification, "Identificacao fisica adicional ausente.", failures);
        RequireText(
            identification.AdditionalIdentificationAssessment,
            "Avaliacao da identificacao fisica adicional ausente.",
            failures);
        RequireText(identification.NominalSupplyIndication, "Indicacao nominal de alimentacao ausente.", failures);
        RequireText(identification.CurrentConnector, "Conector fisico atual ausente.", failures);
        RequireText(identification.Rs485Terminals, "Evidencia dos bornes RS-485 ausente.", failures);
        RequireText(identification.ObservationStatus, "Status da observacao fisica ausente.", failures);
        RequireText(identification.HiStudioIdentity, "Identidade observada no HIstudio ausente.", failures);
        RequireText(identification.PhysicalFrontIdentity, "Identidade fisica frontal comparada ausente.", failures);

        if (identification.Rs485TerminationSwitchPresent is null)
        {
            failures.Add("Presenca da chave de terminacao RS-485 nao registrada.");
        }

        bool identityConfirmed = string.Equals(
                identification.IdentityComparisonStatus,
                "CONFIRMADO",
                StringComparison.OrdinalIgnoreCase)
            && string.Equals(
                identification.IdentityRelationshipStatus,
                "CONFIRMADA",
                StringComparison.OrdinalIgnoreCase);
        if (!identityConfirmed)
        {
            failures.Add("Relacao documental entre identidade fisica e identidade HIstudio nao confirmada.");
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

        RequireText(declarations.DeclaredBy, "Nome do responsavel declarante ausente.", failures);
        RequireTrue(declarations.ResponsiblePresent, "Presenca do responsavel nao declarada.", failures);
        RequireTrue(declarations.GroundingOk, "Aterramento nao declarado como OK.", failures);
        RequireTrue(
            declarations.OutputsDeenergizedOrIsolatedOk,
            "Isolamento/desenergizacao das saidas nao declarado como OK.",
            failures);
        RequireTrue(
            declarations.MachinePreventedFromOperatingOk,
            "Impedimento de operacao da maquina nao declarado como OK.",
            failures);
        RequireTrue(declarations.MachineSafeStateOk, "Estado seguro da maquina nao declarado como OK.", failures);
        RequireTrue(declarations.EmergencyStopOk, "Emergencia nao declarada como OK.", failures);
        RequireTrue(declarations.QuickDisconnectOk, "Desconexao rapida nao declarada como OK.", failures);
        RequireTrue(declarations.ProgramBackupOk, "Backup do programa nao declarado como OK.", failures);

        if (!string.Equals(declarations.EvidenceStatus, "CONFIRMADO", StringComparison.OrdinalIgnoreCase))
        {
            failures.Add("Declaracoes do responsavel ainda nao possuem evidencia confirmada.");
        }
        else
        {
            RequireText(
                declarations.EvidenceReference,
                "Referencia da evidencia confirmada das declaracoes ausente.",
                failures);
        }
    }

    private static void ValidateObservedProgram(
        Layout3ObservedProgramConfiguration? program,
        ICollection<string> failures)
    {
        if (program is null)
        {
            failures.Add("Evidencia do programa observado no HIstudio ausente.");
            return;
        }

        RequireText(program.Condition, "Condicao observada do programa ausente.", failures);
        RequireText(program.Name, "Nome do programa observado ausente.", failures);
        if (program.Version is null or < 0)
        {
            failures.Add("Versao do programa observada ausente ou invalida.");
        }

        if (program.Identifier is null or < 0)
        {
            failures.Add("Identificador do programa observado ausente ou invalido.");
        }

        if (program.Crc is null or < 0)
        {
            failures.Add("CRC do programa observado ausente ou invalido.");
        }

        RequireText(program.StartupMode, "Modo de inicializacao observado ausente.", failures);
    }

    private static void ValidateAvailableInterfaces(
        IReadOnlyList<Layout3AvailableInterfaceEvidence> interfaces,
        ICollection<string> failures)
    {
        if (interfaces.Count == 0)
        {
            failures.Add("Interfaces disponiveis do controlador ausentes.");
            return;
        }

        HashSet<string> names = new(StringComparer.OrdinalIgnoreCase);
        for (int index = 0; index < interfaces.Count; index++)
        {
            Layout3AvailableInterfaceEvidence item = interfaces[index];
            RequireText(item.Name, $"availableInterfaces[{index}].name ausente.", failures);
            RequireText(item.PhysicalLayer, $"availableInterfaces[{index}].physicalLayer ausente.", failures);
            if (!string.IsNullOrWhiteSpace(item.Name) && !names.Add(item.Name))
            {
                failures.Add($"Interface duplicada: {item.Name}.");
            }
        }
    }

    private static void ValidateControllerStatus(
        Layout3ControllerStatusEvidence? status,
        ICollection<string> failures)
    {
        if (status is null)
        {
            failures.Add("Status observado da CPU ausente.");
            return;
        }

        RequireDisplayedRevision(status.HardwareRevisionDisplayed, "Revisao de hardware da CPU", failures);
        RequireDisplayedRevision(status.FirmwareRevisionDisplayed, "Revisao de firmware exibida da CPU", failures);
        RequireText(status.FunctionalStatus, "Status funcional da CPU ausente.", failures);
        RequireText(status.StartupStatus, "Status de inicializacao da CPU ausente.", failures);
        RequireText(status.OperationStatus, "Status de operacao da CPU ausente.", failures);
        RequireText(status.IntermittentStatus, "Status intermitente da CPU ausente.", failures);
        RequireText(status.ConfigurationStatus, "Status de configuracao da CPU ausente.", failures);
    }

    private static void ValidateModuleStatus(
        Layout3ModuleStatusEvidence? status,
        ICollection<string> failures)
    {
        if (status is null)
        {
            failures.Add("Status observado do modulo ausente.");
            return;
        }

        RequireDisplayedRevision(status.HardwareRevisionDisplayed, "Revisao de hardware do modulo", failures);
        RequireDisplayedRevision(status.FirmwareRevisionDisplayed, "Revisao de firmware exibida do modulo", failures);
        RequireText(status.FunctionalStatus, "Status funcional do modulo ausente.", failures);
        ValidateChannelGroup(status.DigitalInputs, "Entradas digitais", failures);
        ValidateChannelGroup(status.DigitalOutputs, "Saidas digitais", failures);
        ValidateChannelGroup(status.AnalogInputs, "Entradas analogicas", failures);
        ValidateChannelGroup(status.FastCounters, "Contadores rapidos", failures);
        ValidateChannelGroup(status.Pwm, "PWM", failures);
        RequireText(status.AnalogInputPresentation, "Apresentacao das entradas analogicas ausente.", failures);
    }

    private static void ValidateChannelGroup(
        Layout3ChannelGroupEvidence? group,
        string label,
        ICollection<string> failures)
    {
        if (group is null)
        {
            failures.Add($"{label}: evidencia ausente.");
            return;
        }

        if (group.Count is null or <= 0)
        {
            failures.Add($"{label}: quantidade ausente ou invalida.");
        }

        RequireText(group.Range, $"{label}: faixa de canais ausente.", failures);
    }

    private static void ValidateLiveDataEvidence(
        Layout3LiveDataEvidence? evidence,
        ICollection<string> failures)
    {
        if (evidence is null)
        {
            failures.Add("Classificacao da evidencia de dados ao vivo ausente.");
            return;
        }

        RequireText(evidence.RemoteEquipmentCondition, "Condicao do equipamento remoto ausente.", failures);
        RequireText(evidence.HardwareBaseCondition, "Condicao da base de hardware ausente.", failures);
        RequireFalse(evidence.DigitalInputStateConfirmed, "Estado atual das entradas nao pode ser promovido.", failures);
        RequireFalse(evidence.DigitalOutputStateConfirmed, "Estado atual das saidas nao pode ser promovido.", failures);
        RequireFalse(evidence.AnalogValuesConfirmed, "Valores analogicos nao podem ser promovidos.", failures);
        RequireFalse(evidence.CounterValuesConfirmed, "Valores de contadores nao podem ser promovidos.", failures);
        RequireFalse(evidence.PwmStateConfirmed, "Estado de PWM nao pode ser promovido.", failures);
    }

    private static void RequireDisplayedRevision(
        int? value,
        string label,
        ICollection<string> failures)
    {
        if (value is null or < 0)
        {
            failures.Add($"{label} ausente ou invalida.");
        }
    }

    private static bool IsValidIpv4(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return false;
        }

        string[] parts = value.Split('.', StringSplitOptions.None);
        return parts.Length == 4
            && parts.All(part =>
                part.Length > 0
                && part.All(char.IsDigit)
                && byte.TryParse(part, out _));
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
