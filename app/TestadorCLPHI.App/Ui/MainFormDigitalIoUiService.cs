using TestadorCLPHI.App.Plc;
using TestadorCLPHI.App.Ui;

namespace TestadorCLPHI.App;

internal sealed class MainFormDigitalIoUiService
{
    private readonly IWin32Window _owner;
    private readonly PlcDigitalIoManualService _digitalIoManualService;
    private readonly IPlcCommunicationService _plcService;
    private readonly IDigitalIoManualPanel _digitalIoManualPanel;
    private readonly Label _statusLabel;
    private readonly Func<bool> _tryUpdateConnectionSettingsFromUi;
    private readonly Action _updateConnectionState;

    public MainFormDigitalIoUiService(
        IWin32Window owner,
        PlcDigitalIoManualService digitalIoManualService,
        IPlcCommunicationService plcService,
        IDigitalIoManualPanel digitalIoManualPanel,
        Label statusLabel,
        Func<bool> tryUpdateConnectionSettingsFromUi,
        Action updateConnectionState)
    {
        _owner = owner;
        _digitalIoManualService = digitalIoManualService;
        _plcService = plcService;
        _digitalIoManualPanel = digitalIoManualPanel;
        _statusLabel = statusLabel;
        _tryUpdateConnectionSettingsFromUi = tryUpdateConnectionSettingsFromUi;
        _updateConnectionState = updateConnectionState;
    }

    public async System.Threading.Tasks.Task AcionarSaidaDigitalManualAsync(int channel)
    {
        if (!_tryUpdateConnectionSettingsFromUi())
        {
            return;
        }

        _digitalIoManualPanel.SetPanelEnabled(false);

        try
        {
            await _digitalIoManualService.ActivateManualOutputAsync(channel);
            await AtualizarEntradasDigitaisAsync();

            _statusLabel.Text =
                $"Saída D{channel:000} acionada em modo manual. Retornos esperados atualizados.";
        }
        catch (Exception ex)
        {
            _plcService.State.SetError(ex.Message);
            _updateConnectionState();

            MessageBox.Show(
                _owner,
                ex.Message,
                $"Erro ao acionar D{channel:000}",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
        }
        finally
        {
            _digitalIoManualPanel.SetPanelEnabled(true);
        }
    }

    public async System.Threading.Tasks.Task AtualizarEntradasDigitaisAsync()
    {
        if (!_tryUpdateConnectionSettingsFromUi())
        {
            return;
        }

        _digitalIoManualPanel.SetPanelEnabled(false);

        try
        {
            bool[] inputStates =
                await _digitalIoManualService.ReadDigitalInputsAsync();

            for (int index = 0; index < inputStates.Length; index++)
            {
                _digitalIoManualPanel.SetInputState(
                    index,
                    inputStates[index]);
            }

            _updateConnectionState();

            _statusLabel.Text = "Entradas digitais DI00 a DI07 atualizadas.";
        }
        catch (Exception ex)
        {
            _plcService.State.SetError(ex.Message);
            _updateConnectionState();

            MessageBox.Show(
                _owner,
                ex.Message,
                "Erro ao atualizar entradas digitais",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
        }
        finally
        {
            _digitalIoManualPanel.SetPanelEnabled(true);
        }
    }
}
