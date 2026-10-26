namespace DragonEmu.Domain;

public enum PollType
{
    Survey,
    Quiz,
}

public enum PollRewardType
{
    None,
    Furni,
    Badge,
}

public enum PollQuestionType
{
    Single = 1,
    Multiple = 2,
    Text = 3,
}

public sealed record Poll(int Id, int RoomId, string Name, string ThanksMessage, PollType Type, PollRewardType RewardType, string Reward);

public sealed record PollOption(int Number, string Content, bool IsCorrect);

public sealed record PollQuestion(
    int Id, int Number, PollQuestionType Type, string Content, int SelectionMin, int SelectionMax, IReadOnlyList<PollOption> Options);

public sealed record PollAnswer(int QuestionId, string Answer, bool IsCorrect);
