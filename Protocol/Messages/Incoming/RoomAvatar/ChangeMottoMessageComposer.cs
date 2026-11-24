using DragonEmu.Domain;
using DragonEmu.Game.Avatars;

namespace DragonEmu.Protocol.Messages.Incoming.RoomAvatar;

public sealed class ChangeMottoMessageComposer : IMessageComposer
{
    private readonly MottoService _mottoService;

    private string _motto = string.Empty;

    public ChangeMottoMessageComposer(MottoService mottoService)
    {
        _mottoService = mottoService;
    }

    public void Parse(ClientPacket packet)
    {
        _motto = packet.PopString();
    }

    public async ValueTask HandleAsync(IGameSession session, CancellationToken cancellationToken)
    {
        if (session.AuthenticatedUser is not { } account || !session.TryUseCooldown(ActionCooldowns.Motto, ActionCooldowns.Default))
        {
            return;
        }

        await _mottoService.ChangeAsync(session, account, _motto, cancellationToken);
    }
}
