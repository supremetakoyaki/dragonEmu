using DragonEmu.Game.Settings;
using DragonEmu.Game.Rooms;
using DragonEmu.Persistence;
using DragonEmu.Protocol;
using DragonEmu.Protocol.Messages.Outgoing.RoomPublicRoom;

namespace DragonEmu.Game.Chat.Commands;

internal static class InfobusChatCommandHandler
{
    private const string InfobusHeightmapLabel = "park_a";

    public static async ValueTask SetStatusAsync(
        GameSettingsCache gameSettings, IRoomManager roomManager, RoomBroadcaster roomBroadcaster, PublicRoomTransfers transfers,
        bool open, CancellationToken cancellationToken)
    {
        await gameSettings.SetInfobusOpenAsync(open, cancellationToken);
        if (!open)
        {
            transfers.EmptyInfobus();
        }

        ServerPacket packet = new ParkBusDoorMessageEvent(open).Compose();
        foreach (Room room in roomManager.GetLoadedRoomsByHeightmapLabel(InfobusHeightmapLabel))
        {
            roomBroadcaster.Broadcast(room, packet);
        }
    }
}
