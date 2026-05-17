using System.IO.Ports;

namespace TestadorCLPHI.App.Plc;

public sealed class PlcConnectionSettings
{
    public string PortName { get; set; } = "COM1";

    public int BaudRate { get; set; } = 9600;

    public Parity Parity { get; set; } = Parity.None;

    public int DataBits { get; set; } = 8;

    public StopBits StopBits { get; set; } = StopBits.One;

    public byte SlaveId { get; set; } = 1;

    public int TimeoutMilliseconds { get; set; } = 1000;
}
