using DragonEmu.Game.Polls;
using DragonEmu.Protocol.Messages;
using DragonEmu.Protocol.Messages.Outgoing.Moderation;

namespace DragonEmu.Game.Chat.Commands;

public sealed class StartVoteChatCommand : IChatCommand
{
    private readonly RoomVoteService _votes;

    public StartVoteChatCommand(RoomVoteService votes)
    {
        _votes = votes;
    }

    public string Name => "start_vote";

    public string RequiredFuse => "fuse_create_voting";

    public ValueTask ExecuteAsync(IGameSession session, string[] args, CancellationToken cancellationToken)
    {
        if (session.AuthenticatedUser is { } account && _votes.Start(session, account, string.Join(' ', args)) is { } refusal)
        {
            session.Send(new ModMessageEvent(refusal).Compose());
        }

        return ValueTask.CompletedTask;
    }
}

public sealed class EndVoteChatCommand : IChatCommand
{
    private readonly RoomVoteService _votes;

    public EndVoteChatCommand(RoomVoteService votes)
    {
        _votes = votes;
    }

    public string Name => "end_vote";

    public string RequiredFuse => "fuse_create_voting";

    public ValueTask ExecuteAsync(IGameSession session, string[] args, CancellationToken cancellationToken)
    {
        if (session.AuthenticatedUser is not null && _votes.EndByCommand(session) is { } refusal)
        {
            session.Send(new ModMessageEvent(refusal).Compose());
        }

        return ValueTask.CompletedTask;
    }
}
