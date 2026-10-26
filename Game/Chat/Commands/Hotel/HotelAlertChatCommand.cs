using DragonEmu.Gateway;
using DragonEmu.Protocol.Messages;
using DragonEmu.Protocol.Messages.Outgoing.Notifications;

namespace DragonEmu.Game.Chat.Commands;

public sealed class HotelAlertChatCommand : IChatCommand
{
    private readonly ClientSessionManager _sessionManager;

    public HotelAlertChatCommand(ClientSessionManager sessionManager)
    {
        _sessionManager = sessionManager;
    }

    public string Name => "hotelalert";

    public string RequiredFuse => "fuse_administrator_access";

    public ValueTask ExecuteAsync(IGameSession session, string[] args, CancellationToken cancellationToken)
    {
        if (args.Length == 0)
        {
            return ValueTask.CompletedTask;
        }

        string message = string.Join(' ', args);
        _sessionManager.BroadcastToAll(new HabboBroadcastMessageEvent(message).Compose().ToArray());
        return ValueTask.CompletedTask;
    }
}
