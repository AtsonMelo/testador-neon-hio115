using System.IO.Ports;
using TestadorCLPHI.App.Plc;

namespace TestadorCLPHI.App.Ui;

public static class PlcConnectionSettingsUiService
{
    public static void UpdatePortList(ComboBox portComboBox, string currentPortName)
    {
        string selectedPort = portComboBox.SelectedItem?.ToString()
            ?? currentPortName;

        string[] ports = SerialPort.GetPortNames()
            .OrderBy(port => port)
            .ToArray();

        portComboBox.Items.Clear();

        foreach (string port in ports)
        {
            portComboBox.Items.Add(port);
        }

        if (portComboBox.Items.Count == 0)
        {
            portComboBox.Items.Add(currentPortName);
        }

        int index = portComboBox.Items.IndexOf(selectedPort);

        if (index < 0)
        {
            index = 0;
        }

        portComboBox.SelectedIndex = index;
    }

    public static bool TryUpdateSettingsFromUi(
        ComboBox portComboBox,
        ComboBox baudRateComboBox,
        TextBox slaveIdTextBox,
        PlcConnectionSettings settings,
        out string? errorMessage)
    {
        errorMessage = null;

        if (portComboBox.SelectedItem is null)
        {
            errorMessage = "Selecione uma porta COM.";
            return false;
        }

        if (!int.TryParse(baudRateComboBox.Text, out int baudRate) || baudRate <= 0)
        {
            errorMessage = "Informe um baud rate válido. Exemplo: 9600.";
            return false;
        }

        if (!byte.TryParse(slaveIdTextBox.Text, out byte slaveId) || slaveId is < 1 or > 247)
        {
            errorMessage = "Informe um Slave ID válido entre 1 e 247.";
            return false;
        }

        settings.PortName = portComboBox.SelectedItem.ToString()!;
        settings.BaudRate = baudRate;
        settings.SlaveId = slaveId;

        return true;
    }

    public static int[] GetSelectedBaudRatesForSearch(
        ComboBox baudRateComboBox,
        CheckedListBox baudRateSearchCheckedListBox,
        int fallbackBaudRate)
    {
        List<int> baudRates = [];

        if (int.TryParse(baudRateComboBox.Text, out int selectedBaudRate))
        {
            baudRates.Add(selectedBaudRate);
        }

        foreach (object? item in baudRateSearchCheckedListBox.CheckedItems)
        {
            if (item is not null && int.TryParse(item.ToString(), out int checkedBaudRate))
            {
                baudRates.Add(checkedBaudRate);
            }
        }

        if (baudRates.Count == 0)
        {
            baudRates.Add(fallbackBaudRate);
        }

        return baudRates
            .Distinct()
            .ToArray();
    }
}
