using DragonEmu.Domain;

namespace DragonEmu.Game.Polls;

public sealed class PollProgress
{
    private readonly List<PollAnswer> _answers = [];

    public PollProgress(Poll poll, IReadOnlyList<PollQuestion> questions)
    {
        Poll = poll;
        Questions = questions;
    }

    public Poll Poll { get; }

    public IReadOnlyList<PollQuestion> Questions { get; }

    public IReadOnlyList<PollAnswer> Answers => _answers;

    public PollQuestion? Current => _answers.Count < Questions.Count ? Questions[_answers.Count] : null;

    public void Add(PollAnswer answer) => _answers.Add(answer);
}
