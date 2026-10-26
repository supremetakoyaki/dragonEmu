namespace DragonEmu.Persistence;

public interface IRoomVoteRepository
{
    Task<bool> HasVotedAsync(int userId, int roomId, CancellationToken cancellationToken = default);

    Task<int> GetScoreAsync(int roomId, CancellationToken cancellationToken = default);

    Task<int?> VoteAsync(int userId, int roomId, int vote, CancellationToken cancellationToken = default);
}
