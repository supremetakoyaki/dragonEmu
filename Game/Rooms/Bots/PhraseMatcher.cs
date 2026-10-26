using System.Text;

namespace DragonEmu.Game.Rooms;

public enum PhraseMatch
{
    Exact,
    Contains,
    StartsWith,
}

public sealed class PhraseMatcher
{
    private readonly string[] _phrases;
    private readonly PhraseMatch _match;
    private readonly bool _caseSensitive;

    public PhraseMatcher(IEnumerable<string> phrases, PhraseMatch match, bool caseSensitive)
    {
        _match = match;
        _caseSensitive = caseSensitive;
        _phrases = phrases.Select(phrase => Normalize(phrase, caseSensitive)).Where(phrase => phrase.Length > 0).Distinct().ToArray();
    }

    public bool IsEmpty => _phrases.Length == 0;

    public bool Matches(string message)
    {
        string said = Normalize(message, _caseSensitive);
        if (said.Length == 0)
        {
            return false;
        }

        foreach (string phrase in _phrases)
        {
            bool matched = _match switch
            {
                PhraseMatch.Exact => said == phrase,
                PhraseMatch.StartsWith => said == phrase || said.StartsWith(phrase + " ", StringComparison.Ordinal),
                _ => $" {said} ".Contains($" {phrase} ", StringComparison.Ordinal),
            };

            if (matched)
            {
                return true;
            }
        }

        return false;
    }

    private static string Normalize(string text, bool caseSensitive)
    {
        var normalized = new StringBuilder(text.Length);
        bool pendingSpace = false;
        foreach (char c in text)
        {
            if (char.IsLetterOrDigit(c))
            {
                if (pendingSpace && normalized.Length > 0)
                {
                    normalized.Append(' ');
                }

                normalized.Append(caseSensitive ? c : char.ToLowerInvariant(c));
                pendingSpace = false;
            }
            else
            {
                pendingSpace = true;
            }
        }

        return normalized.ToString();
    }
}
