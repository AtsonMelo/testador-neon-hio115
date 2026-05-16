namespace TestadorCLPHI.App.Plc;

public enum PlcConnectionStatus
{
    Disconnected,
    Connecting,
    Connected,
    Error
}

public sealed class PlcConnectionState
{
    public PlcConnectionStatus Status { get; private set; } = PlcConnectionStatus.Disconnected;

    public string Message { get; private set; } = "Desconectado";

    public DateTime LastUpdatedAt { get; private set; } = DateTime.Now;

    public bool IsConnected => Status == PlcConnectionStatus.Connected;

    public void SetDisconnected(string message = "Desconectado")
    {
        Update(PlcConnectionStatus.Disconnected, message);
    }

    public void SetConnecting(string message = "Conectando...")
    {
        Update(PlcConnectionStatus.Connecting, message);
    }

    public void SetConnected(string message = "Conectado")
    {
        Update(PlcConnectionStatus.Connected, message);
    }

    public void SetError(string message)
    {
        Update(PlcConnectionStatus.Error, message);
    }

    private void Update(PlcConnectionStatus status, string message)
    {
        Status = status;
        Message = message;
        LastUpdatedAt = DateTime.Now;
    }
}
