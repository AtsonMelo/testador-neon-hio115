namespace TestadorCLPHI.App.Ui.Industrial.Layout3;

/// <summary>
/// Fotografia imutavel do bridge de leitura do host Layout 3.
///
/// O snapshot e puramente local e derivado do catalogo/selecao em memoria. Por
/// construcao ele declara o bridge desligado, sem conexao ativa e com leituras
/// reais, escritas reais e comandos fisicos sempre em 0. Nenhum campo aqui inicia
/// transporte, abre porta, fala protocolo de campo ou executa comando fisico.
/// </summary>
internal sealed record Layout3ReadBridgeSnapshot(
    Layout3ReadBridgeStatus Status,
    string Mode,
    string Origin,
    bool ConnectionActive,
    int RealReads,
    int RealWrites,
    int PhysicalCommands,
    string ProfileContext,
    string ProtocolContext,
    string Message)
{
    public const string DisabledMode = "disabled / no-op / local";

    public const string PreparedOrigin = "Arquitetura preparada";

    public const string DisabledMessage =
        "Bridge preparado, mas bloqueado/desligado nesta fase.";

    public string StatusDisplayName => Status.ToDisplayName();

    public string ConnectionActiveDisplay => ConnectionActive ? "Sim" : "Nao";

    /// <summary>
    /// Cria o snapshot desligado/no-op a partir de uma selecao local de perfil.
    /// A selecao entra apenas como contexto informativo (perfil e protocolo do
    /// catalogo); ela jamais habilita conexao ou leitura.
    /// </summary>
    public static Layout3ReadBridgeSnapshot Disabled(Layout3ProfileSelection? selection)
    {
        Layout3ProfileSelection current = selection ?? Layout3ProfileSelection.Empty;
        bool hasHardware = current.Family is not null;

        string profileContext = hasHardware
            ? $"{current.FamilyName} / {current.ModelName}"
            : "Sem hardware";
        string protocolContext = FormatProtocolContext(current);

        return new Layout3ReadBridgeSnapshot(
            Status: Layout3ReadBridgeStatus.Disabled,
            Mode: DisabledMode,
            Origin: PreparedOrigin,
            ConnectionActive: false,
            RealReads: 0,
            RealWrites: 0,
            PhysicalCommands: 0,
            ProfileContext: profileContext,
            ProtocolContext: protocolContext,
            Message: DisabledMessage);
    }

    private static string FormatProtocolContext(Layout3ProfileSelection selection)
    {
        if (selection.Communication is null)
        {
            return "Nao disponivel";
        }

        string protocol = selection.Communication.Protocol;
        if (!string.IsNullOrWhiteSpace(protocol))
        {
            return protocol;
        }

        return string.IsNullOrWhiteSpace(selection.Communication.DisplayName)
            ? "Nao disponivel"
            : selection.Communication.DisplayName;
    }
}
