namespace DragonEmu.Protocol.Messages.Outgoing.Navigator;

public sealed class PopularRoomTagsResultEvent : IMessageEvent
{
    private readonly IReadOnlyList<(string Tag, int UsersNow)> _tags;

    public PopularRoomTagsResultEvent(IReadOnlyList<(string Tag, int UsersNow)> tags)
    {
        _tags = tags;
    }

    public ServerPacket Compose()
    {
        var packet = new ServerPacket(OutgoingOpcodes.Navigator.PopularRoomTagsResultEvent);
        packet.AppendInt32(_tags.Count);
        foreach ((string tag, int usersNow) in _tags)
        {
            packet.AppendString(tag);
            packet.AppendInt32(usersNow);
        }

        return packet;
    }
}
