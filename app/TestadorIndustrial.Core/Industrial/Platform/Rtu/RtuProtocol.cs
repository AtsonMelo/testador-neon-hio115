namespace TestadorCLPHI.App.Industrial.Platform.Rtu;

internal sealed class RtuProtocolException : Exception
{
    public RtuProtocolException(string message)
        : base(message)
    {
    }
}

internal static class RtuCrc16
{
    internal static ushort Calculate(ReadOnlySpan<byte> data)
    {
        ushort crc = 0xFFFF;
        foreach (byte value in data)
        {
            crc ^= value;
            for (int bit = 0; bit < 8; bit++)
            {
                bool leastSignificantBitSet = (crc & 0x0001) != 0;
                crc >>= 1;
                if (leastSignificantBitSet)
                {
                    crc ^= 0xA001;
                }
            }
        }

        return crc;
    }

    internal static byte[] Append(ReadOnlySpan<byte> payload)
    {
        ushort crc = Calculate(payload);
        byte[] frame = new byte[payload.Length + 2];
        payload.CopyTo(frame);
        frame[^2] = (byte)(crc & 0xFF);
        frame[^1] = (byte)(crc >> 8);
        return frame;
    }

    internal static bool IsValid(ReadOnlySpan<byte> frame)
    {
        if (frame.Length < 4)
        {
            return false;
        }

        ushort expected = Calculate(frame[..^2]);
        ushort received = (ushort)(frame[^2] | frame[^1] << 8);
        return expected == received;
    }
}

internal static class RtuCodec
{
    internal const byte ReadHoldingRegistersFunction = 0x03;
    internal const byte WriteSingleRegisterFunction = 0x06;

    internal static byte[] CreateReadRequest(byte deviceAddress, ushort startAddress, ushort quantity)
    {
        ValidateDeviceAddress(deviceAddress);
        if (quantity is 0 or > 125)
        {
            throw new ArgumentOutOfRangeException(nameof(quantity), "FC03 permite quantidade 1..125.");
        }

        return RtuCrc16.Append(
        [
            deviceAddress,
            ReadHoldingRegistersFunction,
            (byte)(startAddress >> 8),
            (byte)startAddress,
            (byte)(quantity >> 8),
            (byte)quantity
        ]);
    }

    internal static byte[] CreateWriteSingleRequest(byte deviceAddress, ushort address, ushort value)
    {
        ValidateDeviceAddress(deviceAddress);
        return RtuCrc16.Append(
        [
            deviceAddress,
            WriteSingleRegisterFunction,
            (byte)(address >> 8),
            (byte)address,
            (byte)(value >> 8),
            (byte)value
        ]);
    }

    internal static ushort[] ParseReadResponse(
        ReadOnlySpan<byte> frame,
        byte expectedDeviceAddress,
        ushort expectedQuantity)
    {
        ValidateResponseEnvelope(frame, expectedDeviceAddress, ReadHoldingRegistersFunction);
        int expectedByteCount = expectedQuantity * 2;
        if (frame.Length != expectedByteCount + 5 || frame[2] != expectedByteCount)
        {
            throw new RtuProtocolException("Resposta FC03 possui tamanho ou byte count invalido.");
        }

        ushort[] values = new ushort[expectedQuantity];
        for (int index = 0; index < values.Length; index++)
        {
            int offset = 3 + index * 2;
            values[index] = (ushort)(frame[offset] << 8 | frame[offset + 1]);
        }

        return values;
    }

    internal static void ParseWriteSingleResponse(
        ReadOnlySpan<byte> frame,
        byte expectedDeviceAddress,
        ushort expectedAddress,
        ushort expectedValue)
    {
        ValidateResponseEnvelope(frame, expectedDeviceAddress, WriteSingleRegisterFunction);
        if (frame.Length != 8)
        {
            throw new RtuProtocolException("Resposta FC06 deve possuir 8 bytes.");
        }

        ushort address = (ushort)(frame[2] << 8 | frame[3]);
        ushort value = (ushort)(frame[4] << 8 | frame[5]);
        if (address != expectedAddress || value != expectedValue)
        {
            throw new RtuProtocolException("Echo FC06 diverge do comando solicitado.");
        }
    }

    internal static byte[] CreateReadResponse(byte deviceAddress, ReadOnlySpan<ushort> values)
    {
        ValidateDeviceAddress(deviceAddress);
        if (values.Length is 0 or > 125)
        {
            throw new ArgumentOutOfRangeException(nameof(values));
        }

        byte[] payload = new byte[3 + values.Length * 2];
        payload[0] = deviceAddress;
        payload[1] = ReadHoldingRegistersFunction;
        payload[2] = (byte)(values.Length * 2);
        for (int index = 0; index < values.Length; index++)
        {
            payload[3 + index * 2] = (byte)(values[index] >> 8);
            payload[4 + index * 2] = (byte)values[index];
        }

        return RtuCrc16.Append(payload);
    }

    internal static byte[] CreateExceptionResponse(byte deviceAddress, byte function, byte exceptionCode) =>
        RtuCrc16.Append([deviceAddress, (byte)(function | 0x80), exceptionCode]);

    internal static void ValidateRequest(ReadOnlySpan<byte> frame)
    {
        if (frame.Length != 8)
        {
            throw new RtuProtocolException("Request RTU incompleto ou com tamanho invalido.");
        }

        if (!RtuCrc16.IsValid(frame))
        {
            throw new RtuProtocolException("CRC RTU invalido.");
        }

        ValidateDeviceAddress(frame[0]);
    }

    private static void ValidateResponseEnvelope(
        ReadOnlySpan<byte> frame,
        byte expectedDeviceAddress,
        byte expectedFunction)
    {
        if (frame.Length < 5)
        {
            throw new RtuProtocolException("Resposta RTU incompleta.");
        }

        if (!RtuCrc16.IsValid(frame))
        {
            throw new RtuProtocolException("CRC RTU invalido na resposta.");
        }

        if (frame[0] != expectedDeviceAddress)
        {
            throw new RtuProtocolException("Endereco da resposta diverge da solicitacao.");
        }

        if (frame[1] == (expectedFunction | 0x80))
        {
            throw new RtuProtocolException($"Excecao RTU 0x{frame[2]:X2} para FC 0x{expectedFunction:X2}.");
        }

        if (frame[1] != expectedFunction)
        {
            throw new RtuProtocolException("Funcao da resposta diverge da solicitacao.");
        }
    }

    private static void ValidateDeviceAddress(byte deviceAddress)
    {
        if (deviceAddress is 0 or > 247)
        {
            throw new ArgumentOutOfRangeException(nameof(deviceAddress), "Endereco RTU padrao deve estar em 1..247.");
        }
    }
}
