using DragonEmu.Game.Rooms;

namespace DragonEmu.Protocol.Messages.Incoming.RoomSession;

public sealed class QuitRoomMessageComposer : IMessageComposer
{
    private readonly IRoomManager _roomManager;
    private readonly RoomBroadcaster _roomBroadcaster;

    public QuitRoomMessageComposer(IRoomManager roomManager, RoomBroadcaster roomBroadcaster)
    {
        _roomManager = roomManager;
        _roomBroadcaster = roomBroadcaster;
    }

    public void Parse(ClientPacket packet)
    {
    }

    public ValueTask HandleAsync(IGameSession session, CancellationToken cancellationToken)
    {
        if (session.CurrentRoomId is { } roomId)
        {
            _roomManager.TryGetLoadedRoom(roomId, out Room? room);
            RoomExit.ToHotelView(session, room, _roomBroadcaster);
        }

        return ValueTask.CompletedTask;
    }
}
