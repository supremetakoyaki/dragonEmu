using DragonEmu.Domain;

namespace DragonEmu.Persistence;

public interface IMessengerRepository
{
    Task<IReadOnlyList<MessengerProfile>> GetFriendsAsync(int userId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<int>> GetFriendIdsAsync(int userId, CancellationToken cancellationToken = default);

    Task<MessengerProfile?> GetProfileAsync(int userId, CancellationToken cancellationToken = default);

    Task<bool> AreFriendsAsync(int userId, int otherUserId, CancellationToken cancellationToken = default);

    Task<int> CountFriendsAsync(int userId, CancellationToken cancellationToken = default);

    Task<bool> AcceptsFriendRequestsAsync(int userId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<FriendRequest>> GetRequestsAsync(int toUserId, CancellationToken cancellationToken = default);

    Task<bool> HasRequestAsync(int fromUserId, int toUserId, CancellationToken cancellationToken = default);

    Task<bool> AddRequestAsync(int fromUserId, int toUserId, CancellationToken cancellationToken = default);

    Task<bool> AcceptRequestAsync(int fromUserId, int toUserId, CancellationToken cancellationToken = default);

    Task DeclineRequestsAsync(int toUserId, IReadOnlyCollection<int> fromUserIds, CancellationToken cancellationToken = default);

    Task DeclineAllRequestsAsync(int toUserId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<int>> RemoveFriendsAsync(int userId, IReadOnlyCollection<int> friendIds, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<FriendSearchHit>> SearchAsync(int userId, string namePrefix, int limit, CancellationToken cancellationToken = default);
}
