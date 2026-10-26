using DragonEmu.Game.Rooms;
using DragonEmu.Protocol.Messages.Outgoing.RoomEngine;

namespace DragonEmu.Protocol.Messages.Incoming.RoomEngine.AuxiliaryFunctions;

internal static class FloorItemRemoval
{
    public static void RemoveFromRoom(Room room, int itemId, RoomBroadcaster broadcaster)
    {
        IReadOnlyList<long> resting = room.GetFloorItem(itemId) is { } floorItem ? room.RestingActorsOn(floorItem) : [];

        room.RemoveFloorItem(itemId);
        broadcaster.Broadcast(room, new ObjectRemoveMessageEvent(itemId).Compose());

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
