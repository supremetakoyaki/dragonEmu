namespace DragonEmu.Protocol.Messages.Outgoing.Navigator;

public sealed class FavouritesEvent : IMessageEvent
{
    private readonly int _limit;
    private readonly IReadOnlyList<int> _roomIds;

    public FavouritesEvent(int limit, IReadOnlyList<int> roomIds)
    {
        _limit = limit;
        _roomIds = roomIds;
    }

    public ServerPacket Compose()
    {
        var packet = new ServerPacket(OutgoingOpcodes.Navigator.FavouritesEvent);
        packet.AppendInt32(_limit);
        packet.AppendInt32(_roomIds.Count);

        foreach (int roomId in _roomIds)
        {
            packet.AppendInt32(roomId);
        }

        return packet;
    }
}
