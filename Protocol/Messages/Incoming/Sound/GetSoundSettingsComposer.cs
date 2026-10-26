using DragonEmu.Persistence;
using DragonEmu.Protocol.Messages.Outgoing.Sound;

namespace DragonEmu.Protocol.Messages.Incoming.Sound;

public sealed class GetSoundSettingsComposer : IMessageComposer
{
    private readonly IUserSettingsRepository _userSettingsRepository;

    public GetSoundSettingsComposer(IUserSettingsRepository userSettingsRepository)
    {
        _userSettingsRepository = userSettingsRepository;
    }

    public void Parse(ClientPacket packet)
    {
    }

    public async ValueTask HandleAsync(IGameSession session, CancellationToken cancellationToken)
    {
        if (session.AuthenticatedUser is not { } account)
        {
            return;
        }

        (int soundVolume, _) = await _userSettingsRepository.GetOrCreateAsync(account.Id, cancellationToken);
        session.Send(new SoundSettingsEvent(soundVolume).Compose());
    }
}
