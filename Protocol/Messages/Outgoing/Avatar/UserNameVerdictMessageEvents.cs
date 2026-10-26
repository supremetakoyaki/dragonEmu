namespace DragonEmu.Protocol.Messages.Outgoing.Avatar;

public abstract class UserNameVerdictMessageEvent : IMessageEvent
{
    private readonly int _opcode;
    private readonly int _code;
    private readonly string _name;
    private readonly IReadOnlyList<string> _suggestions;

    protected UserNameVerdictMessageEvent(int opcode, int code, string name, IReadOnlyList<string> suggestions)
    {
        _opcode = opcode;
        _code = code;
        _name = name;
        _suggestions = suggestions;
    }

    public ServerPacket Compose()
    {
        var packet = new ServerPacket(_opcode);
        packet.AppendInt32(_code);
        packet.AppendString(_name);
        packet.AppendInt32(_suggestions.Count);
        foreach (string suggestion in _suggestions)
        {
            packet.AppendString(suggestion);
        }

        return packet;
    }
}

public sealed class ChangeUserNameResultMessageEvent : UserNameVerdictMessageEvent
{
    public ChangeUserNameResultMessageEvent(int code, string name, IReadOnlyList<string> suggestions)
        : base(OutgoingOpcodes.Avatar.ChangeUserNameResultMessageEvent, code, name, suggestions)
    {
    }
}

public sealed class CheckUserNameResultMessageEvent : UserNameVerdictMessageEvent
{
    public CheckUserNameResultMessageEvent(int code, string name, IReadOnlyList<string> suggestions)
        : base(OutgoingOpcodes.Avatar.CheckUserNameResultMessageEvent, code, name, suggestions)
    {
    }
}
