namespace DragonEmu.Protocol.Messages.Outgoing.Navigator;

public sealed class FavouriteChangedEvent : IMessageEvent
{
    private readonly int _roomId;
    private readonly bool _added;

    public FavouriteChangedEvent(int roomId, bool added)
    {
        _roomId = roomId;
        _added = added;
    }

    public ServerPacket Compose()
    {
        var packet = new ServerPacket(OutgoingOpcodes.Navigator.FavouriteChangedEvent);
        packet.AppendInt32(_roomId);
        packet.AppendBoolean(_added);
        return packet;
    }
}
