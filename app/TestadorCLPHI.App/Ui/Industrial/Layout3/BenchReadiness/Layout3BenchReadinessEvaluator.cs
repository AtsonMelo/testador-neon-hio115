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
        "docs/layout-3-plano-evidencias-teste.md"
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

    public static Layout3BenchReadinessEvaluationResult Evaluate(
        Layout3BenchReadinessConfiguration configuration,
        ISet<string> availableDocuments)
    {
        ArgumentNullException.ThrowIfNull(configuration);
        ArgumentNullException.ThrowIfNull(availableDocuments);

        List<string> failures = [];

        if (configuration.SchemaVersion != 1)
        {
            failures.Add($"schemaVersion deve ser 1; valor atual: {configuration.SchemaVersion}.");
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
        RequireText(equipment.ModuleModel, "Modelo do modulo ausente.", failures);
        RequireText(equipment.Firmware, "Firmware ausente.", failures);
        RequireText(equipment.LabelPhotoEvidence, "Evidencia da foto/etiqueta ausente.", failures);
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

        RequireText(connection.Protocol, "Protocolo ausente.", failures);
        RequireText(connection.Transport, "Transporte ausente.", failures);
        RequireText(connection.Topology, "Topologia ausente.", failures);

        string protocol = connection.Protocol?.Trim() ?? string.Empty;
        if (string.Equals(protocol, "modbus_tcp", StringComparison.OrdinalIgnoreCase))
        {
            if (!string.Equals(connection.Transport, "ethernet", StringComparison.OrdinalIgnoreCase))
            {
                failures.Add("modbus_tcp exige transport = ethernet.");
            }

            if (!IsValidIpv4(connection.EquipmentIpAddress))
            {
                failures.Add("IP do equipamento ausente ou invalido para modbus_tcp.");
            }

            if (!IsValidIpv4(connection.PcIpAddress))
            {
                failures.Add("IP do PC ausente ou invalido para modbus_tcp.");
            }

            if (connection.TcpPort is null or < 1 or > 65535)
            {
                failures.Add("Porta TCP ausente ou fora do intervalo 1..65535.");
            }

            if (HasSerialParameters(connection))
            {
                failures.Add("Configuracao conflitante: parametros seriais presentes em perfil modbus_tcp.");
            }
        }
        else if (string.Equals(protocol, "modbus_rtu", StringComparison.OrdinalIgnoreCase))
        {
            string[] allowedTransports = ["serial_rs232", "serial_rs485", "wireless_radio_transparent"];
            if (!allowedTransports.Contains(connection.Transport, StringComparer.OrdinalIgnoreCase))
            {
                failures.Add("modbus_rtu exige transporte serial_rs232, serial_rs485 ou wireless_radio_transparent.");
            }

            RequireText(connection.SerialPortName, "Porta serial ausente para modbus_rtu.", failures);
            if (connection.BaudRate is null or <= 0)
            {
                failures.Add("Baud rate ausente ou invalido.");
            }

            if (connection.DataBits is null or < 5 or > 8)
            {
                failures.Add("Data bits ausente ou fora do intervalo 5..8.");
            }

            if (!AllowedParityValues.Contains(connection.Parity, StringComparer.OrdinalIgnoreCase))
            {
                failures.Add("Paridade ausente ou invalida.");
            }

            if (!AllowedStopBitsValues.Contains(connection.StopBits, StringComparer.OrdinalIgnoreCase))
            {
                failures.Add("Stop bits ausente ou invalido.");
            }

            if (HasTcpParameters(connection))
            {
                failures.Add("Configuracao conflitante: parametros TCP presentes em perfil modbus_rtu.");
            }
        }
        else if (!string.IsNullOrWhiteSpace(protocol))
        {
            failures.Add($"Protocolo nao suportado pelo preflight local: {protocol}.");
        }

        if (connection.DeviceAddress is null or < 1 or > 247)
        {
            failures.Add("Unit ID/endereco ausente ou fora do intervalo 1..247.");
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

        if (readPolicy.TimeoutMilliseconds is < 100 or > 5000)
        {
            failures.Add("Timeout deve estar no intervalo local de seguranca 100..5000 ms.");
        }

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

    private static bool HasTcpParameters(Layout3BenchConnectionConfiguration connection)
    {
        return !string.IsNullOrWhiteSpace(connection.EquipmentIpAddress)
            || !string.IsNullOrWhiteSpace(connection.PcIpAddress)
            || connection.TcpPort is not null;
    }

    private static bool HasSerialParameters(Layout3BenchConnectionConfiguration connection)
    {
        return !string.IsNullOrWhiteSpace(connection.SerialPortName)
            || connection.BaudRate is not null
            || connection.DataBits is not null
            || !string.IsNullOrWhiteSpace(connection.Parity)
            || !string.IsNullOrWhiteSpace(connection.StopBits);
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
