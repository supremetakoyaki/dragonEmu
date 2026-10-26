namespace DragonEmu.Protocol.Messages.Outgoing.Users;

public sealed class UserTagsMessageEvent : IMessageEvent
{
    private readonly int _userId;
    private readonly IReadOnlyList<string> _tags;

    public UserTagsMessageEvent(int userId, IReadOnlyList<string> tags)
    {
        _userId = userId;
        _tags = tags;
    }

    public ServerPacket Compose()
    {
        var packet = new ServerPacket(OutgoingOpcodes.Users.UserTagsMessageEvent);
        packet.AppendInt32(_userId);
        packet.AppendInt32(_tags.Count);
        foreach (string tag in _tags)
        {
            packet.AppendString(tag);
        }

        return packet;
    }
}
