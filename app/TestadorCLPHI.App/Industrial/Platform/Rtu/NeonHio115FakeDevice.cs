using TestadorCLPHI.App.Industrial.Platform.Devices;
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

internal sealed class NeonHio115FakeDevice : ISimulatedRtuDeviceEndpoint
{
    private readonly Dictionary<ushort, ushort> _registers = [];
    private readonly IndustrialDeviceProfile _profile;

    internal NeonHio115FakeDevice(
        byte address,
        int? programId = null,
        int? programCrc = null,
        ushort generalFailureStatus = 0,
        IndustrialDeviceProfile? profile = null)
    {
        if (address is 0 or > 247)
        {
            throw new ArgumentOutOfRangeException(nameof(address));
        }

        _profile = profile ?? NeonHio115DeviceProfile.Current;
        if (!string.Equals(
                _profile.Id,
                NeonHio115DeviceProfile.ProfileId,
                StringComparison.OrdinalIgnoreCase))
        {
            throw new ArgumentException(
                "NeonHio115FakeDevice requer um perfil executavel HIO115.",
                nameof(profile));
        }

        Address = address;
        FirmwareFamily = _profile.IdentificationPolicy.ExpectedFirmwareFamily ?? string.Empty;
        FirmwareVersion = _profile.IdentificationPolicy.ExpectedFirmwareVersion ?? string.Empty;
        ProgramId = checked((ushort)(programId
            ?? _profile.IdentificationPolicy.GetExpectedValue(DeviceIdentificationProbeRole.ProgramId)));
        ProgramCrc = checked((ushort)(programCrc
            ?? _profile.IdentificationPolicy.GetExpectedValue(DeviceIdentificationProbeRole.ProgramCrc)));
        GeneralFailureStatus = generalFailureStatus;

        foreach (DeviceIdentificationProbe probe in _profile.IdentificationPolicy.Probes)
        {
            _registers[probe.DocumentedReference] = probe.Role switch
            {
                DeviceIdentificationProbeRole.ProgramId => ProgramId,
                DeviceIdentificationProbeRole.ProgramCrc => ProgramCrc,
                DeviceIdentificationProbeRole.GeneralFailureStatus => GeneralFailureStatus,
                _ => probe.ExpectedValue ?? 0
            };
        }

        foreach (DeviceRegisterPoint point in _profile.InputMap.Blocks.SelectMany(block => block.Points))
        {
            _registers[point.DocumentedReference] = 0;
        }

        foreach (DeviceRegisterPoint point in _profile.OutputMap.Outputs)
        {
            _registers[point.DocumentedReference] = 0;
        }
    }

    public byte Address { get; }
    public bool SimulatesTimeout => ResponseMode == FakeDeviceResponseMode.Timeout;
    internal string DeviceProfileId => _profile.Id;
    internal string FirmwareFamily { get; init; }
    internal string FirmwareVersion { get; init; }
    internal ushort ProgramId { get; }
    internal ushort ProgramCrc { get; }
    internal ushort GeneralFailureStatus { get; }
    internal FakeDeviceResponseMode ResponseMode { get; set; }
    internal event Action<Layout3OutputChannel, bool>? OutputChanged;

    internal ushort GetRegister(ushort documentedReference) =>
        _registers.TryGetValue(documentedReference, out ushort value) ? value : (ushort)0;

    internal void SetDigitalInput(int channel, bool value)
    {
        if (!_profile.InputMap.TryResolve(DeviceInputKind.Digital, channel, out DeviceRegisterPoint point))
        {
            throw new ArgumentOutOfRangeException(nameof(channel));
        }

        _registers[point.DocumentedReference] = value ? (ushort)1 : (ushort)0;
    }

    internal void SetAnalogInput(int channel, ushort rawValue)
    {
        if (!_profile.InputMap.TryResolve(DeviceInputKind.Analog, channel, out DeviceRegisterPoint point))
        {
            throw new ArgumentOutOfRangeException(nameof(channel));
        }

        _registers[point.DocumentedReference] = rawValue;
    }

    internal bool GetDigitalOutput(Layout3OutputChannel channel)
    {
        if (!_profile.OutputMap.TryResolve(channel.ToString(), out DeviceRegisterPoint point))
        {
            throw new ArgumentOutOfRangeException(nameof(channel));
        }

        return _registers[point.DocumentedReference] != 0;
    }

    public byte[]? Process(ReadOnlySpan<byte> request)
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
        DeviceRegisterPoint? output = _profile.OutputMap.Outputs.FirstOrDefault(point =>
            point.DocumentedReference == reference);
        if (output is null || value > 1)
        {
            return RtuCodec.CreateExceptionResponse(Address, request[1], 0x02);
        }

        _registers[reference] = value;
        OutputChanged?.Invoke(GetOutputChannel(output), value != 0);
        return request.ToArray();
    }

    private static Layout3OutputChannel GetOutputChannel(DeviceRegisterPoint output) => output.Channel switch
    {
        0 => Layout3OutputChannel.DO00,
        1 => Layout3OutputChannel.DO01,
        2 => Layout3OutputChannel.DO02,
        3 => Layout3OutputChannel.DO03,
        _ => throw new ArgumentOutOfRangeException(nameof(output))
    };
}
