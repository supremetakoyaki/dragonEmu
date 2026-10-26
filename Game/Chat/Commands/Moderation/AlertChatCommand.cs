using DragonEmu.Gateway;
using DragonEmu.Persistence;
using DragonEmu.Protocol.Messages;
using DragonEmu.Protocol.Messages.Outgoing.Moderation;
using DragonEmu.Game.Settings;

namespace DragonEmu.Game.Chat.Commands;

public sealed class AlertChatCommand : IChatCommand
{
    private readonly IUserRepository _users;
    private readonly ClientSessionManager _sessions;
    private readonly MessageTexts _texts;

    public AlertChatCommand(IUserRepository users, ClientSessionManager sessions, MessageTexts texts)
    {
        _texts = texts;
        _users = users;
        _sessions = sessions;
    }

    public string Name => "alert";

    public string RequiredFuse => "fuse_alert";

    public async ValueTask ExecuteAsync(IGameSession session, string[] args, CancellationToken cancellationToken)
    {
        if (args.Length < 2)
        {
            session.Send(new ModMessageEvent(_texts.Get(MessageKeys.UsageAlert)).Compose());
            return;
        }

        if (await _users.GetByUsernameAsync(args[0], cancellationToken) is not { } target)
        {
            session.Send(new ModMessageEvent(_texts.Format(MessageKeys.UserNotFound, ("username", args[0]))).Compose());
            return;
        }

        if (!_sessions.TryGetByUserId(target.Id, out ClientSession? online))
        {
            session.Send(new ModMessageEvent(_texts.Format(MessageKeys.UserNotOnline, ("username", target.Username))).Compose());
            return;
        }

        online!.EnqueueSend(new ModMessageEvent(string.Join(' ', args[1..])).Compose());
        session.Send(new ModMessageEvent(_texts.Format(MessageKeys.AlertSent, ("username", target.Username))).Compose());
    }
}
