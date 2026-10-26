namespace DragonEmu.Protocol.Messages.Outgoing.Navigator;

public sealed class PublicSpaceCastLibsEvent : IMessageEvent
{
    private readonly int _roomId;
    private readonly string _libraryName;

    public PublicSpaceCastLibsEvent(int roomId, string libraryName)
    {
        _roomId = roomId;
        _libraryName = libraryName;
    }

    public ServerPacket Compose()
    {
        var packet = new ServerPacket(OutgoingOpcodes.Navigator.PublicSpaceCastLibsEvent);
        packet.AppendInt32(_roomId);
        packet.AppendString(_libraryName);
        packet.AppendInt32(_roomId);
        return packet;
    }
}
