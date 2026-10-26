using System.Globalization;
using DragonEmu.Game.Rooms;
using DragonEmu.Protocol.Messages;
using DragonEmu.Protocol.Messages.Outgoing.Moderation;
using DragonEmu.Game.Settings;

namespace DragonEmu.Game.Chat.Commands;

public sealed class TeleportChatCommand : IChatCommand
{
    private readonly MessageTexts _texts;

    public TeleportChatCommand(MessageTexts texts)
    {
        _texts = texts;
    }

    public string Name => "teleport";

    public string RequiredFuse => RoomRightsService.AdministratorAccessFuse;

    public ValueTask ExecuteAsync(IGameSession session, string[] args, CancellationToken cancellationToken)
    {
        session.Send(new ModMessageEvent(Apply(session, args)).Compose());
        return ValueTask.CompletedTask;
    }

    private string Apply(IGameSession session, string[] args)
    {
        if (args.Length == 0)
        {
            return session.TeleportMode
                ? _texts.Format(MessageKeys.TeleportStatusOn, ("height", HeightText(session.TeleportHeight)))
                : _texts.Get(MessageKeys.TeleportStatusOff);
        }

        if (args[0].Equals("off", StringComparison.OrdinalIgnoreCase) && args.Length == 1)
        {
            session.TeleportMode = false;
            session.TeleportHeight = null;
            return _texts.Get(MessageKeys.TeleportOff);
        }

        if (!args[0].Equals("on", StringComparison.OrdinalIgnoreCase) || args.Length > 2)
        {
            return _texts.Get(MessageKeys.UsageTeleport);
        }

        double? height = null;
        if (args.Length == 2)
        {
            if (!double.TryParse(args[1], NumberStyles.Float, CultureInfo.InvariantCulture, out double z) || z < 0 || double.IsInfinity(z))
            {
                return _texts.Get(MessageKeys.TeleportBadHeight);
            }

            height = z;
        }

        session.TeleportMode = true;
        session.TeleportHeight = height;
        return _texts.Format(MessageKeys.TeleportOn, ("height", HeightText(height)));
    }

    private string HeightText(double? height) =>
        height is { } z ? _texts.Format(MessageKeys.TeleportHeightAt, ("height", z.ToString("0.####", CultureInfo.InvariantCulture))) : _texts.Get(MessageKeys.TeleportHeightAnywhere);
}
