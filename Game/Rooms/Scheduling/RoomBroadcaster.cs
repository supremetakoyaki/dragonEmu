using System.Collections.Concurrent;
using DragonEmu.Domain;
using System.Linq;
using DragonEmu.Gateway;
using DragonEmu.Protocol;

namespace DragonEmu.Game.Rooms;

public sealed class RoomBroadcaster
{
    private readonly ClientSessionManager _sessionManager;
    private readonly ConcurrentDictionary<int, ConcurrentQueue<Pending>> _pending = new();

    private readonly record struct Pending(byte[] Frame, byte[][]? ByLanguage, long? ExcludeSessionId, int? FromUserId);

    public RoomBroadcaster(ClientSessionManager sessionManager)
    {
        _sessionManager = sessionManager;
    }

    public void Forget(int roomId) => _pending.TryRemove(roomId, out _);

    public void Broadcast(Room room, ServerPacket packet, long? excludeSessionId = null) =>
        Broadcast(room, packet.ToArray(), excludeSessionId);

    public void Broadcast(Room room, byte[] frame, long? excludeSessionId = null) => Enqueue(room, frame, excludeSessionId, fromUserId: null);

    public void BroadcastFrom(Room room, ServerPacket packet, int fromUserId) => Enqueue(room, new Pending(packet.ToArray(), null, null, fromUserId));

    public void BroadcastLocalized(Room room, Func<Language, ServerPacket> compose)
    {
        byte[][] frames = [.. LanguageExtensions.FallbackOrder.Order().Select(language => compose(language).ToArray())];
        Enqueue(room, new Pending(frames[(int)Language.En], frames, null, null));
    }

    private void Enqueue(Room room, byte[] frame, long? excludeSessionId, int? fromUserId) =>
        Enqueue(room, new Pending(frame, null, excludeSessionId, fromUserId));

    private void Enqueue(Room room, Pending pending)
    {
        ConcurrentQueue<Pending> queue = _pending.GetOrAdd(room.Id, static _ => new ConcurrentQueue<Pending>());
        queue.Enqueue(pending);
    }

    public int SendToRightsHolders(Room room, ServerPacket packet)
    {
        byte[]? frame = null;
        int reached = 0;
        foreach (RoomActor actor in room.Actors)
        {
            if (actor.FlatControl is not null && _sessionManager.TryGet(actor.SessionId, out ClientSession? session))
            {
                session!.EnqueueSend(frame ??= packet.ToArray());
                reached++;
            }
        }

        return reached;
    }

    public IReadOnlyList<int> PendingRoomIds =>
        _pending.Where(kv => !kv.Value.IsEmpty).Select(kv => kv.Key).ToList();

    public void FlushPending(Room room)
    {
        if (!_pending.TryGetValue(room.Id, out ConcurrentQueue<Pending>? queue) || queue.IsEmpty)
        {
            return;
        }

        long[] audience = [.. room.Actors.Select(actor => actor.SessionId), .. room.Spectators];

        while (queue.TryDequeue(out Pending item))
        {
            foreach (long sessionId in audience)
            {
                if (sessionId == item.ExcludeSessionId)
                {
                    continue;
                }

                if (_sessionManager.TryGet(sessionId, out ClientSession? session)
                    && !(item.FromUserId is { } from && session!.IgnoredUsers.Contains(from)))
                {
                    session!.EnqueueSend(item.ByLanguage?[(int)session.Language] ?? item.Frame);
                }
            }
        }
    }
}
