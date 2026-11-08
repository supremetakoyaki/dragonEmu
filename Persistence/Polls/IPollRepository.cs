using DragonEmu.Domain;

namespace DragonEmu.Persistence;

public interface IPollRepository
{
    Task<Poll?> GetUnansweredInRoomAsync(int roomId, int userId, Language language, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<PollQuestion>> GetQuestionsAsync(int pollId, Language language, CancellationToken cancellationToken = default);

    Task<bool> CompleteAsync(
        int userId, int pollId, IReadOnlyList<PollAnswer> answers, int questionsTotal, bool rewardGiven, CancellationToken cancellationToken = default);
}
