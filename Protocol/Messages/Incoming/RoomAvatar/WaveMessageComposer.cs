using DragonEmu.Game.Rooms;
using DragonEmu.Protocol.Messages.Outgoing.RoomAction;

namespace DragonEmu.Protocol.Messages.Incoming.RoomAvatar;

public sealed class WaveMessageComposer : IMessageComposer
{
    private readonly IRoomManager _roomManager;
    private readonly RoomBroadcaster _broadcaster;

    public WaveMessageComposer(IRoomManager roomManager, RoomBroadcaster broadcaster)
    {
        _roomManager = roomManager;
        _broadcaster = broadcaster;
    }

    public void Parse(ClientPacket packet)
    {
    }

    public async ValueTask HandleAsync(IGameSession session, CancellationToken cancellationToken)
    {
        if (session.AuthenticatedUser is null || session.CurrentRoomId is not { } roomId)
        {
            return;
        }

        Room? room = await _roomManager.GetOrLoadRoomAsync(roomId, cancellationToken);
        if (room is null || !room.TryGetActor(session.Id, out RoomActor? actor))
        {
            return;
        }

        lock (actor!.Lock)
        {
            actor.DanceId = 0;
        }

        _broadcaster.Broadcast(room, new WaveMessageEvent(actor.VirtualId).Compose());
    }
}
