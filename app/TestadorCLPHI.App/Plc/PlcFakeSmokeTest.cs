namespace TestadorCLPHI.App.Plc;

public static class PlcFakeSmokeTest
{
    public static async Task<string> RunAsync()
    {
        PlcConnectionSettings settings = new()
        {
            PortName = "COM_FAKE",
            BaudRate = 9600,
            SlaveId = 1
        };

        IPlcCommunicationService plcService = new FakePlcCommunicationService();

        await plcService.ConnectAsync(settings);

        await plcService.WriteHoldingRegisterAsync(
            Hio115MemoryMap.HabilitaTeste,
            1);

        ushort value = await plcService.ReadHoldingRegisterAsync(
            Hio115MemoryMap.HabilitaTeste);

        await plcService.DisconnectAsync();

        if (value != 1)
        {
            return $"FALHA: esperado %MW{Hio115MemoryMap.HabilitaTeste} = 1, recebido {value}.";
        }

        return $"OK: escrita/leitura fake validada em %MW{Hio115MemoryMap.HabilitaTeste} = {value}.";
    }
}
