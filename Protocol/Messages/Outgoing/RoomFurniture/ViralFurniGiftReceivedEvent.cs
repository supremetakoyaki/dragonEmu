namespace DragonEmu.Protocol.Messages.Outgoing.RoomFurniture;

public sealed class ViralFurniGiftReceivedEvent : IMessageEvent
{
    private readonly string _firstClickUserName;

    public ViralFurniGiftReceivedEvent(string firstClickUserName)
    {
        _firstClickUserName = firstClickUserName;
    }

    public ServerPacket Compose()
    {
        var packet = new ServerPacket(OutgoingOpcodes.RoomFurniture.ViralFurniGiftReceivedEvent);
        packet.AppendString(_firstClickUserName);
        return packet;
    }
}
