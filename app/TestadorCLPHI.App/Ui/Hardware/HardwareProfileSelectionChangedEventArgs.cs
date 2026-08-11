using TestadorCLPHI.App.Hardware;

namespace TestadorCLPHI.App.Ui.Hardware;

public sealed class HardwareProfileSelectionChangedEventArgs : EventArgs
{
    public HardwareProfileSelectionChangedEventArgs(SelectedHardwareProfile selectedProfile)
    {
        SelectedProfile = selectedProfile;
    }

    public SelectedHardwareProfile SelectedProfile { get; }
}
