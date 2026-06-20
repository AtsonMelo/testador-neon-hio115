namespace TestadorCLPHI.App.Ui.Industrial.Layout3;

/// <summary>
/// Estado do gate de ativacao do bridge de leitura do host Layout 3.
///
/// Nesta fase (3.6) o gate e apenas contrato/estrutura de decisao: o unico estado
/// possivel e <see cref="Blocked"/>. Um estado liberado so podera existir em uma
/// milestone futura, com aprovacao explicita e requisitos satisfeitos; este enum
/// permanece extensivel para esse fim, mas nenhum valor aqui autoriza ativacao,
/// conexao ou leitura real.
/// </summary>
internal enum Layout3ReadBridgeActivationStatus
{
    /// <summary>Ativacao bloqueada: nenhum perfil libera conexao, leitura ou comando.</summary>
    Blocked
}

internal static class Layout3ReadBridgeActivationStatusExtensions
{
    public static string ToDisplayName(this Layout3ReadBridgeActivationStatus status)
    {
        return status switch
        {
            Layout3ReadBridgeActivationStatus.Blocked => "Bloqueado",
            _ => throw new ArgumentOutOfRangeException(nameof(status), status, null)
        };
    }
}
