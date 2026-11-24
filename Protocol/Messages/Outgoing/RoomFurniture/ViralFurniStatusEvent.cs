namespace DragonEmu.Protocol.Messages.Outgoing.RoomFurniture;

public sealed class ViralFurniStatusEvent : IMessageEvent
{
    private readonly int _objectId;
    private readonly int _status;
    private readonly string _shareId;
    private readonly string _firstClickUserName;

    public ViralFurniStatusEvent(int objectId, int status, string shareId, string firstClickUserName)
    {
        _objectId = objectId;
        _status = status;
        _shareId = shareId;
        _firstClickUserName = firstClickUserName;
    }

    public ServerPacket Compose()
    {
        var packet = new ServerPacket(OutgoingOpcodes.RoomFurniture.ViralFurniStatusEvent);
        packet.AppendInt32(_objectId);
        packet.AppendInt32(_status);
        packet.AppendString(_shareId);
        packet.AppendString(_firstClickUserName);
        return packet;
    }
}
