namespace TestadorCLPHI.App.Ui.Industrial.Layout3;

/// <summary>
/// Descreve um cenario nao visual de verificacao de seguranca do host Layout 3
/// read-only. Cada cenario aponta para uma origem do catalogo local (familia) ou
/// para a ausencia de hardware. Nenhum cenario abre transporte, porta fisica ou
/// canal real: a selecao de perfil e derivada apenas do catalogo em memoria.
/// </summary>
internal sealed record Layout3HostReadOnlySafetyValidationScenario(
    string Name,
    string? FamilyId,
    bool ExpectsHardware,
    bool PendingOfficialReference)
{
    /// <summary>
    /// Cenario sem hardware: selecao vazia, host local desconectado.
    /// </summary>
    public static Layout3HostReadOnlySafetyValidationScenario NoHardware { get; } = new(
        Name: "Sem hardware (selecao vazia)",
        FamilyId: null,
        ExpectsHardware: false,
        PendingOfficialReference: false);

    public bool IsNoHardware => FamilyId is null;
}
