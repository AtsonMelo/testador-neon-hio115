using TestadorCLPHI.App.Hardware;

namespace TestadorCLPHI.App.Ui.Industrial.Layout3;

internal sealed record Layout3HostState(
    Layout3ReadOnlyMode Mode,
    Layout3HardwareState Hardware,
    Layout3IoState Io,
    Layout3ProfileSelection Profile,
    Layout3CommunicationState Communication,
    int PhysicalCommandsExecuted,
    IReadOnlyList<string> LocalEvents)
{
    public static Layout3HostState CreateInitial(HardwareCatalog catalog)
    {
        Layout3HardwareState hardware = Layout3HardwareState.FromLocalCatalog(catalog);
        Layout3ProfileSelection profile = Layout3ProfileSelection.CreateDefault(catalog);
        Layout3CommunicationState communication = Layout3CommunicationState.FromSelection(profile);
        string catalogEvent = hardware.LocalCatalogAvailable
            ? $"Catalogo local carregado: {hardware.FamilyCount} familia(s), {hardware.ModelCount} modelo(s)."
            : "Catalogo local indisponivel; host mantido em estado seguro.";
        string profileEvent = hardware.LocalCatalogAvailable
            ? $"Perfil local inicial: {profile.FamilyName} / {profile.ModelName} / {profile.ModuleName}."
            : "Nenhum perfil local disponivel para selecao.";

        return new Layout3HostState(
            Mode: Layout3ReadOnlyMode.Active,
            Hardware: hardware,
            Io: Layout3IoState.Disconnected,
            Profile: profile,
            Communication: communication,
            PhysicalCommandsExecuted: 0,
            LocalEvents:
            [
                "Host Layout 3 iniciado em modo read-only.",
                catalogEvent,
                profileEvent,
                $"Comunicacao local: {communication.StatusDisplayName}; origem {communication.Origin}; " +
                    $"perfil {communication.ProfileProtocol}.",
                "Nenhuma conexao fisica foi criada; tentativas reais: 0.",
                "Comandos fisicos executados: 0."
            ]);
    }
}
