using DragonEmu.Persistence;

namespace DragonEmu.Protocol.Messages.Incoming.Sound;

public sealed class SetSoundSettingsComposer : IMessageComposer
{
    private const int MaxVolume = 100;

    private readonly IUserSettingsRepository _userSettingsRepository;

    private int _volume;

    public SetSoundSettingsComposer(IUserSettingsRepository userSettingsRepository)
    {
        _userSettingsRepository = userSettingsRepository;
    }

    public void Parse(ClientPacket packet)
    {
        _volume = Math.Clamp(packet.PopInt32(), 0, MaxVolume);
    }

    public async ValueTask HandleAsync(IGameSession session, CancellationToken cancellationToken)
    {
        if (session.AuthenticatedUser is not { } account)
        {
            return;
        }

        await _userSettingsRepository.SetSoundVolumeAsync(account.Id, _volume, cancellationToken);
    }
}
