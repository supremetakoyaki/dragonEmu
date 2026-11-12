using DragonEmu.Domain;

namespace DragonEmu.Persistence;

public interface IClubOfferRepository
{
    Task<IReadOnlyList<ClubOfferDefinition>> GetAllAsync(CancellationToken cancellationToken = default);
}
