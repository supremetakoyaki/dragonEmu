using DragonEmu.Game.Moderation;
using DragonEmu.Protocol.Messages;
using DragonEmu.Protocol.Messages.Outgoing.Moderation;
using DragonEmu.Game.Settings;

namespace DragonEmu.Game.Chat.Commands;

public sealed class BanChatCommand : IChatCommand
{
    private readonly ModerationService _moderation;
    private readonly MessageTexts _texts;

    public BanChatCommand(ModerationService moderation, MessageTexts texts)
    {
        _texts = texts;
        _moderation = moderation;
    }

    public string Name => "ban";

    public string RequiredFuse => "fuse_ban";

    public async ValueTask ExecuteAsync(IGameSession session, string[] args, CancellationToken cancellationToken)
    {
        if (session.AuthenticatedUser is not { } account)
        {
            return;
        }

        if (args.Length == 0 || !ModerationCommandArgs.TryDurationAndText(args, 1, out int? hours, out string? reason))
        {
            session.Send(new ModMessageEvent(_texts.Get(MessageKeys.UsageBan)).Compose());
            return;
        }

        session.Send(new ModMessageEvent(await _moderation.BanAsync(account, args[0], hours, reason, superban: false, cancellationToken)).Compose());
    }
}
