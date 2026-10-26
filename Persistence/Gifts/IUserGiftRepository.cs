using DragonEmu.Domain;

namespace DragonEmu.Persistence;

public interface IUserGiftRepository
{
    Task CreateAsync(int itemId, int senderUserId, IReadOnlyList<GiftContent> contents, CancellationToken cancellationToken = default);

    Task<UserGift?> GetByItemIdAsync(int itemId, CancellationToken cancellationToken = default);

    Task DeleteAsync(int itemId, CancellationToken cancellationToken = default);
}
