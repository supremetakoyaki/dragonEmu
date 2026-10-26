using System.Globalization;
using DragonEmu.Game.Hotel;
using DragonEmu.Protocol.Messages;
using DragonEmu.Protocol.Messages.Outgoing.Moderation;
using DragonEmu.Game.Settings;

namespace DragonEmu.Game.Chat.Commands;

public sealed class ShutdownChatCommand : IChatCommand
{
    private readonly HotelShutdownService _shutdown;
    private readonly MessageTexts _texts;

    public ShutdownChatCommand(HotelShutdownService shutdown, MessageTexts texts)
    {
        _texts = texts;
        _shutdown = shutdown;
    }

    public string Name => "shutdown";

    public string RequiredFuse => "fuse_administrator_access";

    public ValueTask ExecuteAsync(IGameSession session, string[] args, CancellationToken cancellationToken)
    {
        if (session.AuthenticatedUser is not { } account)
        {
            return ValueTask.CompletedTask;
        }

        if (args.Length == 0 || !string.Equals(args[0], "confirm", StringComparison.OrdinalIgnoreCase))
        {
            Tell(session, _texts.Format(MessageKeys.ShutdownConfirmPrompt, ("minutes", HotelShutdownService.MinutesLeft)));
            return ValueTask.CompletedTask;
        }

        TimeOnly? reopens = null;
        if (args.Length > 1)
        {
            if (!TimeOnly.TryParseExact(args[1], "H:mm", CultureInfo.InvariantCulture, DateTimeStyles.None, out TimeOnly parsed))
            {
                Tell(session, _texts.Get(MessageKeys.ShutdownBadTime));
                return ValueTask.CompletedTask;
            }

            reopens = parsed;
        }

        if (!_shutdown.TryBegin(account.Username, reopens))
        {
            Tell(session, _texts.Get(MessageKeys.ShutdownAlready));
        }

        return ValueTask.CompletedTask;
    }

    private static void Tell(IGameSession session, string text) => session.Send(new ModMessageEvent(text).Compose());
}
