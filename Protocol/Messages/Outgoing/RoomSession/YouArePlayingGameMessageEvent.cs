namespace DragonEmu.Protocol.Messages.Outgoing.RoomSession;

public sealed class YouArePlayingGameMessageEvent : IMessageEvent
{
    private readonly bool _isPlaying;

    public YouArePlayingGameMessageEvent(bool isPlaying)
    {
        _isPlaying = isPlaying;
    }

    public ServerPacket Compose()
    {
        var packet = new ServerPacket(OutgoingOpcodes.RoomSession.YouArePlayingGameMessageEvent);
        packet.AppendBoolean(_isPlaying);
        return packet;
    }
}
