using DragonEmu.Game.Rooms;
using DragonEmu.Protocol.Messages.Outgoing.RoomEngine;
using DragonEmu.Protocol.Messages.Outgoing.Trax;

namespace DragonEmu.Protocol.Messages.Incoming.RoomEngine.AuxiliaryFunctions;

internal static class FloorItemRemoval
{
    public static void RemoveFromRoom(Room room, int itemId, RoomBroadcaster broadcaster)
    {
        RoomFloorItem? floorItem = room.GetFloorItem(itemId);
        IReadOnlyList<long> resting = floorItem is not null ? room.RestingActorsOn(floorItem) : [];

        room.RemoveFloorItem(itemId);
        broadcaster.Broadcast(room, new ObjectRemoveMessageEvent(itemId).Compose());
        if (floorItem is not null && Room.IsMusicFurni(floorItem.LogicClass))
        {
            broadcaster.Broadcast(room, new SoundMachineClosedMessageEvent(itemId).Compose());
        }

        var resettled = new List<RoomActor>();
        foreach (long sessionId in resting)
        {
            if (room.TryGetActor(sessionId, out RoomActor? actor))
            {
                lock (actor!.Lock)
                {
                    if (room.ApplyTilePosture(actor))
                    {
                        resettled.Add(actor);
                    }
                }
            }
        }

        if (resettled.Count > 0)
        {
            broadcaster.Broadcast(room, new UserUpdateMessageEvent(resettled).Compose());
        }
    }
}
