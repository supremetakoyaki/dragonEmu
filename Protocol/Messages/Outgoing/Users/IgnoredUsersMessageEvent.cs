namespace DragonEmu.Protocol.Messages.Outgoing.Users;

public sealed class IgnoredUsersMessageEvent : IMessageEvent
{
    private readonly IReadOnlyList<string> _usernames;

    public IgnoredUsersMessageEvent(IReadOnlyList<string> usernames)
    {
        _usernames = usernames;
    }

    public ServerPacket Compose()
    {
        var packet = new ServerPacket(OutgoingOpcodes.Users.IgnoredUsersMessageEvent);
        packet.AppendInt32(_usernames.Count);
        foreach (string username in _usernames)
        {
            packet.AppendString(username);
        }

        return packet;
    }
}
