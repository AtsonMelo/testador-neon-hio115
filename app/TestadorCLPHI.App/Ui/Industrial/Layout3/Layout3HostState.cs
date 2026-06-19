using TestadorCLPHI.App.Hardware;

namespace TestadorCLPHI.App.Ui.Industrial.Layout3;

internal sealed record Layout3HostState(
    Layout3ReadOnlyMode Mode,
    Layout3HardwareState Hardware,
    Layout3IoState Io,
    string CommunicationStatus,
    int PhysicalCommandsExecuted,
    IReadOnlyList<string> LocalEvents)
{
    public static Layout3HostState CreateInitial(HardwareCatalog catalog)
    {
        Layout3HardwareState hardware = Layout3HardwareState.FromLocalCatalog(catalog);
        string catalogEvent = hardware.LocalCatalogAvailable
            ? $"Catalogo local carregado: {hardware.FamilyCount} familia(s), {hardware.ModelCount} modelo(s)."
            : "Catalogo local indisponivel; host mantido em estado seguro.";

        return new Layout3HostState(
            Mode: Layout3ReadOnlyMode.Active,
            Hardware: hardware,
            Io: Layout3IoState.Disconnected,
            CommunicationStatus: "Nao conectada",
            PhysicalCommandsExecuted: 0,
            LocalEvents:
            [
                "Host Layout 3 iniciado em modo read-only.",
                catalogEvent,
                "Nenhuma comunicacao fisica foi criada.",
                "Comandos fisicos executados: 0."
            ]);
    }
}
