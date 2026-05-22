namespace TestadorCLPHI.App.Ui;

public interface IDigitalIoManualPanel
{
    event Action<int>? OutputCommandClicked;

    event EventHandler? RefreshInputsClicked;

    void SetInputState(int channel, bool active);

    void SetPanelEnabled(bool enabled);
}
