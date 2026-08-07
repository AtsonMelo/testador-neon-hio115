using TestadorCLPHI.App.Ui.Industrial.Layout3;

namespace TestadorCLPHI.App.Industrial.Platform.Rtu;

internal enum FakeDeviceResponseMode
{
    Normal,
    NoResponse,
    Timeout,
    InvalidCrc,
    InvalidFunction,
    InvalidAddress
}

internal sealed class NeonHio115FakeDevice
{
    private readonly Dictionary<ushort, ushort> _registers = [];

    internal NeonHio115FakeDevice(
        byte address,
        int programId = 31134,
        int programCrc = 23248,
        ushort generalFailureStatus = 0)
    {
        if (address is 0 or > 247)
        {
            throw new ArgumentOutOfRangeException(nameof(address));
        }

        Address = address;
        ProgramId = checked((ushort)programId);
        ProgramCrc = checked((ushort)programCrc);
        GeneralFailureStatus = generalFailureStatus;
        _registers[30012] = ProgramId;
        _registers[30013] = ProgramCrc;
        _registers[30021] = GeneralFailureStatus;

        for (ushort reference = 31120; reference <= 31127; reference++)
        {
            _registers[reference] = 0;
        }

        for (ushort reference = 31128; reference <= 31134; reference++)
        {
            _registers[reference] = 0;
        }
    }

    internal byte Address { get; }
    internal string FirmwareFamily { get; init; } = "G5PLC.C950.ST";
    internal string FirmwareVersion { get; init; } = "3.3.11";
    internal ushort ProgramId { get; }
    internal ushort ProgramCrc { get; }
    internal ushort GeneralFailureStatus { get; }
    internal FakeDeviceResponseMode ResponseMode { get; set; }

    internal ushort GetRegister(ushort documentedReference) =>
        _registers.TryGetValue(documentedReference, out ushort value) ? value : (ushort)0;

    internal void SetDigitalInput(int channel, bool value)
    {
        ValidateChannel(channel, 8, nameof(channel));
        _registers[(ushort)(31120 + channel)] = value ? (ushort)1 : (ushort)0;
    }

    internal void SetAnalogInput(int channel, ushort rawValue)
    {
        ValidateChannel(channel, 3, nameof(channel));
        _registers[(ushort)(31132 + channel)] = rawValue;
    }

    internal bool GetDigitalOutput(Layout3OutputChannel channel) =>
        _registers[(ushort)Layout3BenchWorkflowPolicy.GetOutputDocumentedReference(channel)] != 0;

    internal byte[]? Process(ReadOnlySpan<byte> request)
    {
        if (ResponseMode == FakeDeviceResponseMode.NoResponse)
        {
            return null;
        }

        RtuCodec.ValidateRequest(request);
        byte function = request[1];
        byte[] response = ResponseMode switch
        {
            FakeDeviceResponseMode.InvalidFunction =>
                RtuCodec.CreateExceptionResponse(Address, function, 0x01),
            FakeDeviceResponseMode.InvalidAddress =>
                RtuCodec.CreateExceptionResponse(Address, function, 0x02),
            _ => ProcessFunction(request, function)
        };

        if (ResponseMode == FakeDeviceResponseMode.InvalidCrc)
        {
            response[^1] ^= 0xFF;
        }

        return response;
    }

    private byte[] ProcessFunction(ReadOnlySpan<byte> request, byte function)
    {
        return function switch
        {
            RtuCodec.ReadHoldingRegistersFunction => ProcessRead(request),
            RtuCodec.WriteSingleRegisterFunction => ProcessOutput(request),
            _ => RtuCodec.CreateExceptionResponse(Address, function, 0x01)
        };
    }

    private byte[] ProcessRead(ReadOnlySpan<byte> request)
    {
        ushort start = (ushort)(request[2] << 8 | request[3]);
        ushort quantity = (ushort)(request[4] << 8 | request[5]);
        if (quantity is 0 or > 125)
        {
            return RtuCodec.CreateExceptionResponse(Address, request[1], 0x03);
        }

        ushort[] values = new ushort[quantity];
        for (int index = 0; index < values.Length; index++)
        {
            ushort reference = checked((ushort)(start + index));
            if (!_registers.TryGetValue(reference, out values[index]))
            {
                return RtuCodec.CreateExceptionResponse(Address, request[1], 0x02);
            }
        }

        return RtuCodec.CreateReadResponse(Address, values);
    }

    private byte[] ProcessOutput(ReadOnlySpan<byte> request)
    {
        ushort reference = (ushort)(request[2] << 8 | request[3]);
        ushort value = (ushort)(request[4] << 8 | request[5]);
        if (!Layout3BenchWorkflowPolicy.IsOutputReferenceAllowed(reference) || value > 1)
        {
            return RtuCodec.CreateExceptionResponse(Address, request[1], 0x02);
        }

        _registers[reference] = value;
        return request.ToArray();
    }

    private static void ValidateChannel(int channel, int count, string parameterName)
    {
        if (channel < 0 || channel >= count)
        {
            throw new ArgumentOutOfRangeException(parameterName);
        }
    }
}
