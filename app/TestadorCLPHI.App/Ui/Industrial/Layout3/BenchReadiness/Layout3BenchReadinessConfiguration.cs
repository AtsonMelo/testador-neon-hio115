namespace TestadorCLPHI.App.Ui.Industrial.Layout3;

internal sealed class Layout3BenchReadinessConfiguration
{
    public int SchemaVersion { get; set; }

    public Layout3BenchEquipmentConfiguration? Equipment { get; set; }

    public Layout3BenchConnectionConfiguration? Connection { get; set; }

    public Layout3BenchReadPolicyConfiguration? ReadPolicy { get; set; }

    public Layout3BenchSafetyConfiguration? Safety { get; set; }

    public Layout3BenchConditionsConfiguration? Bench { get; set; }

    public Dictionary<string, string>? ApprovalStatuses { get; set; }
}

internal sealed class Layout3BenchEquipmentConfiguration
{
    public string? PlcModel { get; set; }

    public string? ModuleModel { get; set; }

    public string? Firmware { get; set; }

    public string? LabelPhotoEvidence { get; set; }
}

internal sealed class Layout3BenchConnectionConfiguration
{
    public string? Protocol { get; set; }

    public string? Transport { get; set; }

    public string? Topology { get; set; }

    public string? EquipmentIpAddress { get; set; }

    public string? PcIpAddress { get; set; }

    public int? TcpPort { get; set; }

    public string? SerialPortName { get; set; }

    public int? BaudRate { get; set; }

    public int? DataBits { get; set; }

    public string? Parity { get; set; }

    public string? StopBits { get; set; }

    public int? DeviceAddress { get; set; }
}

internal sealed class Layout3BenchReadPolicyConfiguration
{
    public string? RegisterMapReference { get; set; }

    public int TimeoutMilliseconds { get; set; }

    public int MaximumReads { get; set; }

    public List<Layout3BenchAllowedRegister> AllowedRegisters { get; set; } = [];
}

internal sealed class Layout3BenchAllowedRegister
{
    public string? Name { get; set; }

    public string? Area { get; set; }

    public int? Address { get; set; }

    public string? Access { get; set; }

    public string? ApprovalEvidence { get; set; }
}

internal sealed class Layout3BenchSafetyConfiguration
{
    public bool? FeatureEnabled { get; set; }

    public bool? RealCommunicationEnabled { get; set; }

    public bool? WritesEnabled { get; set; }

    public bool? PollingEnabled { get; set; }

    public bool? AutomaticReconnectEnabled { get; set; }

    public bool? SingleShotOnly { get; set; }
}

internal sealed class Layout3BenchConditionsConfiguration
{
    public string? BackupReference { get; set; }

    public string? SupplyVoltage { get; set; }

    public string? Responsible { get; set; }

    public string? MachineState { get; set; }

    public bool? GroundingConfirmed { get; set; }

    public bool? NetworkIsolated { get; set; }

    public bool? OutputsDeenergizedOrIsolated { get; set; }

    public bool? MachinePreventedFromOperating { get; set; }

    public bool? EmergencyStopIdentified { get; set; }

    public bool? QuickDisconnectDefined { get; set; }
}

internal sealed record Layout3BenchReadinessEvaluationResult(IReadOnlyList<string> Failures)
{
    public bool IsReady => Failures.Count == 0;
}
