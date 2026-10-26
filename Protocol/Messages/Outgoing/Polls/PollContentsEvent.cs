using DragonEmu.Domain;

namespace DragonEmu.Protocol.Messages.Outgoing.Polls;

public sealed class PollContentsEvent : IMessageEvent
{
    private readonly Poll _poll;
    private readonly IReadOnlyList<PollQuestion> _questions;

    public PollContentsEvent(Poll poll, IReadOnlyList<PollQuestion> questions)
    {
        _poll = poll;
        _questions = questions;
    }

    public ServerPacket Compose()
    {
        var packet = new ServerPacket(OutgoingOpcodes.Poll.PollContentsEvent);
        packet.AppendInt32(_poll.Id);
        packet.AppendString(_poll.Name);
        packet.AppendString(_poll.ThanksMessage);
        packet.AppendInt32(_questions.Count);
        foreach (PollQuestion question in _questions)
        {
            packet.AppendInt32(question.Id);
            packet.AppendInt32(question.Number);
            packet.AppendInt32((int)question.Type);
            packet.AppendString(question.Content);
            if (question.Type == PollQuestionType.Text)
            {
                continue;
            }

            packet.AppendInt32(question.Options.Count);
            packet.AppendInt32(question.SelectionMin);
            packet.AppendInt32(question.Type == PollQuestionType.Single ? 1 : question.SelectionMax);
            foreach (PollOption option in question.Options)
            {
                packet.AppendString(option.Content);
            }
        }

        return packet;
    }
}
