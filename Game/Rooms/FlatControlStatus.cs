using DragonEmu.Protocol.Messages.Outgoing.RoomEngine;

namespace DragonEmu.Game.Rooms;

public static class FlatControlStatus
{
    public const string Owner = "useradmin";

    public const string RightsHolder = "";

    public static void Show(Room room, RoomActor actor, bool hasRights, RoomBroadcaster broadcaster)
    {
        lock (actor.Lock)
        {
            if (actor.FlatControl == Owner)
            {
                return;
            }

            actor.FlatControl = hasRights ? RightsHolder : null;
        }

        broadcaster.Broadcast(room, new UserUpdateMessageEvent([actor]).Compose());
    }
}
