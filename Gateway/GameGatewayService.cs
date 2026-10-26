using System.Net;
using System.Net.Sockets;
using System.Net.WebSockets;
using DragonEmu.Game.Achievements;
using DragonEmu.Game.Messenger;
using DragonEmu.Protocol;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace DragonEmu.Gateway;

public sealed class GameGatewayService : BackgroundService
{
    private static readonly TimeSpan HandshakeTimeout = TimeSpan.FromSeconds(10);

    private readonly GameServerOptions _options;

    private readonly byte[] _policyResponse;
    private readonly ClientSessionManager _sessionManager;
    private readonly IPacketDispatcher _dispatcher;
    private readonly PresenceService _presence;
    private readonly MessengerService _messenger;
    private readonly ILoggerFactory _loggerFactory;
    private readonly ILogger<GameGatewayService> _logger;
    private readonly List<Socket> _listeners = [];
    private volatile bool _listenerClosed;

    public GameGatewayService(
        IOptions<GameServerOptions> options,
        ClientSessionManager sessionManager,
        IPacketDispatcher dispatcher,
        PresenceService presence,
        MessengerService messenger,
        ILoggerFactory loggerFactory,
        ILogger<GameGatewayService> logger)
    {
        _messenger = messenger;
        _options = options.Value;
        _policyResponse = FlashSocketPolicy.BuildResponse(_options.Port);
        _sessionManager = sessionManager;
        _dispatcher = dispatcher;
        _presence = presence;
        _loggerFactory = loggerFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        Socket game = Listen(_options.Port);
        _logger.LogInformation("Game gateway listening on {Endpoint}", game.LocalEndPoint);
        var loops = new List<Task> { AcceptLoopAsync(game, HandleGameSocketAsync, stoppingToken) };

        if (_options.WebSocketPort > 0)
        {
            Socket webSocket = Listen(_options.WebSocketPort);
            _logger.LogInformation("WebSocket gateway (Ruffle) listening on {Endpoint}", webSocket.LocalEndPoint);
            loops.Add(AcceptLoopAsync(webSocket, HandleWebSocketAsync, stoppingToken));
        }

        await Task.WhenAll(loops);
    }

    private Socket Listen(int port)
    {
        var endpoint = new IPEndPoint(IPAddress.Parse(_options.BindAddress), port);
        var listener = new Socket(endpoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
        listener.Bind(endpoint);
        listener.Listen(512);
        _listeners.Add(listener);
        return listener;
    }

    private async Task AcceptLoopAsync(Socket listener, Func<Socket, CancellationToken, Task> handle, CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            Socket accepted;
            try
            {
                accepted = await listener.AcceptAsync(stoppingToken);
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (ObjectDisposedException)
            {
                break;
            }
            catch (SocketException) when (_listenerClosed || stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (SocketException ex)
            {
                _logger.LogWarning(ex, "Failed to accept a connection.");
                continue;
            }

            if (!_sessionManager.TryAcquireSlot())
            {
                accepted.Close();
                continue;
            }

            _ = handle(accepted, stoppingToken);
        }
    }

    private async Task HandleGameSocketAsync(Socket socket, CancellationToken stoppingToken)
    {
        byte[] probeBuffer = new byte[_options.ReceiveBufferSize];
        int received;
        try
        {
            received = await socket.ReceiveAsync(probeBuffer, SocketFlags.None, stoppingToken);
        }
        catch (Exception ex)
        {
            _logger.LogDebug(ex, "Connection dropped before it sent any data.");
            socket.Close();
            return;
        }

        if (received == 0)
        {
            socket.Close();
            return;
        }

        if (FlashSocketPolicy.LooksLikePolicyRequest(probeBuffer[0]))
        {
            try
            {
                await socket.SendAsync(_policyResponse, SocketFlags.None, stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogDebug(ex, "Failed to send the socket policy response.");
            }
            finally
            {
                socket.Close();
            }

            return;
        }

        await RunSessionAsync(new SocketConnection(socket), probeBuffer.AsMemory(0, received), stoppingToken);
    }

    private async Task HandleWebSocketAsync(Socket socket, CancellationToken stoppingToken)
    {
        string remoteAddress = (socket.RemoteEndPoint as IPEndPoint)?.Address.ToString() ?? "unknown";
        var stream = new NetworkStream(socket, ownsSocket: true);
        WebSocketUpgrade? upgrade;
        try
        {
            using var timeout = CancellationTokenSource.CreateLinkedTokenSource(stoppingToken);
            timeout.CancelAfter(HandshakeTimeout);
            upgrade = await ReadUpgradeAsync(stream, timeout.Token);
            if (upgrade is null)
            {
                await stream.WriteAsync(WebSocketHandshake.Refusal(400, "Bad Request"), timeout.Token);
            }
            else if (!WebSocketHandshake.IsAllowedOrigin(upgrade.Origin, _options.WebSocketAllowedOrigins))
            {
                _logger.LogWarning("Refused a WebSocket connection from {RemoteAddress}: page {Origin} isn't in WebSocketAllowedOrigins.", remoteAddress, upgrade.Origin ?? "(none)");
                await stream.WriteAsync(WebSocketHandshake.Refusal(403, "Forbidden"), timeout.Token);
                upgrade = null;
            }
            else
            {
                await stream.WriteAsync(WebSocketHandshake.SwitchingProtocols(upgrade.Key), timeout.Token);
            }
        }
        catch (Exception ex)
        {
            _logger.LogDebug(ex, "WebSocket connection from {RemoteAddress} dropped during the handshake.", remoteAddress);
            upgrade = null;
        }

        if (upgrade is null)
        {
            await stream.DisposeAsync();
            return;
        }

        WebSocket webSocket = WebSocket.CreateFromStream(stream, new WebSocketCreationOptions
        {
            IsServer = true,
            KeepAliveInterval = TimeSpan.FromSeconds(30),
        });
        await RunSessionAsync(new WebSocketConnection(webSocket, stream, remoteAddress), ReadOnlyMemory<byte>.Empty, stoppingToken);
    }

    private static async Task<WebSocketUpgrade?> ReadUpgradeAsync(NetworkStream stream, CancellationToken cancellationToken)
    {
        byte[] buffer = new byte[WebSocketHandshake.MaxRequestBytes];
        int length = 0;
        while (length < buffer.Length)
        {
            int read = await stream.ReadAsync(buffer.AsMemory(length), cancellationToken);
            if (read == 0)
            {
                return null;
            }

            length += read;
            int end = WebSocketHandshake.HeaderEnd(buffer.AsSpan(0, length));
            if (end >= 0)
            {
                return end == length ? WebSocketHandshake.Parse(buffer.AsSpan(0, end)) : null;
            }
        }

        return null;
    }

    private async Task RunSessionAsync(IClientConnection connection, ReadOnlyMemory<byte> initialBytes, CancellationToken stoppingToken)
    {
        long id = _sessionManager.AllocateId();
        var session = new ClientSession(id, connection, _options, _dispatcher, _loggerFactory.CreateLogger<ClientSession>());
        _sessionManager.Add(session);

        try
        {
            await session.RunAsync(initialBytes, stoppingToken);
        }
        finally
        {
            if (_sessionManager.Remove(session))
            {
                await _presence.FlushAsync(session, CancellationToken.None);
            }

            if (session.AuthenticatedUser is { } leaving && !_sessionManager.IsOnline(leaving.Id))
            {
                await _messenger.AnnounceAsync(leaving.Id, CancellationToken.None);
            }

            await session.DisposeAsync();
        }
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        _listenerClosed = true;
        foreach (Socket listener in _listeners)
        {
            listener.Close();
        }

        await base.StopAsync(cancellationToken);
    }
}
