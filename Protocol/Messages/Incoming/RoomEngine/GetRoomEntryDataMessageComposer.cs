using DragonEmu.Game.Rooms;
using DragonEmu.Protocol.Messages.Outgoing.RoomEngine;

namespace DragonEmu.Protocol.Messages.Incoming.RoomEngine;

public sealed class GetRoomEntryDataMessageComposer : IMessageComposer
{
    private readonly IRoomManager _roomManager;

    public GetRoomEntryDataMessageComposer(IRoomManager roomManager)
    {
        _roomManager = roomManager;
    }

    public void Parse(ClientPacket packet)
    {
    }

    public async ValueTask HandleAsync(IGameSession session, CancellationToken cancellationToken)
    {
        int? roomId = session.LoadingRoomId ?? session.CurrentRoomId;
        if (roomId is null)
        {
            return;
        }

        Room? room = await _roomManager.GetOrLoadRoomAsync(roomId.Value, cancellationToken);
        if (room is null)
        {
            return;
        }

        session.Send(new HeightMapMessageEvent(room.StackingMap()).Compose());
        session.Send(new FloorHeightMapMessageEvent(room.Data.Heightmap, room.Data.DoorX, room.Data.DoorY, room.Data.DoorZ).Compose());
    }
}
