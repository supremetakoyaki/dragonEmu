namespace DragonEmu.Protocol.Messages.Outgoing.Handshake;

public sealed class UserRightsMessageEvent : IMessageEvent
{
    private readonly IReadOnlyList<string> _fuses;

    public UserRightsMessageEvent(IReadOnlyList<string> fuses)
    {
        _fuses = fuses;
    }

    public ServerPacket Compose()
    {
        var packet = new ServerPacket(OutgoingOpcodes.Handshake.UserRightsMessageEvent);
        packet.AppendInt32(_fuses.Count);

        foreach (string fuse in _fuses)
        {
            packet.AppendString(fuse);
        }

        return packet;
    }
}
