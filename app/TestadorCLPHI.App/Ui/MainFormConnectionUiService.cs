using TestadorCLPHI.App.Plc;
using TestadorCLPHI.App.Ui.Controls;

namespace TestadorCLPHI.App.Ui;

internal static class MainFormConnectionUiService
{
    public static void AtualizarListaDePortas(
        ComboBox portaComboBox,
        PlcConnectionSettings connectionSettings)
    {
        PlcConnectionSettingsUiService.UpdatePortList(
            portaComboBox,
            connectionSettings.PortName);
    }

    public static void AtualizarResumoConexao(
        Label conexaoResumoLabel,
        PlcConnectionSettings connectionSettings)
    {
        conexaoResumoLabel.Text =
            $"Atual: {connectionSettings.PortName}, {connectionSettings.BaudRate} bps, Slave {connectionSettings.SlaveId}" +
            $"{Environment.NewLine}Paridade: {connectionSettings.Parity}, Stop bits: {connectionSettings.StopBits}, Timeout: {connectionSettings.TimeoutMilliseconds} ms";
    }

    public static void AtualizarEstadoConexao(
        ConnectionStatePanelControl connectionStatePanel,
        PlcConnectionState state)
    {
        connectionStatePanel.UpdateState(state);
    }

    public static int[] ObterBaudRatesSelecionadosParaBusca(
        ComboBox baudRateComboBox,
        CheckedListBox baudRateBuscaCheckedListBox,
        PlcConnectionSettings connectionSettings)
    {
        return PlcConnectionSettingsUiService.GetSelectedBaudRatesForSearch(
            baudRateComboBox,
            baudRateBuscaCheckedListBox,
            connectionSettings.BaudRate);
    }
}
