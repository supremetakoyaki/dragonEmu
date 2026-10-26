using DragonEmu.Domain;

namespace DragonEmu.Persistence;

public interface IInterstitialAdRepository
{
    Task<IReadOnlyList<InterstitialAd>> GetAllAsync(CancellationToken cancellationToken = default);
}
