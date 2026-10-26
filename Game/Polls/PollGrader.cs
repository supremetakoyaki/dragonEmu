using System.Globalization;
using DragonEmu.Domain;

namespace DragonEmu.Game.Polls;

public static class PollGrader
{
    public const int MaxTextLength = 500;

    public static PollAnswer? Grade(PollQuestion question, IReadOnlyList<string> answers)
    {
        return question.Type == PollQuestionType.Text ? GradeText(question, answers) : GradeChoice(question, answers);
    }

    private static PollAnswer? GradeChoice(PollQuestion question, IReadOnlyList<string> answers)
    {
        var chosen = new SortedSet<int>();
        foreach (string answer in answers)
        {
            if (!int.TryParse(answer, NumberStyles.None, CultureInfo.InvariantCulture, out int number)
                || question.Options.All(o => o.Number != number) || !chosen.Add(number))
            {
                return null;
            }
        }

        int max = question.Type == PollQuestionType.Single ? 1 : question.SelectionMax;
        if (chosen.Count < question.SelectionMin || chosen.Count > max || chosen.Count == 0)
        {
            return null;
        }

        var right = question.Options.Where(o => o.IsCorrect).Select(o => o.Number).ToHashSet();
        return new PollAnswer(question.Id, string.Join(',', chosen), right.Count == 0 || right.SetEquals(chosen));
    }

    private static PollAnswer? GradeText(PollQuestion question, IReadOnlyList<string> answers)
    {
        if (answers.Count != 1)
        {
            return null;
        }

        string text = answers[0].Trim();
        if (text.Length == 0 || text.Length > MaxTextLength)
        {
            return null;
        }

        var accepted = question.Options.Where(o => o.IsCorrect).ToList();
        return new PollAnswer(question.Id, text, accepted.Count == 0 || accepted.Any(o => string.Equals(o.Content, text, StringComparison.OrdinalIgnoreCase)));
    }
}
