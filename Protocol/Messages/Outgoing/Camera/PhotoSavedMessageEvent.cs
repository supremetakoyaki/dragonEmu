namespace DragonEmu.Protocol.Messages.Outgoing.Camera;

public sealed class PhotoSavedMessageEvent : IMessageEvent
{
    private readonly bool _saved;

    public PhotoSavedMessageEvent(bool saved)
    {
        _saved = saved;
    }

    public ServerPacket Compose()
    {
        var packet = new ServerPacket(OutgoingOpcodes.Camera.PhotoSavedMessageEvent);
        packet.AppendBoolean(_saved);
        return packet;
    }
}
