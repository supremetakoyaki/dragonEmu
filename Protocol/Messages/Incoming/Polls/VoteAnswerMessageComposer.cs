using DragonEmu.Game.Polls;

namespace DragonEmu.Protocol.Messages.Incoming.Polls;

public sealed class VoteAnswerMessageComposer : IMessageComposer
{
    private readonly RoomVoteService _votes;

    private int _choice;

    public VoteAnswerMessageComposer(RoomVoteService votes)
    {
        _votes = votes;
    }

    public void Parse(ClientPacket packet)
    {
        _choice = packet.PopInt32();
    }

    public ValueTask HandleAsync(IGameSession session, CancellationToken cancellationToken)
    {
        if (session.AuthenticatedUser is { } account)
        {
            _votes.Answer(session, account, _choice);
        }

        return ValueTask.CompletedTask;
    }
}
