using DragonEmu.Domain;

namespace DragonEmu.Persistence;

public sealed record RecycleRequest(
    int UserId, IReadOnlyCollection<int> ItemIds, int PrizeBoxFurniDefId, string PrizeBoxExtraData, GiftContent Prize,
    int CooldownSeconds, DateTime UtcNow);

public interface IRecyclerRepository
{
    Task<DateTime?> GetLastRecycledAtAsync(int userId, CancellationToken cancellationToken = default);

    Task<int?> TryRecycleAsync(RecycleRequest request, CancellationToken cancellationToken = default);
}
