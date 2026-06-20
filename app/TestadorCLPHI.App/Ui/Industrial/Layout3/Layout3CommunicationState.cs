namespace TestadorCLPHI.App.Ui.Industrial.Layout3;

/// <summary>
/// Fotografia imutavel do estado visual de comunicacao do host read-only.
/// O estado e derivado apenas do catalogo em memoria e nunca inicia transporte,
/// tentativa real ou comando fisico.
/// </summary>
internal sealed record Layout3CommunicationState(
    Layout3CommunicationStatus Status,
    string Origin,
    string ProfileProtocol,
    string OperationalMessage,
    DateTime LastUpdatedAt,
    int RealConnectionAttempts,
    int PhysicalCommandsExecuted)
{
    public string StatusDisplayName => Status.ToDisplayName();

    public string LastUpdatedDisplay => LastUpdatedAt.ToString("dd/MM/yyyy HH:mm:ss");

    public static Layout3CommunicationState FromSelection(
        Layout3ProfileSelection? selection,
        DateTime? updatedAt = null)
    {
        Layout3ProfileSelection current = selection ?? Layout3ProfileSelection.Empty;
        bool hasHardware = current.Family is not null;
        bool hasCommunicationProfile = current.Communication is not null;

        return new Layout3CommunicationState(
            Status: hasHardware
                ? Layout3CommunicationStatus.LocalReadOnly
                : Layout3CommunicationStatus.NotConnected,
            Origin: hasCommunicationProfile
                ? "Catalogo local"
                : hasHardware ? "Host local" : "Sem hardware",
            ProfileProtocol: FormatProfileProtocol(current),
            OperationalMessage: hasCommunicationProfile
                ? "Informacao do perfil; nenhuma conexao aberta."
                : hasHardware
                    ? "Perfil local sem protocolo; nenhuma conexao aberta."
                    : "Sem hardware; host local desconectado.",
            LastUpdatedAt: updatedAt ?? DateTime.Now,
            RealConnectionAttempts: 0,
            PhysicalCommandsExecuted: 0);
    }

    public Layout3CommunicationState AsBlocked(string message, DateTime? updatedAt = null)
    {
        return this with
        {
            Status = Layout3CommunicationStatus.Blocked,
            OperationalMessage = message,
            LastUpdatedAt = updatedAt ?? DateTime.Now,
            RealConnectionAttempts = 0,
            PhysicalCommandsExecuted = 0
        };
    }

    private static string FormatProfileProtocol(Layout3ProfileSelection selection)
    {
        if (selection.Communication is null)
        {
            return "Nao disponivel";
        }

        string protocol = string.IsNullOrWhiteSpace(selection.Communication.Protocol)
            ? string.Empty
            : $"protocolo: {selection.Communication.Protocol}";
        string[] details = [selection.Communication.DisplayName, protocol];

        string[] availableDetails = details
            .Where(value => !string.IsNullOrWhiteSpace(value))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();
        return availableDetails.Length == 0
            ? "Nao disponivel"
            : string.Join(" | ", availableDetails);
    }
}
