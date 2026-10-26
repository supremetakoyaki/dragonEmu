using DragonEmu.Game.Rooms;
using DragonEmu.Protocol.Messages;
using DragonEmu.Protocol.Messages.Outgoing.Moderation;
using DragonEmu.Game.Settings;

namespace DragonEmu.Game.Chat.Commands;

public sealed class SoftKickChatCommand : IChatCommand
{
    private readonly RoomKickService _kicks;
    private readonly MessageTexts _texts;

    public SoftKickChatCommand(RoomKickService kicks, MessageTexts texts)
    {
        _texts = texts;
        _kicks = kicks;
    }

    public string Name => "softkick";

    public string? RequiredFuse => null;

    public async ValueTask ExecuteAsync(IGameSession session, string[] args, CancellationToken cancellationToken)
    {
        if (session.AuthenticatedUser is not { } account)
        {
            return;
        }

        if (args.Length != 1)
        {
            session.Send(new ModMessageEvent(_texts.Get(MessageKeys.UsageSoftkick)).Compose());
            return;
        }

        string? refusal = await _kicks.SoftKickAsync(session, account, args[0], cancellationToken) switch
        {
            KickResult.NotInARoom => _texts.Get(MessageKeys.NotInRoom),
            KickResult.NotAllowed => _texts.Get(MessageKeys.SoftkickNotAllowed),
            KickResult.NotHere => _texts.Format(MessageKeys.KickNotHere, ("username", args[0])),
            KickResult.CantBeKicked => _texts.Format(MessageKeys.KickCantBeKicked, ("username", args[0])),
            _ => null,
        };

        if (refusal is not null)
        {
            session.Send(new ModMessageEvent(refusal).Compose());
        }
    }
}
