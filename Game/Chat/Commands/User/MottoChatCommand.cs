using DragonEmu.Game.Avatars;
using DragonEmu.Protocol.Messages;
using DragonEmu.Protocol.Messages.Outgoing.Moderation;
using DragonEmu.Domain;
using DragonEmu.Game.Settings;

namespace DragonEmu.Game.Chat.Commands;

public sealed class MottoChatCommand : IChatCommand
{
    private readonly MottoService _mottoService;
    private readonly MessageTexts _texts;

    public MottoChatCommand(MottoService mottoService, MessageTexts texts)
    {
        _texts = texts;
        _mottoService = mottoService;
    }

    public string Name => "motto";

    public string? RequiredFuse => null;

    public async ValueTask ExecuteAsync(IGameSession session, string[] args, CancellationToken cancellationToken)
    {
        if (session.AuthenticatedUser is not { } account)
        {
            return;
        }

        if (args.Length == 0)
        {
            session.Send(new ModMessageEvent(_texts.Get(MessageKeys.UsageMotto)).Compose());
            return;
        }

        if (!session.TryUseCooldown(ActionCooldowns.Motto, ActionCooldowns.Default))
        {
            session.Send(new ModMessageEvent(_texts.Get(MessageKeys.MottoTooFast)).Compose());
            return;
        }

        await _mottoService.ChangeAsync(session, account, string.Join(' ', args), cancellationToken);
    }
}
