namespace TestadorCLPHI.App.Ui.Industrial.Layout3;

/// <summary>
/// Estado do bridge de leitura do host Layout 3.
///
/// Nesta fase (3.5) o bridge e apenas contrato/estrutura: o unico estado possivel
/// e <see cref="Disabled"/>. Estados ativos so poderao existir em uma milestone
/// futura, com aprovacao explicita; este enum permanece extensivel para esse fim,
/// mas nenhum valor aqui representa um canal real aberto.
/// </summary>
internal enum Layout3ReadBridgeStatus
{
    /// <summary>Bridge desligado/bloqueado: sem conexao, sem leitura, sem comando.</summary>
    Disabled
}

internal static class Layout3ReadBridgeStatusExtensions
{
    public static string ToDisplayName(this Layout3ReadBridgeStatus status)
    {
        return status switch
        {
            Layout3ReadBridgeStatus.Disabled => "Desligado",
            _ => throw new ArgumentOutOfRangeException(nameof(status), status, null)
        };
    }
}
