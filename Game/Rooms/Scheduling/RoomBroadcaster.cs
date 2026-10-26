using System.Collections.Concurrent;
using System.Linq;
using DragonEmu.Gateway;
using DragonEmu.Protocol;

namespace DragonEmu.Game.Rooms;

public sealed class RoomBroadcaster
{
    private readonly ClientSessionManager _sessionManager;
    private readonly ConcurrentDictionary<int, ConcurrentQueue<(byte[] Frame, long? ExcludeSessionId, int? FromUserId)>> _pending = new();

    public RoomBroadcaster(ClientSessionManager sessionManager)
    {
        _sessionManager = sessionManager;
    }

    public void Forget(int roomId) => _pending.TryRemove(roomId, out _);

    public void Broadcast(Room room, ServerPacket packet, long? excludeSessionId = null) =>
        Broadcast(room, packet.ToArray(), excludeSessionId);

    public void Broadcast(Room room, byte[] frame, long? excludeSessionId = null) => Enqueue(room, frame, excludeSessionId, fromUserId: null);

    public void BroadcastFrom(Room room, ServerPacket packet, int fromUserId) => Enqueue(room, packet.ToArray(), excludeSessionId: null, fromUserId);

    private void Enqueue(Room room, byte[] frame, long? excludeSessionId, int? fromUserId)
    {
        ConcurrentQueue<(byte[], long?, int?)> queue = _pending.GetOrAdd(room.Id, static _ => new ConcurrentQueue<(byte[], long?, int?)>());
        queue.Enqueue((frame, excludeSessionId, fromUserId));
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
        if (!_pending.TryGetValue(room.Id, out ConcurrentQueue<(byte[] Frame, long? ExcludeSessionId, int? FromUserId)>? queue) || queue.IsEmpty)
        {
            return;
        }

        IReadOnlyCollection<RoomActor> actors = room.Actors;

        while (queue.TryDequeue(out (byte[] Frame, long? ExcludeSessionId, int? FromUserId) item))
        {
            foreach (RoomActor actor in actors)
            {
                if (actor.SessionId == item.ExcludeSessionId)
                {
                    continue;
                }

                if (_sessionManager.TryGet(actor.SessionId, out ClientSession? session)
                    && !(item.FromUserId is { } from && session!.IgnoredUsers.Contains(from)))
                {
                    session!.EnqueueSend(item.Frame);
                }
            }
        }
    }
}
