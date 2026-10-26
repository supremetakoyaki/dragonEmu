namespace DragonEmu.Domain;

public sealed record GroupSettings(int Limit, int LimitClub)
{
    public static readonly GroupSettings Default = new(50, 100);
}

public sealed record UsernameSettings(bool ChangeAllowed, int CooldownDays, int ChangeLimit, int MinLength, int MaxLength, string AllowedChars)
{
    public const string DefaultAllowedChars = "^[a-zA-Z0-9.:;,_@!?-]+$";

    public static readonly UsernameSettings Default = new(true, 14, 5, 4, 15, DefaultAllowedChars);
}

public sealed record UsernameChangeStatus(DateTime? ChangedAt, int Changes, bool CanChange);
