namespace TestadorCLPHI.App.Ui.Industrial.Layout3;

internal sealed record Layout3ReadOnlyMode(
    string DisplayName,
    string SafetyMessage,
    bool AllowsPhysicalWrites)
{
    public static Layout3ReadOnlyMode Active { get; } = new(
        "MODO READ-ONLY",
        "SEM ESCRITA FISICA",
        AllowsPhysicalWrites: false);
}
