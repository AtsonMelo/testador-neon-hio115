using TestadorCLPHI.App.Hardware;

namespace TestadorCLPHI.App.Ui.Industrial.Layout3;

internal sealed record Layout3HostState(
    Layout3ReadOnlyMode Mode,
    Layout3HardwareState Hardware,
    Layout3IoState Io,
    Layout3ProfileSelection Profile,
    string CommunicationStatus,
    int PhysicalCommandsExecuted,
    IReadOnlyList<string> LocalEvents)
{
    public static Layout3HostState CreateInitial(HardwareCatalog catalog)
    {
        Layout3HardwareState hardware = Layout3HardwareState.FromLocalCatalog(catalog);
        Layout3ProfileSelection profile = Layout3ProfileSelection.CreateDefault(catalog);
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
            CommunicationStatus: "Nao conectada",
            PhysicalCommandsExecuted: 0,
            LocalEvents:
            [
                "Host Layout 3 iniciado em modo read-only.",
                catalogEvent,
                profileEvent,
                "Nenhuma comunicacao fisica foi criada.",
                "Comandos fisicos executados: 0."
            ]);
    }
}
