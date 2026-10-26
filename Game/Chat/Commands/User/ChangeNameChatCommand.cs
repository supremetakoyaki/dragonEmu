using DragonEmu.Game.Avatars;
using DragonEmu.Protocol.Messages;
using DragonEmu.Protocol.Messages.Outgoing.Help;
using DragonEmu.Protocol.Messages.Outgoing.Moderation;

namespace DragonEmu.Game.Chat.Commands;

public sealed class ChangeNameChatCommand : IChatCommand
{
    private readonly UsernameChangeService _usernameChange;

    public ChangeNameChatCommand(UsernameChangeService usernameChange)
    {
        _usernameChange = usernameChange;
    }

    public string Name => "changename";

    public string? RequiredFuse => null;

    public async ValueTask ExecuteAsync(IGameSession session, string[] args, CancellationToken cancellationToken)
    {
        if (session.AuthenticatedUser is not { } account)
        {
            return;
        }

        session.Send(await _usernameChange.DescribeAsync(account, cancellationToken));
        if (await _usernameChange.MayChangeAsync(account, cancellationToken))
        {
            session.NameChangeDialogOpen = true;
            session.Send(new HotelMergeNameChangeEvent().Compose());
        }
    }
}
