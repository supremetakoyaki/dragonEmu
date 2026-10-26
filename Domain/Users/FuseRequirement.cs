using System.Globalization;

namespace DragonEmu.Domain;

public interface IFuseFacts
{
    int Rank { get; }

    ValueTask<bool> HasSubscriptionAsync(string subscription);

    ValueTask<int> AchievementLevelAsync(string codeBase);

    bool IsRoomOwner { get; }
}

public abstract record FuseRequirement
{
    private static readonly HashSet<string> Subscriptions = ["habbo_club"];

    public abstract ValueTask<bool> IsMetAsync(IFuseFacts facts);

    public virtual IEnumerable<string> Achievements => [];

    public static FuseRequirement Parse(string text) => new Parser(text).ParseWhole();

    private sealed record Everyone : FuseRequirement
    {
        public override ValueTask<bool> IsMetAsync(IFuseFacts facts) => ValueTask.FromResult(true);
    }

    private sealed record RankIs(string Comparison, int Value) : FuseRequirement
    {
        public override ValueTask<bool> IsMetAsync(IFuseFacts facts) => ValueTask.FromResult(Comparison switch
        {
            ">=" => facts.Rank >= Value,
            ">" => facts.Rank > Value,
            "<=" => facts.Rank <= Value,
            "<" => facts.Rank < Value,
            "==" => facts.Rank == Value,
            _ => facts.Rank != Value,
        });
    }

    private sealed record HasSubscription(string Subscription) : FuseRequirement
    {
        public override ValueTask<bool> IsMetAsync(IFuseFacts facts) => facts.HasSubscriptionAsync(Subscription);
    }

    private sealed record HasAchievement(string CodeBase, int Level) : FuseRequirement
    {
        public override async ValueTask<bool> IsMetAsync(IFuseFacts facts) => await facts.AchievementLevelAsync(CodeBase) >= Level;

        public override IEnumerable<string> Achievements => [CodeBase];
    }

    private sealed record UserIsRoomOwner : FuseRequirement
    {
        public override ValueTask<bool> IsMetAsync(IFuseFacts facts) => ValueTask.FromResult(facts.IsRoomOwner);
    }

    private sealed record Not(FuseRequirement Inner) : FuseRequirement
    {
        public override async ValueTask<bool> IsMetAsync(IFuseFacts facts) => !await Inner.IsMetAsync(facts);

        public override IEnumerable<string> Achievements => Inner.Achievements;
    }

    private sealed record Both(FuseRequirement Left, FuseRequirement Right) : FuseRequirement
    {
        public override async ValueTask<bool> IsMetAsync(IFuseFacts facts) => await Left.IsMetAsync(facts) && await Right.IsMetAsync(facts);

        public override IEnumerable<string> Achievements => Left.Achievements.Concat(Right.Achievements);
    }

    private sealed record Either(FuseRequirement Left, FuseRequirement Right) : FuseRequirement
    {
        public override async ValueTask<bool> IsMetAsync(IFuseFacts facts) => await Left.IsMetAsync(facts) || await Right.IsMetAsync(facts);

        public override IEnumerable<string> Achievements => Left.Achievements.Concat(Right.Achievements);
    }

    private sealed class Parser(string text)
    {
        private int _position;

        public FuseRequirement ParseWhole()
        {
            FuseRequirement requirement = ParseEither();
            SkipSpace();
            if (_position < text.Length)
            {
                throw Error("unexpected text");
            }

            return requirement;
        }

        private FuseRequirement ParseEither()
        {
            FuseRequirement requirement = ParseBoth();
            while (TryOperator("OR"))
            {
                requirement = new Either(requirement, ParseBoth());
            }

            return requirement;
        }

        private FuseRequirement ParseBoth()
        {
            FuseRequirement requirement = ParseNot();
            while (TryOperator("AND"))
            {
                requirement = new Both(requirement, ParseNot());
            }

            return requirement;
        }

        private FuseRequirement ParseNot() => TrySymbol("!") ? new Not(ParseNot()) : ParseAtom();

        private FuseRequirement ParseAtom()
        {
            if (TrySymbol("("))
            {
                FuseRequirement inner = ParseEither();
                Expect(")");
                return inner;
            }

            string word = ReadWord();
            switch (word)
            {
                case "everyone":
                    return new Everyone();

                case "user_is_room_owner":
                    return new UserIsRoomOwner();

                case "rank":
                    string comparison = ReadComparison();
                    return new RankIs(comparison, ReadNumber());

                case "has_subscription":
                    Expect("(");
                    string subscription = ReadQuoted();
                    Expect(")");
                    return Subscriptions.Contains(subscription)
                        ? new HasSubscription(subscription)
                        : throw Error($"unknown subscription \"{subscription}\"");

                case "has_achievement":
                    Expect("(");
                    string codeBase = ReadQuoted();
                    Expect(",");
                    int level = ReadNumber();
                    Expect(")");
                    return new HasAchievement(codeBase, level);

                default:
                    throw Error(word.Length == 0 ? "expected a condition" : $"unknown condition \"{word}\"");
            }
        }

        private bool TryOperator(string name)
        {
            int start = _position;
            if (TrySymbol(".") && ReadWord() == name && TrySymbol("."))
            {
                return true;
            }

            _position = start;
            return false;
        }

        private bool TrySymbol(string symbol)
        {
            SkipSpace();
            if (string.CompareOrdinal(text, _position, symbol, 0, symbol.Length) != 0)
            {
                return false;
            }

            _position += symbol.Length;
            return true;
        }

        private void Expect(string symbol)
        {
            if (!TrySymbol(symbol))
            {
                throw Error($"expected \"{symbol}\"");
            }
        }

        private string ReadWord()
        {
            SkipSpace();
            int start = _position;
            while (_position < text.Length && (char.IsLetter(text[_position]) || text[_position] == '_'))
            {
                _position++;
            }

            return text[start.._position];
        }

        private string ReadComparison()
        {
            foreach (string comparison in (string[])[">=", "<=", "==", "!=", ">", "<"])
            {
                if (TrySymbol(comparison))
                {
                    return comparison;
                }
            }

            throw Error("expected a comparison");
        }

        private int ReadNumber()
        {
            SkipSpace();
            int start = _position;
            while (_position < text.Length && char.IsAsciiDigit(text[_position]))
            {
                _position++;
            }

            return int.TryParse(text.AsSpan(start, _position - start), NumberStyles.None, CultureInfo.InvariantCulture, out int number)
                ? number
                : throw Error("expected a number");
        }

        private string ReadQuoted()
        {
            Expect("\"");
            int end = text.IndexOf('"', _position);
            if (end < 0)
            {
                throw Error("unclosed quote");
            }

            string value = text[_position..end];
            _position = end + 1;
            return value;
        }

        private void SkipSpace()
        {
            while (_position < text.Length && char.IsWhiteSpace(text[_position]))
            {
                _position++;
            }
        }

        private FormatException Error(string problem) => new($"{problem} at position {_position} of \"{text}\"");
    }
}
