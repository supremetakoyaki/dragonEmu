using System.Net;
using System.Net.Sockets;

namespace DragonEmu.Gateway;

public sealed class SocketConnection : IClientConnection
{
    private readonly Socket _socket;

    public SocketConnection(Socket socket)
    {
        _socket = socket;
        RemoteAddress = (socket.RemoteEndPoint as IPEndPoint)?.Address.ToString() ?? "unknown";
    }

    public string RemoteAddress { get; }

    public ValueTask<int> ReceiveAsync(Memory<byte> buffer, CancellationToken cancellationToken) =>
        _socket.ReceiveAsync(buffer, SocketFlags.None, cancellationToken);

    public async ValueTask SendAsync(ReadOnlyMemory<byte> data, CancellationToken cancellationToken) =>
        await _socket.SendAsync(data, SocketFlags.None, cancellationToken);

    public ValueTask DisposeAsync()
    {
        try
        {
            _socket.Shutdown(SocketShutdown.Both);
        }
        catch (SocketException)
        {
        }

        _socket.Close();
        return ValueTask.CompletedTask;
    }
}
