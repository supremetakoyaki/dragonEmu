namespace DragonEmu.Protocol.Messages.Outgoing.RoomEngine;

public sealed class HeightMapUpdateMessageEvent : IMessageEvent
{
    private readonly string _update;

    public HeightMapUpdateMessageEvent(string update)
    {
        _update = update;
    }

    public ServerPacket Compose()
    {
        var packet = new ServerPacket(OutgoingOpcodes.RoomEngine.HeightMapUpdateMessageEvent);
        packet.AppendString(_update);
        return packet;
    }
}
