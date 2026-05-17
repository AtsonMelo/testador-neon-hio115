namespace TestadorCLPHI.App.Plc;

public sealed record PlcAutoDetectionResult(
    PlcConnectionSettings Settings,
    ushort ProbeRegisterValue,
    int AttemptCount);


