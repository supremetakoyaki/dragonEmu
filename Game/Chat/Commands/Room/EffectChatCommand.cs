using DragonEmu.Game.Avatars;
using DragonEmu.Game.Rooms;
using DragonEmu.Protocol.Messages;
using DragonEmu.Protocol.Messages.Outgoing.Moderation;
using DragonEmu.Game.Settings;

namespace DragonEmu.Game.Chat.Commands;

public sealed class EffectChatCommand : IChatCommand
{
    private readonly AvatarEffectService _avatarEffectService;
    private readonly MessageTexts _texts;

    public EffectChatCommand(AvatarEffectService avatarEffectService, MessageTexts texts)
    {
        _texts = texts;
        _avatarEffectService = avatarEffectService;
    }

    public string Name => "fx";

    public string RequiredFuse => RoomRightsService.AdministratorAccessFuse;

    public ValueTask ExecuteAsync(IGameSession session, string[] args, CancellationToken cancellationToken)
    {
        if (args.Length != 1 || !int.TryParse(args[0], out int effectId) || effectId < 0)
        {
            session.Send(new ModMessageEvent(_texts.Get(MessageKeys.UsageFx)).Compose());
        }
        else if (!(effectId == AvatarEffectService.NoEffect ? _avatarEffectService.TakeOff(session) : _avatarEffectService.ShowInRoom(session, effectId)))
        {
            session.Send(new ModMessageEvent(_texts.Get(MessageKeys.NotInRoom)).Compose());
        }

        return ValueTask.CompletedTask;
    }
}
