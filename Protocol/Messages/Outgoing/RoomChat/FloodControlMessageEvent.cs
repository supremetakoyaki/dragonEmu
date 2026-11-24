namespace DragonEmu.Protocol.Messages.Outgoing.RoomChat;

public sealed class FloodControlMessageEvent : IMessageEvent
{
    private readonly int _seconds;

    public FloodControlMessageEvent(int seconds)
    {
        _seconds = seconds;
    }

    public ServerPacket Compose()
    {
        var packet = new ServerPacket(OutgoingOpcodes.RoomChat.FloodControlMessageEvent);
        packet.AppendInt32(_seconds);
        return packet;
    }
}
