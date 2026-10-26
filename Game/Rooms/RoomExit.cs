using DragonEmu.Protocol.Messages;
using DragonEmu.Protocol.Messages.Outgoing.RoomEngine;
using DragonEmu.Protocol.Messages.Outgoing.RoomSession;

namespace DragonEmu.Game.Rooms;

public static class RoomExit
{
    public static void ToHotelView(IGameSession session, Room? room, RoomBroadcaster broadcaster)
    {
        if (room is not null && room.TryRemoveActor(session.Id, out RoomActor? actor))
        {
            broadcaster.Broadcast(room, new UserRemoveMessageEvent(actor!.VirtualId).Compose());
        }

        session.SetCurrentRoom(null);
        session.Send(new CloseConnectionMessageEvent().Compose());
    }
}
