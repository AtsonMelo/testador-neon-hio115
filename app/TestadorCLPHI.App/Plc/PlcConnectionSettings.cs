using TestadorCLPHI.App.Industrial.Transport;

namespace TestadorCLPHI.App.Plc;

public sealed class PlcConnectionSettings
{
    public string PortName { get; set; } = "COM1";

    public int BaudRate { get; set; } = 9600;

    internal IndustrialSerialParity Parity { get; set; } = IndustrialSerialParity.None;

    public int DataBits { get; set; } = 8;

    internal IndustrialSerialStopBits StopBits { get; set; } = IndustrialSerialStopBits.One;

    public byte SlaveId { get; set; } = 1;

    public int TimeoutMilliseconds { get; set; } = 1000;
}
