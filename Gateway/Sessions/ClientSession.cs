using System.Net;
using System.Net.Sockets;
using System.Threading.Channels;
using DragonEmu.Domain;
using DragonEmu.Protocol;
using DragonEmu.Protocol.Messages;
using Microsoft.Extensions.Logging;

namespace DragonEmu.Gateway;

public sealed class ClientSession : IAsyncDisposable, IGameSession
{
    private readonly Socket _socket;
    private readonly ClientPacketReader _reader;
    private readonly Channel<byte[]> _outgoing;
    private readonly CancellationTokenSource _cts = new();
    private readonly TimeSpan _idleTimeout;
    private readonly byte[] _receiveBuffer;
    private readonly IPacketDispatcher _dispatcher;
    private readonly ILogger<ClientSession> _logger;

    public long Id { get; }

    public string RemoteAddress { get; }

    public UserAccount? AuthenticatedUser { get; private set; }

    public OnlineTimeMeter OnlineTime { get; } = new();

    public int? LoadingRoomId { get; private set; }

    public int? CurrentRoomId { get; private set; }

    public bool TeleportMode { get; set; }

    public double? TeleportHeight { get; set; }

    public PendingTeleportArrival? PendingTeleport { get; set; }

    public PendingArrival? PendingArrival { get; set; }

    public DateTime? NoInterstitialUntil { get; set; }

    public bool NameChangeDialogOpen { get; set; }

    public Game.Polls.PollProgress? ActivePoll { get; set; }

    public string? Swimsuit { get; set; }

    public IReadOnlySet<int> IgnoredUsers { get; set; } = new HashSet<int>();

    public int? RingingAt { get; set; }

    public int? LetInto { get; set; }

    public int AvatarEffectId { get; set; }

    public bool UserObjectSent { get; set; }

    public DateTime? LoggedInAt { get; private set; }

    public void Authenticate(UserAccount user)
    {
        LoggedInAt ??= DateTime.UtcNow;
        AuthenticatedUser = user;
        OnlineTime.Start(DateTime.UtcNow);
    }

    public void SetLoadingRoom(int? roomId) => LoadingRoomId = roomId;

    public void SetCurrentRoom(int? roomId) => CurrentRoomId = roomId;

    public ClientSession(
        long id,
        Socket socket,
        GameServerOptions options,
        IPacketDispatcher dispatcher,
        ILogger<ClientSession> logger)
    {
        Id = id;
        _socket = socket;
        RemoteAddress = (socket.RemoteEndPoint as IPEndPoint)?.Address.ToString() ?? "unknown";
        _dispatcher = dispatcher;
        _logger = logger;
        _reader = new ClientPacketReader(options.MaxFrameSize);
        _receiveBuffer = new byte[options.ReceiveBufferSize];
        _idleTimeout = TimeSpan.FromSeconds(options.IdleTimeoutSeconds);
        _outgoing = Channel.CreateBounded<byte[]>(new BoundedChannelOptions(256)
        {
            SingleReader = true,
            SingleWriter = false,
        });
    }

    void IGameSession.Send(ServerPacket packet) => EnqueueSend(packet);

    public bool EnqueueSend(ServerPacket packet) => EnqueueSend(packet.ToArray());

    public bool EnqueueSend(byte[] frame)
    {
        if (_outgoing.Writer.TryWrite(frame))
        {
            _logger.LogOutgoing(frame);
            return true;
        }

        _logger.LogWarning("Session {SessionId} outgoing queue is full. Disconnecting...", Id);
        Close();
        return false;
    }

    public void Close()
    {
        try
        {
            _cts.Cancel();
        }
        catch (ObjectDisposedException)
        {
        }
    }

    private int _replaced;

    public bool IsReplaced => Volatile.Read(ref _replaced) == 1;

    internal bool MarkReplaced() => Interlocked.Exchange(ref _replaced, 1) == 0;

    public DateTime? MutedUntil { get; set; }

    public bool IsModerator { get; set; }

    public ChatFloodGuard RoomChatFlood { get; } = new();

    public ChatFloodGuard MessengerFlood { get; } = new();

    private readonly ActionCooldowns _cooldowns = new();

    public bool TryUseCooldown(string key, TimeSpan cooldown) => _cooldowns.TryUse(key, cooldown, Environment.TickCount64);

    public RewardTimer CreditReward { get; } = new();

    public RewardTimer PixelReward { get; } = new();

    internal RepeatFilter Repeats { get; } = new();

    internal async Task RunAsync(ReadOnlyMemory<byte> initialBytes, CancellationToken hostCancellationToken)
    {
        using CancellationTokenSource linked =
            CancellationTokenSource.CreateLinkedTokenSource(hostCancellationToken, _cts.Token);
        CancellationToken token = linked.Token;

        Task sendLoop = SendLoopAsync(token);

        try
        {
            await ProcessReceivedBytesAsync(initialBytes, token);
            await ReceiveLoopAsync(token);
        }
        catch (OperationCanceledException)
        {
        }
        catch (Exception ex)
        {
            _logger.LogDebug(ex, "Session {SessionId} receive loop ended abnormally.", Id);
        }
        finally
        {
            _cts.Cancel();
            _outgoing.Writer.TryComplete();
            await sendLoop;
        }
    }

    private async Task ReceiveLoopAsync(CancellationToken token)
    {
        while (!token.IsCancellationRequested)
        {
            int received;
            try
            {
                received = await _socket.ReceiveAsync(_receiveBuffer, SocketFlags.None, token)
                    .AsTask()
                    .WaitAsync(_idleTimeout, token);
            }
            catch (TimeoutException)
            {
                _logger.LogInformation("Session {SessionId} timed out after {Timeout}.", Id, _idleTimeout);
                return;
            }

            if (received == 0)
            {
                return;
            }

            await ProcessReceivedBytesAsync(_receiveBuffer.AsMemory(0, received), token);
        }
    }

    private async Task ProcessReceivedBytesAsync(ReadOnlyMemory<byte> data, CancellationToken token)
    {
        _reader.Append(data.Span);

        while (_reader.TryReadPacket(out ClientPacket? packet))
        {
            await _dispatcher.DispatchAsync(this, packet!, token);
        }
    }

    private async Task SendLoopAsync(CancellationToken token)
    {
        try
        {
            await foreach (byte[] frame in _outgoing.Reader.ReadAllAsync(token))
            {
                await _socket.SendAsync(frame, SocketFlags.None, token);
            }
        }
        catch (OperationCanceledException)
        {
        }
        catch (Exception ex)
        {
            _logger.LogDebug(ex, "Session {SessionId} send loop ended abnormally.", Id);
        }
    }

    public async ValueTask DisposeAsync()
    {
        _cts.Cancel();
        _cts.Dispose();

        try
        {
            _socket.Shutdown(SocketShutdown.Both);
        }
        catch (SocketException)
        {
        }

        _socket.Close();
        await Task.CompletedTask;
    }
}
