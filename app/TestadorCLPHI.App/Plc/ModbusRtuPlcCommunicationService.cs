using System.IO.Ports;

namespace TestadorCLPHI.App.Plc;

public sealed class ModbusRtuPlcCommunicationService : IPlcCommunicationService, IDisposable
{
    private readonly object _syncRoot = new();
    private SerialPort? _serialPort;
    private byte _slaveId = 1;

    public PlcConnectionState State { get; } = new();

    public Task ConnectAsync(PlcConnectionSettings settings)
    {
        return ConnectAsync(settings, CancellationToken.None);
    }

    public Task ConnectAsync(PlcConnectionSettings settings, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(settings);

        return Task.Run(() =>
        {
            cancellationToken.ThrowIfCancellationRequested();

            lock (_syncRoot)
            {
                State.SetConnecting($"Abrindo {settings.PortName}...");

                CloseSerialPort();

                _slaveId = settings.SlaveId;

                _serialPort = new SerialPort(
                    settings.PortName,
                    settings.BaudRate,
                    settings.Parity,
                    settings.DataBits,
                    settings.StopBits)
                {
                    ReadTimeout = settings.TimeoutMilliseconds,
                    WriteTimeout = settings.TimeoutMilliseconds
                };

                _serialPort.Open();

                State.SetConnected($"Porta aberta em {settings.PortName} - Slave {_slaveId}");
            }
        }, cancellationToken);
    }

    public Task DisconnectAsync()
    {
        return DisconnectAsync(CancellationToken.None);
    }

    public Task DisconnectAsync(CancellationToken cancellationToken = default)
    {
        return Task.Run(() =>
        {
            cancellationToken.ThrowIfCancellationRequested();

            lock (_syncRoot)
            {
                CloseSerialPort();
                State.SetDisconnected("Desconectado");
            }
        }, cancellationToken);
    }

    public Task<ushort> ReadHoldingRegisterAsync(int registerAddress)
    {
        return ReadHoldingRegisterAsync(registerAddress, CancellationToken.None);
    }

    public Task<ushort> ReadHoldingRegisterAsync(int registerAddress, CancellationToken cancellationToken = default)
    {
        return Task.Run(() =>
        {
            cancellationToken.ThrowIfCancellationRequested();

            lock (_syncRoot)
            {
                EnsureConnected();

                byte[] request = BuildReadHoldingRegisterRequest(registerAddress);
                byte[] response = SendAndReceive(request, expectedLength: 7);

                ValidateResponse(response, functionCode: 0x03);

                if (response[2] != 2)
                {
                    throw new InvalidOperationException($"Resposta Modbus inválida. Byte count recebido: {response[2]}.");
                }

                ushort value = (ushort)((response[3] << 8) | response[4]);

                State.SetConnected($"Leitura OK: %MW{registerAddress} = {value}");

                return value;
            }
        }, cancellationToken);
    }

    public Task WriteHoldingRegisterAsync(int registerAddress, ushort value)
    {
        return WriteHoldingRegisterAsync(registerAddress, value, CancellationToken.None);
    }

    public Task WriteHoldingRegisterAsync(int registerAddress, ushort value, CancellationToken cancellationToken = default)
    {
        return Task.Run(() =>
        {
            cancellationToken.ThrowIfCancellationRequested();

            lock (_syncRoot)
            {
                EnsureConnected();

                byte[] request = BuildWriteSingleRegisterRequest(registerAddress, value);
                byte[] response = SendAndReceive(request, expectedLength: 8);

                ValidateResponse(response, functionCode: 0x06);

                State.SetConnected($"Escrita OK: %MW{registerAddress} = {value}");
            }
        }, cancellationToken);
    }

    private byte[] BuildReadHoldingRegisterRequest(int registerAddress)
    {
        ValidateRegisterAddress(registerAddress);

        ushort address = (ushort)registerAddress;

        byte[] frameWithoutCrc =
        [
            _slaveId,
            0x03,
            (byte)(address >> 8),
            (byte)(address & 0xFF),
            0x00,
            0x01
        ];

        return AddCrc(frameWithoutCrc);
    }

    private byte[] BuildWriteSingleRegisterRequest(int registerAddress, ushort value)
    {
        ValidateRegisterAddress(registerAddress);

        ushort address = (ushort)registerAddress;

        byte[] frameWithoutCrc =
        [
            _slaveId,
            0x06,
            (byte)(address >> 8),
            (byte)(address & 0xFF),
            (byte)(value >> 8),
            (byte)(value & 0xFF)
        ];

        return AddCrc(frameWithoutCrc);
    }

    private byte[] SendAndReceive(byte[] request, int expectedLength)
    {
        SerialPort serialPort = _serialPort ?? throw new InvalidOperationException("Porta serial não inicializada.");

        serialPort.DiscardInBuffer();
        serialPort.DiscardOutBuffer();

        serialPort.Write(request, 0, request.Length);

        byte[] response = new byte[expectedLength];
        int offset = 0;

        while (offset < expectedLength)
        {
            int read = serialPort.Read(response, offset, expectedLength - offset);

            if (read <= 0)
            {
                throw new TimeoutException("Timeout aguardando resposta Modbus RTU.");
            }

            offset += read;
        }

        return response;
    }

    private void ValidateResponse(byte[] response, byte functionCode)
    {
        if (!HasValidCrc(response))
        {
            throw new InvalidOperationException("CRC Modbus inválido na resposta.");
        }

        if (response[0] != _slaveId)
        {
            throw new InvalidOperationException($"Resposta de Slave ID inesperado. Recebido: {response[0]}, esperado: {_slaveId}.");
        }

        if ((response[1] & 0x80) != 0)
        {
            byte exceptionCode = response.Length > 2 ? response[2] : (byte)0;
            throw new InvalidOperationException($"Exceção Modbus. Função: 0x{response[1]:X2}, código: 0x{exceptionCode:X2}.");
        }

        if (response[1] != functionCode)
        {
            throw new InvalidOperationException($"Função Modbus inesperada. Recebido: 0x{response[1]:X2}, esperado: 0x{functionCode:X2}.");
        }
    }

    private void EnsureConnected()
    {
        if (_serialPort is null || !_serialPort.IsOpen)
        {
            State.SetError("Porta serial não conectada.");
            throw new InvalidOperationException("Conecte ao CLP antes de ler ou escrever registradores.");
        }
    }

    private static void ValidateRegisterAddress(int registerAddress)
    {
        if (registerAddress is < 0 or > ushort.MaxValue)
        {
            throw new ArgumentOutOfRangeException(
                nameof(registerAddress),
                "Endereço Modbus deve estar entre 0 e 65535.");
        }
    }

    private static byte[] AddCrc(byte[] frameWithoutCrc)
    {
        ushort crc = CalculateCrc(frameWithoutCrc);

        byte[] frame = new byte[frameWithoutCrc.Length + 2];

        Buffer.BlockCopy(frameWithoutCrc, 0, frame, 0, frameWithoutCrc.Length);

        frame[^2] = (byte)(crc & 0xFF);
        frame[^1] = (byte)(crc >> 8);

        return frame;
    }

    private static bool HasValidCrc(byte[] frame)
    {
        if (frame.Length < 3)
        {
            return false;
        }

        ushort expectedCrc = CalculateCrc(frame.AsSpan(0, frame.Length - 2));
        ushort receivedCrc = (ushort)(frame[^2] | (frame[^1] << 8));

        return expectedCrc == receivedCrc;
    }

    private static ushort CalculateCrc(ReadOnlySpan<byte> data)
    {
        ushort crc = 0xFFFF;

        foreach (byte value in data)
        {
            crc ^= value;

            for (int bit = 0; bit < 8; bit++)
            {
                bool lsbSet = (crc & 0x0001) != 0;
                crc >>= 1;

                if (lsbSet)
                {
                    crc ^= 0xA001;
                }
            }
        }

        return crc;
    }

    private void CloseSerialPort()
    {
        if (_serialPort is null)
        {
            return;
        }

        if (_serialPort.IsOpen)
        {
            _serialPort.Close();
        }

        _serialPort.Dispose();
        _serialPort = null;
    }

    public void Dispose()
    {
        lock (_syncRoot)
        {
            CloseSerialPort();
        }
    }
}

