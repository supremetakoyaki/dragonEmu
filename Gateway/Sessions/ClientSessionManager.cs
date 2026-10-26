using System.Collections.Concurrent;
using DragonEmu.Game.Rooms;
using DragonEmu.Protocol.Messages.Outgoing.RoomEngine;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace DragonEmu.Gateway;

public sealed class ClientSessionManager
{
    private readonly ConcurrentDictionary<long, ClientSession> _sessions = new();

    private readonly ConcurrentDictionary<int, ClientSession> _byUserId = new();
    private readonly Lock _loginLock = new();
    private readonly GameServerOptions _options;
    private readonly IRoomManager _roomManager;
    private readonly ILogger<ClientSessionManager> _logger;
    private long _nextId;

    public int Count => _sessions.Count;

    public int OnlineUserCount => _byUserId.Count;

    public ClientSessionManager(
        IOptions<GameServerOptions> options, IRoomManager roomManager, ILogger<ClientSessionManager> logger)
    {
        _options = options.Value;
        _roomManager = roomManager;
        _logger = logger;
    }

    public bool TryAcquireSlot() => _sessions.Count < _options.MaxConnections;

    public bool TryGet(long sessionId, out ClientSession? session) => _sessions.TryGetValue(sessionId, out session);

    public bool TryGetByUserId(int userId, out ClientSession? session) => _byUserId.TryGetValue(userId, out session);

    public bool IsOnline(int userId) => _byUserId.ContainsKey(userId);

    public IReadOnlyList<ClientSession> SetLoggedIn(long sessionId)
    {
        if (!_sessions.TryGetValue(sessionId, out ClientSession? session) || session.AuthenticatedUser is not { } user)
        {
            return [];
        }

        lock (_loginLock)
        {
            if (session.IsReplaced)
            {
                return [];
            }

            var replaced = new List<ClientSession>();
            foreach (ClientSession other in _sessions.Values)
            {
                if (other.Id != sessionId && other.AuthenticatedUser?.Id == user.Id && other.MarkReplaced())
                {
                    replaced.Add(other);
                }
            }

            _byUserId[user.Id] = session;
            return replaced;
        }
    }

    public long AllocateId() => Interlocked.Increment(ref _nextId);

    public IReadOnlyList<int> GetOnlineUserIds() => _byUserId.Keys.ToList();

    public IReadOnlyList<ClientSession> GetAuthenticatedSessions() =>
        _sessions.Values.Where(session => session.AuthenticatedUser is not null).ToList();

    public void BroadcastToAll(byte[] frame)
    {
        foreach (ClientSession session in _sessions.Values)
        {
            session.EnqueueSend(frame);
        }
    }

    public void Add(ClientSession session)
    {
        _sessions[session.Id] = session;
        _logger.LogInformation(
            "Session {SessionId} connected from {RemoteAddress} ({Count}/{Max})",
            session.Id, session.RemoteAddress, _sessions.Count, _options.MaxConnections);
    }

    public bool Remove(ClientSession session)
    {
        if (!_sessions.TryRemove(session.Id, out _))
        {
            return false;
        }

        if (session.AuthenticatedUser is { } loggedIn)
        {
            _byUserId.TryRemove(new KeyValuePair<int, ClientSession>(loggedIn.Id, session));
        }

        if (session.CurrentRoomId is { } roomId
            && _roomManager.TryGetLoadedRoom(roomId, out Room? room)
            && room!.TryRemoveActor(session.Id, out RoomActor? actor))
        {
            byte[] frame = new UserRemoveMessageEvent(actor!.VirtualId).Compose().ToArray();
            foreach (RoomActor remaining in room.Actors)
            {
                if (_sessions.TryGetValue(remaining.SessionId, out ClientSession? peer))
                {
                    peer.EnqueueSend(frame);
                }
            }
        }

        _logger.LogInformation(
            "Session {SessionId} disconnected ({Count}/{Max})",
            session.Id, _sessions.Count, _options.MaxConnections);
        return true;
    }
}
