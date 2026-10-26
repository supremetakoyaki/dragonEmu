using DragonEmu.Domain;

namespace DragonEmu.Persistence;

public interface IWardrobeRepository
{
    Task<IReadOnlyList<WardrobeOutfit>> GetAsync(int userId, CancellationToken cancellationToken = default);

    Task SaveAsync(int userId, WardrobeOutfit outfit, CancellationToken cancellationToken = default);
}
