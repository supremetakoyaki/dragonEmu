using DragonEmu.Domain;

namespace DragonEmu.Persistence;

public interface IUserRepository
{
    Task<(UserAccount Account, int LoginStreakDays)?> ValidateAndConsumeSsoTokenAsync(
        string token, string? ipAddress, DateTime todayStartUtc, DateTime yesterdayStartUtc, CancellationToken cancellationToken = default);

    Task<bool> IsUsernameOrEmailTakenAsync(string username, string email, CancellationToken cancellationToken = default);

    Task<UserAccount> CreateUserAsync(string username, string email, string passwordHash, string motto, char gender, string figure, CancellationToken cancellationToken = default);

    Task<(UserAccount Account, string PasswordHash)?> FindForLoginAsync(string username, CancellationToken cancellationToken = default);

    Task<UserAccount?> GetByIdAsync(int id, CancellationToken cancellationToken = default);

    Task<UserAccount?> GetByUsernameAsync(string username, CancellationToken cancellationToken = default);

    Task<string> IssueSsoTokenAsync(int userId, TimeSpan ttl, CancellationToken cancellationToken = default);

    Task UpdateFigureAsync(int userId, string figure, char gender, CancellationToken cancellationToken = default);

    Task UpdateMottoAsync(int userId, string motto, CancellationToken cancellationToken = default);

    Task<string> GetPoolFigureAsync(int userId, CancellationToken cancellationToken = default);

    Task UpdatePoolFigureAsync(int userId, string poolFigure, CancellationToken cancellationToken = default);

    Task<(DateTime? ClubExpiresAt, int ClubMonthsAccumulated, int ClubGiftsClaimed)> GetClubStatusAsync(int userId, CancellationToken cancellationToken = default);

    Task UpdateClubStatusAsync(int userId, DateTime clubExpiresAt, int clubMonthsAccumulated, CancellationToken cancellationToken = default);

    Task<bool> TryClaimClubGiftAsync(int userId, CancellationToken cancellationToken = default);

    Task UpdateRankAsync(int userId, int rank, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<int>> GetUserIdsWhoseClubEndedAsync(DateTime after, DateTime upTo, CancellationToken cancellationToken = default);

    Task<IReadOnlyDictionary<int, int>> AddOnlineSecondsAsync(IReadOnlyDictionary<int, int> secondsByUser, CancellationToken cancellationToken = default);

    Task<int> GetOnlineSecondsAsync(int userId, CancellationToken cancellationToken = default);

    Task SetEmailVerifiedAsync(int userId, bool verified, CancellationToken cancellationToken = default);

    Task<UsernameChangeStatus> GetUsernameChangeStatusAsync(int userId, CancellationToken cancellationToken = default);

    Task<bool> TryChangeUsernameAsync(int userId, string oldUsername, string newUsername, CancellationToken cancellationToken = default);
}
