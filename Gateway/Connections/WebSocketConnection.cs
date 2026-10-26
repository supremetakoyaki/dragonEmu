using System.Net.WebSockets;

namespace DragonEmu.Gateway;

public sealed class WebSocketConnection : IClientConnection
{
    private readonly WebSocket _webSocket;
    private readonly Stream _transport;

    public WebSocketConnection(WebSocket webSocket, Stream transport, string remoteAddress)
    {
        _webSocket = webSocket;
        _transport = transport;
        RemoteAddress = remoteAddress;
    }

    public string RemoteAddress { get; }

    public async ValueTask<int> ReceiveAsync(Memory<byte> buffer, CancellationToken cancellationToken)
    {
        while (true)
        {
            ValueWebSocketReceiveResult result = await _webSocket.ReceiveAsync(buffer, cancellationToken);
            if (result.MessageType == WebSocketMessageType.Close)
            {
                return 0;
            }

            if (result.Count > 0)
            {
                return result.Count;
            }
        }
    }

    public ValueTask SendAsync(ReadOnlyMemory<byte> data, CancellationToken cancellationToken) =>
        _webSocket.SendAsync(data, WebSocketMessageType.Binary, endOfMessage: true, cancellationToken);

    public async ValueTask DisposeAsync()
    {
        if (_webSocket.State is WebSocketState.Open or WebSocketState.CloseReceived)
        {
            try
            {
                using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(2));
                await _webSocket.CloseOutputAsync(WebSocketCloseStatus.NormalClosure, null, timeout.Token);
            }
            catch (Exception)
            {
            }
        }

        _webSocket.Dispose();
        await _transport.DisposeAsync();
    }
}
