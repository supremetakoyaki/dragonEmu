using DragonEmu.Domain;
using DragonEmu.Gateway;
using DragonEmu.Game.Rooms;
using DragonEmu.Persistence;
using DragonEmu.Protocol;
using DragonEmu.Protocol.Messages;
using DragonEmu.Protocol.Messages.Outgoing.InventoryAvatarEffect;
using DragonEmu.Protocol.Messages.Outgoing.RoomAction;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace DragonEmu.Game.Avatars;

public sealed class AvatarEffectService : IHostedService
{
    public const int NoEffect = 0;

    public const int StopWearing = -1;

    private static readonly TimeSpan MaxSleep = TimeSpan.FromHours(1);

    private readonly IAvatarEffectRepository _repository;
    private readonly ClientSessionManager _sessionManager;
    private readonly IRoomManager _roomManager;
    private readonly RoomBroadcaster _broadcaster;
    private readonly ILogger<AvatarEffectService> _logger;

    private readonly PriorityQueue<(int UserId, int EffectId), DateTime> _expiries = new();
    private readonly SemaphoreSlim _scheduled = new(0);
    private CancellationTokenSource? _stoppingCts;
    private Task? _loop;

    public AvatarEffectService(
        IAvatarEffectRepository repository, ClientSessionManager sessionManager, IRoomManager roomManager,
        RoomBroadcaster broadcaster, ILogger<AvatarEffectService> logger)
    {
        _repository = repository;
        _sessionManager = sessionManager;
        _roomManager = roomManager;
        _broadcaster = broadcaster;
        _logger = logger;
    }

    public async Task SendInventoryAsync(IGameSession session, int userId, CancellationToken cancellationToken)
    {
        DateTime now = UtcNow();
        IReadOnlyList<AvatarEffect> effects = await _repository.GetAsync(userId, now, cancellationToken);
        session.Send(new AvatarEffectsMessageEvent(effects, now).Compose());

        foreach (AvatarEffect effect in effects)
        {
            if (effect.ExpiresAt is { } expiresAt)
            {
                ScheduleExpiry(userId, effect.EffectId, expiresAt);
            }
        }
    }

    public async Task GrantAsync(IGameSession session, int userId, int effectId, int duration, int quantity, CancellationToken cancellationToken)
    {
        await _repository.AddAsync(userId, effectId, duration, quantity, cancellationToken);

        ServerPacket added = new AvatarEffectAddedMessageEvent(effectId, duration).Compose();
        for (int i = 0; i < quantity; i++)
        {
            session.Send(added);
        }
    }

    public async Task<bool> GrantRunningAsync(IGameSession session, int userId, int effectId, int duration, CancellationToken cancellationToken)
    {
        AvatarEffect? effect = await _repository.TryAddRunningAsync(userId, effectId, duration, UtcNow(), cancellationToken);
        if (effect?.ExpiresAt is not { } expiresAt)
        {
            return false;
        }

        session.Send(new AvatarEffectAddedMessageEvent(effectId, duration).Compose());
        session.Send(new AvatarEffectActivatedMessageEvent(effectId, duration).Compose());
        ScheduleExpiry(userId, effectId, expiresAt);
        Wear(session, effectId);
        return true;
    }

    public async Task ActivateAsync(IGameSession session, int userId, int effectId, CancellationToken cancellationToken)
    {
        AvatarEffect? effect = await _repository.TryActivateAsync(userId, effectId, UtcNow(), cancellationToken);
        if (effect?.ExpiresAt is not { } expiresAt)
        {
            return;
        }

        session.Send(new AvatarEffectActivatedMessageEvent(effectId, effect.Duration).Compose());
        ScheduleExpiry(userId, effectId, expiresAt);
    }

    public async Task WearAsync(IGameSession session, int userId, int effectId, CancellationToken cancellationToken)
    {
        if (effectId == StopWearing)
        {
            Wear(session, NoEffect);
        }
        else if (await _repository.IsRunningAsync(userId, effectId, UtcNow(), cancellationToken))
        {
            Wear(session, effectId);
        }
    }

    public bool TakeOff(IGameSession session) => Wear(session, NoEffect);

    private bool Wear(IGameSession session, int effectId)
    {
        session.AvatarEffectId = effectId;
        return ShowInRoom(session, effectId);
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _stoppingCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);

        _loop = Task.Run(() => RunExpiriesAsync(_stoppingCts.Token), CancellationToken.None);
        return Task.CompletedTask;
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        _stoppingCts?.Cancel();

        if (_loop is not null)
        {
            try
            {
                await _loop.WaitAsync(cancellationToken);
            }
            catch (OperationCanceledException)
            {
            }
        }
    }

    public bool ShowInRoom(IGameSession session, int effectId)
    {
        if (session.CurrentRoomId is not { } roomId
            || !_roomManager.TryGetLoadedRoom(roomId, out Room? room)
            || !room!.TryGetActor(session.Id, out RoomActor? actor))
        {
            return false;
        }

        lock (actor!.Lock)
        {
            actor.EffectId = effectId;
        }

        _broadcaster.Broadcast(room, new AvatarEffectMessageEvent(actor.VirtualId, effectId).Compose());
        return true;
    }

    private void ScheduleExpiry(int userId, int effectId, DateTime expiresAt)
    {
        lock (_expiries)
        {
            _expiries.Enqueue((userId, effectId), expiresAt);
        }

        _scheduled.Release();
    }

    private async Task RunExpiriesAsync(CancellationToken cancellationToken)
    {
        try
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                TimeSpan? untilNext = UntilNextExpiry();
                if (untilNext is null || untilNext > TimeSpan.Zero)
                {
                    await _scheduled.WaitAsync(untilNext ?? Timeout.InfiniteTimeSpan, cancellationToken);
                    continue;
                }

                while (TryTakeDue(out (int UserId, int EffectId) due))
                {
                    await ExpireAsync(due.UserId, due.EffectId, cancellationToken);
                }
            }
        }
        catch (OperationCanceledException)
        {
        }
    }

    private TimeSpan? UntilNextExpiry()
    {
        lock (_expiries)
        {
            if (!_expiries.TryPeek(out _, out DateTime expiresAt))
            {
                return null;
            }

            TimeSpan untilNext = expiresAt - DateTime.UtcNow;
            return untilNext > MaxSleep ? MaxSleep : untilNext;
        }
    }

    private bool TryTakeDue(out (int UserId, int EffectId) due)
    {
        lock (_expiries)
        {
            if (_expiries.TryPeek(out due, out DateTime expiresAt) && expiresAt <= DateTime.UtcNow)
            {
                _expiries.Dequeue();
                return true;
            }

            return false;
        }
    }

    private async Task ExpireAsync(int userId, int effectId, CancellationToken cancellationToken)
    {
        try
        {
            if (!await _repository.TryExpireAsync(userId, effectId, UtcNow(), cancellationToken)
                || !_sessionManager.TryGetByUserId(userId, out ClientSession? session))
            {
                return;
            }

            IGameSession online = session!;
            online.Send(new AvatarEffectExpiredMessageEvent(effectId).Compose());

            if (online.AvatarEffectId == effectId)
            {
                Wear(online, NoEffect);
            }
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            _logger.LogError(ex, "Couldn't expire avatar effect {EffectId} of user {UserId}.", effectId, userId);
        }
    }

    private static DateTime UtcNow() => DateTime.UtcNow.ToWholeSecond();
}
