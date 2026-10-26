using System.Net;
using System.Net.Sockets;
using DragonEmu.Game.Achievements;
using DragonEmu.Game.Messenger;
using DragonEmu.Protocol;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace DragonEmu.Gateway;

public sealed class GameGatewayService : BackgroundService
{
    private readonly GameServerOptions _options;

    private readonly byte[] _policyResponse;
    private readonly ClientSessionManager _sessionManager;
    private readonly IPacketDispatcher _dispatcher;
    private readonly PresenceService _presence;
    private readonly MessengerService _messenger;
    private readonly ILoggerFactory _loggerFactory;
    private readonly ILogger<GameGatewayService> _logger;
    private Socket? _listener;
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
        var endpoint = new IPEndPoint(IPAddress.Parse(_options.BindAddress), _options.Port);
        _listener = new Socket(endpoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
        _listener.Bind(endpoint);
        _listener.Listen(512);

        _logger.LogInformation("Game gateway listening on {Endpoint}", endpoint);

        while (!stoppingToken.IsCancellationRequested)
        {
            Socket accepted;
            try
            {
                accepted = await _listener.AcceptAsync(stoppingToken);
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

            _ = HandleAcceptedSocketAsync(accepted, stoppingToken);
        }
    }

    private async Task HandleAcceptedSocketAsync(Socket socket, CancellationToken stoppingToken)
    {
        if (!_sessionManager.TryAcquireSlot())
        {
            socket.Close();
            return;
        }

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

        long id = _sessionManager.AllocateId();
        var session = new ClientSession(id, socket, _options, _dispatcher, _loggerFactory.CreateLogger<ClientSession>());
        _sessionManager.Add(session);

        try
        {
            await session.RunAsync(probeBuffer.AsMemory(0, received), stoppingToken);
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
        _listener?.Close();
        await base.StopAsync(cancellationToken);
    }
}
