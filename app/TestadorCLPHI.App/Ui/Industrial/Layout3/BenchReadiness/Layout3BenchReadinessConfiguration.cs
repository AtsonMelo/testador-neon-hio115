namespace TestadorCLPHI.App.Ui.Industrial.Layout3;

internal sealed class Layout3BenchReadinessConfiguration
{
    public int SchemaVersion { get; set; }
    public Layout3BenchEquipmentConfiguration? Equipment { get; set; }
    public Layout3BenchConnectionConfiguration? Connection { get; set; }
    public Layout3EquipmentIdentificationConfiguration? Identification { get; set; }
    public Layout3BenchReadPolicyConfiguration? ReadPolicy { get; set; }
    public Layout3BenchOutputPolicyConfiguration? OutputPolicy { get; set; }
    public Layout3BenchSafetyConfiguration? Safety { get; set; }
    public Layout3BenchConditionsConfiguration? Bench { get; set; }
    public Dictionary<string, string>? ApprovalStatuses { get; set; }
}

internal sealed class Layout3BenchEquipmentConfiguration
{
    public string? PlcModel { get; set; }
    public string? ControllerCpu { get; set; }
    public int? CpuSlot { get; set; }
    public int? MaximumModules { get; set; }
    public int? DetectedModules { get; set; }
    public List<Layout3AvailableInterfaceEvidence> AvailableInterfaces { get; set; } = [];
    public Layout3ControllerStatusEvidence? ControllerStatus { get; set; }
    public string? ModuleModel { get; set; }
    public int? ModuleSlot { get; set; }
    public Layout3ModuleStatusEvidence? ModuleStatus { get; set; }
    public string? Firmware { get; set; }
    public string? ProbableFirmware { get; set; }
    public string? FirmwareEvidenceStatus { get; set; }
    public string? LabelPhotoEvidence { get; set; }
    public string? HiStudioVersion { get; set; }
    public Layout3ObservedProgramConfiguration? ObservedProgram { get; set; }
    public Layout3LiveDataEvidence? LiveDataEvidence { get; set; }
    public Layout3PhysicalIdentificationEvidence? PhysicalIdentification { get; set; }
}

internal sealed class Layout3PhysicalIdentificationEvidence
{
    public string? FrontIdentification { get; set; }
    public string? DisplayedManufacturer { get; set; }
    public string? FrontModel { get; set; }
    public string? SerialNumber { get; set; }
    public string? PartNumber { get; set; }
    public string? AdditionalIdentification { get; set; }
    public string? AdditionalIdentificationAssessment { get; set; }
    public string? NominalSupplyIndication { get; set; }
    public string? CurrentConnector { get; set; }
    public string? Rs485Terminals { get; set; }
    public bool? Rs485TerminationSwitchPresent { get; set; }
    public string? ObservationStatus { get; set; }
    public string? HiStudioIdentity { get; set; }
    public string? PhysicalFrontIdentity { get; set; }
    public string? IdentityComparisonStatus { get; set; }
    public string? IdentityRelationshipStatus { get; set; }
}

internal sealed class Layout3AvailableInterfaceEvidence
{
    public string? Name { get; set; }
    public string? PhysicalLayer { get; set; }
}

internal sealed class Layout3ControllerStatusEvidence
{
    public int? HardwareRevisionDisplayed { get; set; }
    public int? FirmwareRevisionDisplayed { get; set; }
    public string? FunctionalStatus { get; set; }
    public string? StartupStatus { get; set; }
    public string? OperationStatus { get; set; }
    public string? IntermittentStatus { get; set; }
    public string? ConfigurationStatus { get; set; }
}

internal sealed class Layout3ModuleStatusEvidence
{
    public int? HardwareRevisionDisplayed { get; set; }
    public int? FirmwareRevisionDisplayed { get; set; }
    public string? FunctionalStatus { get; set; }
    public Layout3ChannelGroupEvidence? DigitalInputs { get; set; }
    public Layout3ChannelGroupEvidence? DigitalOutputs { get; set; }
    public Layout3ChannelGroupEvidence? AnalogInputs { get; set; }
    public Layout3ChannelGroupEvidence? FastCounters { get; set; }
    public Layout3ChannelGroupEvidence? Pwm { get; set; }
    public string? AnalogInputPresentation { get; set; }
}

internal sealed class Layout3ChannelGroupEvidence
{
    public int? Count { get; set; }
    public string? Range { get; set; }
}

internal sealed class Layout3LiveDataEvidence
{
    public string? RemoteEquipmentCondition { get; set; }
    public string? HardwareBaseCondition { get; set; }
    public bool? DigitalInputStateConfirmed { get; set; }
    public bool? DigitalOutputStateConfirmed { get; set; }
    public bool? AnalogValuesConfirmed { get; set; }
    public bool? CounterValuesConfirmed { get; set; }
    public bool? PwmStateConfirmed { get; set; }
}

internal sealed class Layout3ObservedProgramConfiguration
{
    public string? Condition { get; set; }
    public string? Name { get; set; }
    public int? Version { get; set; }
    public int? Identifier { get; set; }
    public int? Crc { get; set; }
    public string? StartupMode { get; set; }
}

internal sealed class Layout3BenchConnectionConfiguration
{
    public string? SelectedTransport { get; set; }
    public bool? ActiveProtocolConfirmed { get; set; }
    public string? ObservedTransport { get; set; }
    public Layout3ModbusRtuProfileConfiguration? Rtu { get; set; }
    public Layout3ModbusAddressPolicyConfiguration? Addressing { get; set; }
    public Layout3ModbusDiscoveryConfiguration? Discovery { get; set; }
}

internal sealed class Layout3ModbusRtuProfileConfiguration
{
    public string? Driver { get; set; }
    public string? Channel { get; set; }
    public string? SerialPortName { get; set; }
    public string? PhysicalLayer { get; set; }
    public string? ControllerInterface { get; set; }
    public string? ControllerInterfaceExact { get; set; }
    public string? PhysicalConnector { get; set; }
    public int? BaudRate { get; set; }
    public int? DataBits { get; set; }
    public string? Parity { get; set; }
    public string? StopBits { get; set; }
    public int? InterCharacterTimeoutMilliseconds { get; set; }
    public int? TransmissionDelayMilliseconds { get; set; }
    public int? CarrierRemovalDelayMilliseconds { get; set; }
    public int? MaximumFrameSize { get; set; }
    public bool? AddressRemappingEnabled { get; set; }
    public int? ManualAddress { get; set; }
    public int? TimeoutMilliseconds { get; set; }
    public int? MaximumAttempts { get; set; }
    public int? AttemptIntervalMilliseconds { get; set; }
}

internal sealed class Layout3ModbusAddressPolicyConfiguration
{
    public int? BroadcastAddress { get; set; }
    public int? StandardMinimum { get; set; }
    public int? StandardMaximum { get; set; }
    public int? ReservedMinimum { get; set; }
    public int? ReservedMaximum { get; set; }
    public int? HistoricalObservedAddress { get; set; }
    public bool? ReservedAddressesEnabled { get; set; }
}

internal sealed class Layout3ModbusDiscoveryConfiguration
{
    public bool? Enabled { get; set; }
    public bool? ExplicitStartRequired { get; set; }
    public int? StartAddress { get; set; }
    public int? EndAddress { get; set; }
    public int? MaximumAttempts { get; set; }
    public int? MaximumAttemptsPerAddress { get; set; }
    public int? AttemptIntervalMilliseconds { get; set; }
    public bool? SequentialAscending { get; set; }
    public bool? ContinuousRepeatEnabled { get; set; }
    public bool? AutomaticReconnectEnabled { get; set; }
    public bool? ImmediateCancellationEnabled { get; set; }
    public bool? StopOnValidSignature { get; set; }
    public int? FunctionCode { get; set; }
    public bool? WritesAllowed { get; set; }
    public bool? CoilsAllowed { get; set; }
    public bool? UseHioIoForDiscovery { get; set; }
}

internal sealed class Layout3EquipmentIdentificationConfiguration
{
    public string? ExpectedFirmwareFamily { get; set; }
    public string? ExpectedFirmwareVersion { get; set; }
    public int? ExpectedProgramId { get; set; }
    public int? ExpectedProgramCrc { get; set; }
    public string? ExpectedController { get; set; }
    public string? ExpectedCpu { get; set; }
    public string? ExpectedModule { get; set; }
    public int? ExpectedModuleSlot { get; set; }
    public List<Layout3IdentificationRegisterConfiguration> Registers { get; set; } = [];
    public List<Layout3F21BitConfiguration> CriticalF21Bits { get; set; } = [];
}

internal sealed class Layout3IdentificationRegisterConfiguration
{
    public string? Name { get; set; }
    public string? SymbolicReference { get; set; }
    public int? DocumentedReference { get; set; }
    public int? ProtocolDataAddress { get; set; }
    public string? Access { get; set; }
    public string? Status { get; set; }
}

internal sealed class Layout3F21BitConfiguration
{
    public int? Bit { get; set; }
    public string? Name { get; set; }
    public bool? Critical { get; set; }
}

internal sealed class Layout3BenchReadPolicyConfiguration
{
    public string? RegisterMapReference { get; set; }
    public int MaximumReads { get; set; }
    public List<Layout3BenchAllowedRegister> AllowedRegisters { get; set; } = [];
}

internal sealed class Layout3BenchAllowedRegister
{
    public string? Name { get; set; }
    public string? SymbolicReference { get; set; }
    public int? DocumentedReference { get; set; }
    public string? Mode { get; set; }
    public string? Access { get; set; }
    public string? ApprovalEvidence { get; set; }
}

internal sealed class Layout3BenchOutputPolicyConfiguration
{
    public bool? Enabled { get; set; }
    public bool? GenericAddressApiExposed { get; set; }
    public bool? OneOutputAtATime { get; set; }
    public bool? ExplicitCommandRequired { get; set; }
    public bool? MomentaryModeRequired { get; set; }
    public int? MaximumActivationDurationMilliseconds { get; set; }
    public bool? CancellationRequired { get; set; }
    public bool? TurnOffAtEndRequired { get; set; }
    public bool? ValidateReturnRequired { get; set; }
    public bool? PwmBlocked { get; set; }
    public bool? ReservedRegistersBlocked { get; set; }
    public bool? ArbitraryRegistersBlocked { get; set; }
    public List<Layout3BenchAllowedOutput> AllowedOutputs { get; set; } = [];
    public List<Layout3BenchBlockedRegister> BlockedRegisters { get; set; } = [];
}

internal sealed class Layout3BenchAllowedOutput
{
    public string? Channel { get; set; }
    public string? SymbolicReference { get; set; }
    public int? DocumentedReference { get; set; }
    public string? Access { get; set; }
    public string? ApprovalEvidence { get; set; }
}

internal sealed class Layout3BenchBlockedRegister
{
    public string? Name { get; set; }
    public string? SymbolicReference { get; set; }
    public int? DocumentedReference { get; set; }
    public string? Reason { get; set; }
}

internal sealed class Layout3BenchSafetyConfiguration
{
    public bool? FeatureEnabled { get; set; }
    public bool? RealCommunicationEnabled { get; set; }
    public bool? WritesEnabled { get; set; }
    public bool? OutputModeEnabled { get; set; }
    public bool? PollingEnabled { get; set; }
    public bool? AutomaticReconnectEnabled { get; set; }
    public bool? SingleShotOnly { get; set; }
    public bool? OfflineGateDAuthorized { get; set; }
    public bool? PhysicalTransportAuthorized { get; set; }
    public bool? PhysicalOutputGateAuthorized { get; set; }
}

internal sealed class Layout3BenchConditionsConfiguration
{
    public string? BackupReference { get; set; }
    public string? BackupSha256 { get; set; }
    public string? SupplyVoltage { get; set; }
    public string? MeasuredVoltageEvidence { get; set; }
    public string? Responsible { get; set; }
    public string? MachineState { get; set; }
    public bool? GroundingConfirmed { get; set; }
    public bool? ChannelIsolated { get; set; }
    public bool? OutputsDeenergizedOrIsolated { get; set; }
    public bool? MachinePreventedFromOperating { get; set; }
    public bool? EmergencyStopIdentified { get; set; }
    public bool? QuickDisconnectDefined { get; set; }
    public Layout3ResponsibleDeclarations? ResponsibleDeclarations { get; set; }
}

internal sealed class Layout3ResponsibleDeclarations
{
    public string? DeclaredBy { get; set; }
    public string? EvidenceStatus { get; set; }
    public string? EvidenceReference { get; set; }
    public bool? ResponsiblePresent { get; set; }
    public bool? GroundingOk { get; set; }
    public bool? OutputsDeenergizedOrIsolatedOk { get; set; }
    public bool? MachinePreventedFromOperatingOk { get; set; }
    public bool? MachineSafeStateOk { get; set; }
    public bool? EmergencyStopOk { get; set; }
    public bool? QuickDisconnectOk { get; set; }
    public bool? ProgramBackupOk { get; set; }
}

internal sealed record Layout3BenchReadinessEvaluationResult(IReadOnlyList<string> Failures)
{
    public bool IsReady => Failures.Count == 0;
}
