using DragonEmu.Persistence;
using DragonEmu.Protocol.Messages.Outgoing.Navigator;
using DragonEmu.Domain;

namespace DragonEmu.Protocol.Messages.Incoming.Navigator;

public sealed class UpdateNavigatorSettingsMessageComposer : IMessageComposer
{
    private readonly IUserSettingsRepository _userSettingsRepository;
    private readonly IPrivateRoomRepository _privateRoomRepository;

    private int _homeRoomId;

    public UpdateNavigatorSettingsMessageComposer(IUserSettingsRepository userSettingsRepository, IPrivateRoomRepository privateRoomRepository)
    {
        _userSettingsRepository = userSettingsRepository;
        _privateRoomRepository = privateRoomRepository;
    }

    public void Parse(ClientPacket packet)
    {
        _homeRoomId = packet.PopInt32();
    }

    public async ValueTask HandleAsync(IGameSession session, CancellationToken cancellationToken)
    {
        if (session.AuthenticatedUser is not { } account)
        {
            return;
        }

        if (_homeRoomId != 0 && await _privateRoomRepository.GetByIdAsync(_homeRoomId, cancellationToken) is not { OwnerId: not null })
        {
            return;
        }

        if (!session.TryUseCooldown(ActionCooldowns.HomeRoom, ActionCooldowns.Default))
        {
            return;
        }

        await _userSettingsRepository.SetHomeRoomAsync(account.Id, _homeRoomId, cancellationToken);
        session.Send(new NavigatorSettingsEvent(_homeRoomId).Compose());
    }
}
