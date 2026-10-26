using DragonEmu.Game.Moderation;
using DragonEmu.Protocol.Messages;
using DragonEmu.Protocol.Messages.Outgoing.Moderation;
using DragonEmu.Game.Settings;

namespace DragonEmu.Game.Chat.Commands;

public sealed class MuteChatCommand : IChatCommand
{
    private readonly ModerationService _moderation;
    private readonly MessageTexts _texts;

    public MuteChatCommand(ModerationService moderation, MessageTexts texts)
    {
        _texts = texts;
        _moderation = moderation;
    }

    public string Name => "mute";

    public string RequiredFuse => "fuse_mute";

    public async ValueTask ExecuteAsync(IGameSession session, string[] args, CancellationToken cancellationToken)
    {
        if (session.AuthenticatedUser is not { } account)
        {
            return;
        }

        if (args.Length is 0 or > 2 || !ModerationCommandArgs.TryDurationAndText(args, 1, out int? hours, out string? extra) || extra is not null)
        {
            session.Send(new ModMessageEvent(_texts.Get(MessageKeys.UsageMute)).Compose());
            return;
        }

        session.Send(new ModMessageEvent(await _moderation.MuteAsync(account, args[0], hours, cancellationToken)).Compose());
    }
}
