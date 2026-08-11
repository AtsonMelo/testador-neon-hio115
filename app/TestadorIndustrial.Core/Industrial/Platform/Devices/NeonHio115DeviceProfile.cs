namespace TestadorCLPHI.App.Industrial.Platform.Devices;

internal static class NeonHio115DeviceProfile
{
    internal const string ProfileId = "NEON5_CPU450_HIO115";

    internal static IndustrialDeviceProfile Current { get; } = Create();

    private static IndustrialDeviceProfile Create()
    {
        DeviceIdentificationPolicy identification = new(
            IsSupported: true,
            ExpectedFirmwareFamily: "G5PLC.C950.ST",
            ExpectedFirmwareVersion: "3.3.11",
            Probes:
            [
                new("PROG_ID", 30012, DeviceIdentificationProbeRole.ProgramId, ExpectedValue: 31134),
                new("PROG_CRC", 30013, DeviceIdentificationProbeRole.ProgramCrc, ExpectedValue: 23248),
                new(
                    "DEV_GFAIL_STS",
                    30021,
                    DeviceIdentificationProbeRole.GeneralFailureStatus,
                    CriticalMask: (ushort)(
                        (1 << 0) | (1 << 1) | (1 << 2) | (1 << 3) |
                        (1 << 8) | (1 << 9) | (1 << 10) | (1 << 11) |
                        (1 << 12) | (1 << 13) | (1 << 14)))
            ],
            IdentifiedDetail: "Programa conhecido; F10/F11 permanecem opcionais ate mapeamento confirmado.",
            SignatureMismatchDetail: "Resposta RTU valida, assinatura de programa divergente.",
            CriticalFaultDetail: "F21 critico.");

        DeviceInputMap inputMap = new(
        [
            new(
                DeviceInputKind.Digital,
                "DI00..DI07",
                Enumerable.Range(0, 8)
                    .Select(channel => new DeviceRegisterPoint(
                        channel,
                        $"DI{channel:00}",
                        checked((ushort)(31120 + channel))))
                    .ToArray()),
            new(
                DeviceInputKind.Analog,
                "AI00..AI02",
                Enumerable.Range(0, 3)
                    .Select(channel => new DeviceRegisterPoint(
                        channel,
                        $"AI{channel:00}",
                        checked((ushort)(31132 + channel))))
                    .ToArray())
        ]);

        DeviceOutputMap outputMap = new(
            Enumerable.Range(0, 4)
                .Select(channel => new DeviceRegisterPoint(
                    channel,
                    $"DO{channel:00}",
                    checked((ushort)(31128 + channel))))
                .ToArray(),
            [
                new("RESERVED_31137", 31137, "reserved"),
                new("RESERVED_31140", 31140, "reserved"),
                new("RESERVED_31143", 31143, "reserved"),
                new("PWM_FREQUENCY", 31144, "pwm"),
                new("PWM_DUTY", 31145, "pwm")
            ]);

        return new(
            Id: ProfileId,
            DisplayName: "NEON5-1S / CPU450 / HIO115",
            Family: "NEON5",
            Model: "NEON5-1S",
            ControllerCpu: "CPU450",
            IoModule: "HIO115",
            IdentificationPolicy: identification,
            InputMap: inputMap,
            OutputMap: outputMap,
            Limits: new(
                DigitalInputCount: 8,
                AnalogInputCount: 3,
                DigitalOutputCount: 4,
                MaximumSimulatedOutputDuration: TimeSpan.FromSeconds(3)),
            Capabilities: new(
                IdentificationSupported: true,
                DigitalInputReadSupported: true,
                AnalogInputReadSupported: true,
                SupervisedOutputTestSupported: true,
                SimulationSupported: true),
            SupportStatus: IndustrialDeviceSupportStatus.SimulationOnly,
            PhysicalSupport: false,
            LogIdentity: "NEON5-CPU450-HIO115",
            ProtocolAdapterId: "MODBUS_RTU");
    }
}
