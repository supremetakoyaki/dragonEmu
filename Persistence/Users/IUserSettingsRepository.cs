namespace DragonEmu.Persistence;

public interface IUserSettingsRepository
{
    Task<(int SoundVolume, int HomeRoom)> GetOrCreateAsync(int userId, CancellationToken cancellationToken = default);

    Task SetSoundVolumeAsync(int userId, int soundVolume, CancellationToken cancellationToken = default);

    Task SetHomeRoomAsync(int userId, int homeRoom, CancellationToken cancellationToken = default);
}
