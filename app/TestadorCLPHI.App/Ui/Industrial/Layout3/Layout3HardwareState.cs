using TestadorCLPHI.App.Hardware;

namespace TestadorCLPHI.App.Ui.Industrial.Layout3;

internal sealed record Layout3HardwareState(
    bool LocalCatalogAvailable,
    int FamilyCount,
    int ModelCount,
    int IoModuleCount,
    string Source)
{
    public static Layout3HardwareState FromLocalCatalog(HardwareCatalog catalog)
    {
        return new Layout3HardwareState(
            LocalCatalogAvailable: catalog.Families.Count > 0,
            FamilyCount: catalog.Families.Count,
            ModelCount: catalog.Models.Count,
            IoModuleCount: catalog.IoModules.Count,
            Source: "Catalogo local em memoria");
    }
}
