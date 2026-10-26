using DragonEmu.Domain;

namespace DragonEmu.Persistence;

public interface IClubGiftRepository
{
    Task<IReadOnlyList<ClubGiftDefinition>> GetAllAsync(CancellationToken cancellationToken = default);
}
