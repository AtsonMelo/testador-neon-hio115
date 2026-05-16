namespace TestadorCLPHI.App.Plc;

public static class Hio115MemoryMap
{
    public const int ModoAuto = 10;
    public const int ModoManual = 11;

    public const int CmdManualD000 = 12;
    public const int CmdManualD001 = 13;
    public const int CmdManualD002 = 14;
    public const int CmdManualD003 = 15;

    public const int RetornoDi00 = 20;
    public const int RetornoDi01 = 21;
    public const int RetornoDi02 = 22;
    public const int RetornoDi03 = 23;

    public const int OkD000 = 30;
    public const int OkD001 = 31;
    public const int OkD002 = 32;
    public const int OkD003 = 33;

    public const int FalhaD000 = 40;
    public const int FalhaD001 = 41;
    public const int FalhaD002 = 42;
    public const int FalhaD003 = 43;

    public const int ErroD000 = 50;
    public const int ErroD001 = 51;
    public const int ErroD002 = 52;
    public const int ErroD003 = 53;

    public const int TesteOkGeral = 60;
    public const int TesteFalhaGeral = 61;
    public const int TesteErroGeral = 62;

    public const int HabilitaTeste = 70;
    public const int ResetSaidas = 71;
    public const int WatchdogApp = 72;
    public const int FalhaWatchdog = 73;
    public const int WatchdogAtivo = 74;
}
