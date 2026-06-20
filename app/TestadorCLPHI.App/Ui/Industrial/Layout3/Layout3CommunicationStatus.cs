namespace TestadorCLPHI.App.Ui.Industrial.Layout3;

/// <summary>
/// Estados exclusivamente locais exibidos pelo host Layout 3 read-only.
/// Nenhum valor representa um canal fisico aberto.
/// </summary>
internal enum Layout3CommunicationStatus
{
    NotConnected,
    LocalReadOnly,
    Simulated,
    Blocked
}

internal static class Layout3CommunicationStatusExtensions
{
    public static string ToDisplayName(this Layout3CommunicationStatus status)
    {
        return status switch
        {
            Layout3CommunicationStatus.NotConnected => "Nao conectada",
            Layout3CommunicationStatus.LocalReadOnly => "Local read-only",
            Layout3CommunicationStatus.Simulated => "Simulada",
            Layout3CommunicationStatus.Blocked => "Bloqueada",
            _ => throw new ArgumentOutOfRangeException(nameof(status), status, null)
        };
    }
}
