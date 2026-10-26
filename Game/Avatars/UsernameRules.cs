using System.Text.RegularExpressions;
using DragonEmu.Domain;
using DragonEmu.Game.Chat;
using DragonEmu.Game.Settings;
using Microsoft.Extensions.Logging;

namespace DragonEmu.Game.Avatars;

public enum UsernameCheck
{
    Ok = 0,
    TooShort = 2,
    TooLong = 3,
    Invalid = 4,
    Taken = 5,
    ChangeNotAllowed = 6,
}

public sealed class UsernameRules
{
    private static readonly TimeSpan MatchTimeout = TimeSpan.FromMilliseconds(100);

    private readonly GameSettingsCache _gameSettings;
    private readonly ProfanityFilterCache _profanityFilter;
    private readonly ILogger<UsernameRules> _logger;

    private (string Pattern, Regex Regex)? _allowed;

    public UsernameRules(GameSettingsCache gameSettings, ProfanityFilterCache profanityFilter, ILogger<UsernameRules> logger)
    {
        _gameSettings = gameSettings;
        _profanityFilter = profanityFilter;
        _logger = logger;
    }

    public static string Normalize(string name) => name.Trim();

    public UsernameCheck Check(string name)
    {
        UsernameSettings settings = _gameSettings.Current.Username;
        if (name.Length < settings.MinLength)
        {
            return UsernameCheck.TooShort;
        }

        if (name.Length > settings.MaxLength)
        {
            return UsernameCheck.TooLong;
        }

        return !AllowsCharacters(name) || _profanityFilter.IsUnacceptable(name)
            ? UsernameCheck.Invalid
            : UsernameCheck.Ok;
    }

    public bool AllowsCharacters(string text)
    {
        try
        {
            return AllowedCharacters(_gameSettings.Current.Username.AllowedChars).IsMatch(text);
        }
        catch (RegexMatchTimeoutException)
        {
            return false;
        }
    }

    private Regex AllowedCharacters(string pattern)
    {
        if (_allowed is { } allowed && allowed.Pattern == pattern)
        {
            return allowed.Regex;
        }

        Regex regex;
        try
        {
            regex = new Regex(pattern, RegexOptions.CultureInvariant, MatchTimeout);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "global_game_settings.username_allowed_chars isn't a valid regex. I will use the default.");
            regex = new Regex(UsernameSettings.DefaultAllowedChars, RegexOptions.CultureInvariant, MatchTimeout);
        }

        _allowed = (pattern, regex);
        return regex;
    }
}
