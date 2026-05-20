using System.IO.Ports;
using TestadorCLPHI.App.Plc;
using TestadorCLPHI.App.Ui;
using TestadorCLPHI.App.Ui.Controls;

namespace TestadorCLPHI.App;

internal sealed class MainFormCommandUiService
{
    private readonly IWin32Window _owner;
    private readonly PlcRegisterCommandService _registerCommandService;
    private readonly IPlcCommunicationService _plcService;
    private readonly Func<bool> _tryUpdateConnectionSettingsFromUi;
    private readonly Action _updateConnectionState;

    public MainFormCommandUiService(
        IWin32Window owner,
        PlcRegisterCommandService registerCommandService,
        IPlcCommunicationService plcService,
        Func<bool> tryUpdateConnectionSettingsFromUi,
        Action updateConnectionState)
    {
        _owner = owner;
        _registerCommandService = registerCommandService;
        _plcService = plcService;
        _tryUpdateConnectionSettingsFromUi = tryUpdateConnectionSettingsFromUi;
        _updateConnectionState = updateConnectionState;
    }

    public async System.Threading.Tasks.Task ExecutarComandoRegistradorAsync(
        int registerAddress,
        ushort valueToWrite,
        string commandName,
        string dialogTitle,
        string errorTitle)
    {
        if (!_tryUpdateConnectionSettingsFromUi())
        {
            return;
        }

        try
        {
            PlcRegisterCommandResult result =
                await _registerCommandService.WriteAndReadBackAsync(
                    registerAddress,
                    valueToWrite);

            _updateConnectionState();

            MessageBox.Show(
                _owner,
                $"{commandName} aplicado: %MW{result.RegisterAddress} = {result.ReadBackValue}",
                dialogTitle,
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
        }
        catch (Exception ex)
        {
            _plcService.State.SetError(ex.Message);
            _updateConnectionState();

            MessageBox.Show(
                _owner,
                ex.Message,
                errorTitle,
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
        }
    }
}
