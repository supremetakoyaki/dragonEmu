namespace DragonEmu.Persistence;

public enum FavouriteAddResult
{
    Added,

    ListFull,

    Refused,
}

public interface IFavouriteRoomRepository
{
    Task<IReadOnlyList<int>> GetRoomIdsAsync(int userId, CancellationToken cancellationToken = default);

    Task<FavouriteAddResult> TryAddAsync(int userId, int roomId, int limit, CancellationToken cancellationToken = default);

    Task<bool> RemoveAsync(int userId, int roomId, CancellationToken cancellationToken = default);
}
