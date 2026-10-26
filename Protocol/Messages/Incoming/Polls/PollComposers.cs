using DragonEmu.Game.Polls;

namespace DragonEmu.Protocol.Messages.Incoming.Polls;

public sealed class PollStartComposer : IMessageComposer
{
    private readonly PollService _polls;

    private int _pollId;

    public PollStartComposer(PollService polls)
    {
        _polls = polls;
    }

    public void Parse(ClientPacket packet)
    {
        _pollId = packet.PopInt32();
    }

    public async ValueTask HandleAsync(IGameSession session, CancellationToken cancellationToken)
    {
        if (session.AuthenticatedUser is { } account)
        {
            await _polls.BeginAsync(session, account, _pollId, cancellationToken);
        }
    }
}

public sealed class PollRejectComposer : IMessageComposer
{
    private int _pollId;

    public void Parse(ClientPacket packet)
    {
        _pollId = packet.PopInt32();
    }

    public ValueTask HandleAsync(IGameSession session, CancellationToken cancellationToken)
    {
        PollService.Reject(session, _pollId);
        return ValueTask.CompletedTask;
    }
}

public sealed class PollAnswerComposer : IMessageComposer
{
    private const int MaxAnswers = 50;

    private readonly PollService _polls;

    private int _pollId;
    private int _questionId;
    private List<string> _answers = [];

    public PollAnswerComposer(PollService polls)
    {
        _polls = polls;
    }

    public void Parse(ClientPacket packet)
    {
        _pollId = packet.PopInt32();
        _questionId = packet.PopInt32();
        int count = packet.PopInt32();
        _answers = [];
        for (int i = 0; i < Math.Clamp(count, 0, MaxAnswers); i++)
        {
            _answers.Add(packet.PopString());
        }

        if (count > MaxAnswers)
        {
            _answers.Add(string.Empty);
        }
    }

    public async ValueTask HandleAsync(IGameSession session, CancellationToken cancellationToken)
    {
        if (session.AuthenticatedUser is { } account)
        {
            await _polls.AnswerAsync(session, account, _pollId, _questionId, _answers, cancellationToken);
        }
    }
}
