using DragonEmu.Domain;

namespace DragonEmu.Persistence;

public interface IUserSettingsRepository
{
    Task<UserSettings> GetOrCreateAsync(int userId, CancellationToken cancellationToken = default);

    Task SetSoundVolumeAsync(int userId, int soundVolume, CancellationToken cancellationToken = default);

    Task SetHomeRoomAsync(int userId, int homeRoom, CancellationToken cancellationToken = default);

    Task<Language> GetLanguageAsync(int userId, CancellationToken cancellationToken = default);
}
