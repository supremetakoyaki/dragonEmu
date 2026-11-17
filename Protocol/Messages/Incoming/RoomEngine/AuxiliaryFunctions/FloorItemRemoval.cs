using DragonEmu.Game.Rooms;
using DragonEmu.Protocol.Messages.Outgoing.RoomEngine;
using DragonEmu.Protocol.Messages.Outgoing.Trax;

namespace DragonEmu.Protocol.Messages.Incoming.RoomEngine.AuxiliaryFunctions;

internal static class FloorItemRemoval
{
    public static void RemoveFromRoom(Room room, int itemId, RoomBroadcaster broadcaster)
    {
        RoomFloorItem? floorItem = room.GetFloorItem(itemId);
        HashSet<(int X, int Y)> footprint = floorItem is not null ? floorItem.OccupiedTiles().ToHashSet() : [];

        room.RemoveFloorItem(itemId);
        broadcaster.Broadcast(room, new ObjectRemoveMessageEvent(itemId).Compose());
        if (floorItem is not null && Room.IsMusicFurni(floorItem.LogicClass))
        {
            broadcaster.Broadcast(room, new SoundMachineClosedMessageEvent(itemId).Compose());
        }

        List<RoomActor> resettled = room.SettleOn(footprint);
        if (resettled.Count > 0)
        {
            broadcaster.Broadcast(room, new UserUpdateMessageEvent(resettled).Compose());
        }
    }
}
