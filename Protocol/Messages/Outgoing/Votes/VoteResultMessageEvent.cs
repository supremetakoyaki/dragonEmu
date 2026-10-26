namespace DragonEmu.Protocol.Messages.Outgoing.Votes;

public sealed class VoteResultMessageEvent : IMessageEvent
{
    private readonly string _question;
    private readonly IReadOnlyList<string> _choices;
    private readonly IReadOnlyList<int> _votes;

    public VoteResultMessageEvent(string question, IReadOnlyList<string> choices, IReadOnlyList<int> votes)
    {
        _question = question;
        _choices = choices;
        _votes = votes;
    }

    public ServerPacket Compose()
    {
        var packet = new ServerPacket(OutgoingOpcodes.Poll.VoteResultMessageEvent);
        packet.AppendString(_question);
        packet.AppendInt32(_choices.Count);
        for (int i = 0; i < _choices.Count; i++)
        {
            packet.AppendInt32(i + 1);
            packet.AppendString(_choices[i]);
            packet.AppendInt32(_votes[i]);
        }

        packet.AppendInt32(_votes.Sum());
        return packet;
    }
}
