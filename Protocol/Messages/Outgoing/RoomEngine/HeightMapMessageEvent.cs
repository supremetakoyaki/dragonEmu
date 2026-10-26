namespace DragonEmu.Protocol.Messages.Outgoing.RoomEngine;

public sealed class HeightMapMessageEvent : IMessageEvent
{
    private readonly string _stackingMap;

    public HeightMapMessageEvent(string stackingMap)
    {
        _stackingMap = stackingMap;
    }

    public ServerPacket Compose()
    {
        var packet = new ServerPacket(OutgoingOpcodes.RoomEngine.HeightMapMessageEvent);
        packet.AppendString(_stackingMap);
        return packet;
    }
}
