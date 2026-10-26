namespace DragonEmu.Protocol.Messages.Outgoing.Votes;

public sealed class VoteQuestionMessageEvent : IMessageEvent
{
    private readonly string _question;
    private readonly IReadOnlyList<string> _choices;

    public VoteQuestionMessageEvent(string question, IReadOnlyList<string> choices)
    {
        _question = question;
        _choices = choices;
    }

    public ServerPacket Compose()
    {
        var packet = new ServerPacket(OutgoingOpcodes.Poll.VoteQuestionMessageEvent);
        packet.AppendString(_question);
        packet.AppendInt32(_choices.Count);
        for (int i = 0; i < _choices.Count; i++)
        {
            packet.AppendInt32(i + 1);
            packet.AppendString(_choices[i]);
        }

        return packet;
    }
}
