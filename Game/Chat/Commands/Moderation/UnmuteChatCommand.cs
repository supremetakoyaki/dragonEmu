using DragonEmu.Game.Moderation;
using DragonEmu.Protocol.Messages;
using DragonEmu.Protocol.Messages.Outgoing.Moderation;
using DragonEmu.Game.Settings;

namespace DragonEmu.Game.Chat.Commands;

public sealed class UnmuteChatCommand : IChatCommand
{
    private readonly ModerationService _moderation;
    private readonly MessageTexts _texts;

    public UnmuteChatCommand(ModerationService moderation, MessageTexts texts)
    {
        _texts = texts;
        _moderation = moderation;
    }

    public string Name => "unmute";

    public string RequiredFuse => "fuse_mute";

    public async ValueTask ExecuteAsync(IGameSession session, string[] args, CancellationToken cancellationToken)
    {
        if (args.Length != 1)
        {
            session.Send(new ModMessageEvent(_texts.Get(MessageKeys.UsageUnmute)).Compose());
            return;
        }

        session.Send(new ModMessageEvent(await _moderation.UnmuteAsync(args[0], cancellationToken)).Compose());
    }
}
