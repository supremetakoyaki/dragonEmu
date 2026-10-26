using DragonEmu.Game.Furniture;
using DragonEmu.Game.Rooms;

namespace DragonEmu.Protocol.Messages.Incoming.RoomFurniture;

public sealed class EnterOneWayDoorMessageComposer : IMessageComposer
{
    private readonly IRoomManager _roomManager;
    private readonly OneWayDoorService _oneWayDoors;

    private int _itemId;

    public EnterOneWayDoorMessageComposer(IRoomManager roomManager, OneWayDoorService oneWayDoors)
    {
        _roomManager = roomManager;
        _oneWayDoors = oneWayDoors;
    }

    public void Parse(ClientPacket packet)
    {
        _itemId = packet.PopInt32();
    }

    public ValueTask HandleAsync(IGameSession session, CancellationToken cancellationToken)
    {
        if (session.CurrentRoomId is { } roomId
            && _roomManager.TryGetLoadedRoom(roomId, out Room? room)
            && room!.TryGetActor(session.Id, out RoomActor? actor)
            && room.GetFloorItem(_itemId) is { } door)
        {
            _oneWayDoors.Enter(room, actor!, door);
        }

        return ValueTask.CompletedTask;
    }
}
