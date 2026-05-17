using System.Threading;
using System.Threading.Tasks;

namespace TestadorCLPHI.App.Plc;

public interface IPlcCommunicationService
{
    PlcConnectionState State { get; }

    Task ConnectAsync(
        PlcConnectionSettings settings,
        CancellationToken cancellationToken = default);

    Task DisconnectAsync(
        CancellationToken cancellationToken = default);

    Task<ushort> ReadHoldingRegisterAsync(
        int registerAddress,
        CancellationToken cancellationToken = default);

    Task WriteHoldingRegisterAsync(
        int registerAddress,
        ushort value,
        CancellationToken cancellationToken = default);
}
