using DragonEmu.Game.Rooms;
using DragonEmu.Protocol.Messages.Outgoing.RoomSession;

namespace DragonEmu.Protocol.Messages.Incoming.RoomSession;

public sealed class QuitRoomMessageComposer : IMessageComposer
{
    private readonly IRoomManager _roomManager;
    private readonly RoomBroadcaster _roomBroadcaster;

    private readonly RoomQueueService _queue;

    public QuitRoomMessageComposer(IRoomManager roomManager, RoomBroadcaster roomBroadcaster, RoomQueueService queue)
    {
        _roomManager = roomManager;
        _roomBroadcaster = roomBroadcaster;
        _queue = queue;
    }

    public void Parse(ClientPacket packet)
    {
    }

    public ValueTask HandleAsync(IGameSession session, CancellationToken cancellationToken)
    {
        if (_queue.Leave(session))
        {
            session.Send(new CloseConnectionMessageEvent().Compose());
        }

        if (session.CurrentRoomId is { } roomId)
        {
            _roomManager.TryGetLoadedRoom(roomId, out Room? room);
            RoomExit.ToHotelView(session, room, _roomBroadcaster);
        }

        return ValueTask.CompletedTask;
    }
}
