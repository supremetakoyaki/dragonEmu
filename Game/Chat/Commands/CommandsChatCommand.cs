using DragonEmu.Protocol.Messages;
using DragonEmu.Protocol.Messages.Outgoing.Moderation;

namespace DragonEmu.Game.Chat.Commands;

public sealed class CommandsChatCommand : IChatCommand
{
    private readonly IChatCommandDispatcher _dispatcher;

    public CommandsChatCommand(IChatCommandDispatcher dispatcher)
    {
        _dispatcher = dispatcher;
    }

    public string Name => "commands";

    public string? RequiredFuse => null;

    public async ValueTask ExecuteAsync(IGameSession session, string[] args, CancellationToken cancellationToken)
    {
        if (session.AuthenticatedUser is not { } account)
        {
            return;
        }

        IReadOnlyList<string> commands = await _dispatcher.GetAvailableCommandsAsync(account, cancellationToken);
        session.Send(ModMessageEvent.FromLines(commands).Compose());
    }
}
