using DragonEmu.Domain;

namespace DragonEmu.Persistence;

public interface ITraxRepository
{
    Task<IReadOnlyList<TraxSong>> GetPlaylistAsync(int machineItemId, CancellationToken cancellationToken = default);
}
