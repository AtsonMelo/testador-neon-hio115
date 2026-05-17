namespace TestadorCLPHI.App.Plc;

public sealed class PlcDigitalIoManualService
{
    private readonly PlcRegisterCommandService _registerCommandService;

    private static readonly int[] OutputRegisters =
    [
        Hio115MemoryMap.CmdManualD000,
        Hio115MemoryMap.CmdManualD001,
        Hio115MemoryMap.CmdManualD002,
        Hio115MemoryMap.CmdManualD003
    ];

    private static readonly int[] InputRegisters =
    [
        Hio115MemoryMap.RetornoDi00,
        Hio115MemoryMap.RetornoDi01,
        Hio115MemoryMap.RetornoDi02,
        Hio115MemoryMap.RetornoDi03,
        Hio115MemoryMap.RetornoDi04,
        Hio115MemoryMap.RetornoDi05,
        Hio115MemoryMap.RetornoDi06,
        Hio115MemoryMap.RetornoDi07
    ];

    public PlcDigitalIoManualService(
        PlcRegisterCommandService registerCommandService)
    {
        _registerCommandService = registerCommandService;
    }

    public async Task ActivateManualOutputAsync(
        int channel,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (channel < 0 || channel >= OutputRegisters.Length)
        {
            throw new ArgumentOutOfRangeException(
                nameof(channel),
                channel,
                "Canal de saída digital inválido.");
        }

        await _registerCommandService.WriteAndReadBackAsync(
            Hio115MemoryMap.ModoAuto,
            0,
            cancellationToken);

        await _registerCommandService.WriteAndReadBackAsync(
            Hio115MemoryMap.ModoManual,
            1,
            cancellationToken);

        for (int index = 0; index < OutputRegisters.Length; index++)
        {
            cancellationToken.ThrowIfCancellationRequested();

            ushort valueToWrite = index == channel
                ? (ushort)1
                : (ushort)0;

            await _registerCommandService.WriteAndReadBackAsync(
                OutputRegisters[index],
                valueToWrite,
                cancellationToken);
        }
    }

    public async Task<bool[]> ReadDigitalInputsAsync(
        CancellationToken cancellationToken = default)
    {
        bool[] inputStates = new bool[InputRegisters.Length];

        for (int index = 0; index < InputRegisters.Length; index++)
        {
            cancellationToken.ThrowIfCancellationRequested();

            ushort value = await _registerCommandService.ReadAsync(
                InputRegisters[index],
                cancellationToken);

            inputStates[index] = value != 0;
        }

        return inputStates;
    }
}
