namespace TestadorCLPHI.App.Ui.Industrial.Layout3;

/// <summary>
/// Decisao imutavel do gate de ativacao do bridge de leitura do host Layout 3.
///
/// A decisao e puramente local e derivada do catalogo/selecao em memoria. Por
/// construcao ela declara a ativacao bloqueada, sem conexao ativa e com leituras
/// reais, escritas reais e comandos fisicos sempre em 0, alem de listar os
/// requisitos futuros ainda pendentes. Nenhum campo aqui inicia transporte, abre
/// porta, fala protocolo de campo ou executa comando fisico.
/// </summary>
internal sealed record Layout3ReadBridgeActivationDecision(
    Layout3ReadBridgeActivationStatus Status,
    bool ActivationAllowed,
    bool ConnectionActive,
    int RealReads,
    int RealWrites,
    int PhysicalCommands,
    string ProfileContext,
    string ProtocolContext,
    IReadOnlyList<Layout3ReadBridgeActivationRequirement> Requirements,
    string Message)
{
    public const string BlockedMessage =
        "Ativacao bloqueada nesta fase: nenhum perfil libera conexao ou leitura real.";

    public string StatusDisplayName => Status.ToDisplayName();

    public string ActivationAllowedDisplay => ActivationAllowed ? "Sim" : "Nao";

    public string ConnectionActiveDisplay => ConnectionActive ? "Sim" : "Nao";

    public int PendingRequirements => Requirements.Count(requirement => !requirement.Satisfied);

    public int SatisfiedRequirements => Requirements.Count(requirement => requirement.Satisfied);

    /// <summary>
    /// Requisitos futuros que precisariam ser cumpridos antes da primeira leitura
    /// real. Sao declarativos e nascem todos pendentes nesta fase; satisfaze-los de
    /// verdade exigira milestone futura com aprovacao explicita.
    /// </summary>
    public static IReadOnlyList<Layout3ReadBridgeActivationRequirement> FutureRequirements { get; } =
    [
        Layout3ReadBridgeActivationRequirement.Pending(
            "Aprovacao explicita de milestone de leitura real",
            "Abertura de milestone dedicada, fora da Fase 3 segura, com autorizacao registrada."),
        Layout3ReadBridgeActivationRequirement.Pending(
            "Referencia oficial dos modelos pendentes validada",
            "Confirmar dados oficiais de NEON_5_CONTROLLER e RION_5_CONTROLLER antes de habilitar."),
        Layout3ReadBridgeActivationRequirement.Pending(
            "Bancada fisica homologada e isolada",
            "Ambiente de teste fisico controlado, com isolamento eletrico e parada de emergencia."),
        Layout3ReadBridgeActivationRequirement.Pending(
            "Camada de transporte real auditada",
            "Implementacao de transporte revisada e auditada em milestone propria, ainda inexistente."),
        Layout3ReadBridgeActivationRequirement.Pending(
            "Procedimento de leitura somente leitura revisado",
            "Revisao do fluxo de leitura para garantir zero escrita e zero comando fisico.")
    ];

    /// <summary>
    /// Cria a decisao bloqueada a partir de uma selecao local de perfil. A selecao
    /// entra apenas como contexto informativo (perfil e protocolo do catalogo); ela
    /// jamais libera ativacao, conexao ou leitura.
    /// </summary>
    public static Layout3ReadBridgeActivationDecision Blocked(Layout3ProfileSelection? selection)
    {
        Layout3ProfileSelection current = selection ?? Layout3ProfileSelection.Empty;
        bool hasHardware = current.Family is not null;

        string profileContext = hasHardware
            ? $"{current.FamilyName} / {current.ModelName}"
            : "Sem hardware";
        string protocolContext = FormatProtocolContext(current);

        return new Layout3ReadBridgeActivationDecision(
            Status: Layout3ReadBridgeActivationStatus.Blocked,
            ActivationAllowed: false,
            ConnectionActive: false,
            RealReads: 0,
            RealWrites: 0,
            PhysicalCommands: 0,
            ProfileContext: profileContext,
            ProtocolContext: protocolContext,
            Requirements: FutureRequirements,
            Message: BlockedMessage);
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
