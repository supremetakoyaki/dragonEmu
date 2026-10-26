using System.Text.RegularExpressions;
using DragonEmu.Domain;
using DragonEmu.Game.Chat;
using DragonEmu.Game.Settings;
using Microsoft.Extensions.Logging;

namespace DragonEmu.Game.Pets;

public sealed class PetNameRules
{
    private static readonly TimeSpan MatchTimeout = TimeSpan.FromMilliseconds(100);

    private readonly GameSettingsCache _gameSettings;
    private readonly ProfanityFilterCache _profanityFilter;
    private readonly ILogger<PetNameRules> _logger;

    private (string Pattern, Regex Regex)? _allowed;

    public PetNameRules(GameSettingsCache gameSettings, ProfanityFilterCache profanityFilter, ILogger<PetNameRules> logger)
    {
        _gameSettings = gameSettings;
        _profanityFilter = profanityFilter;
        _logger = logger;
    }

    public static string Normalize(string name) => name.Trim();

    public PetNameCheck Check(string name)
    {
        PetSettings settings = _gameSettings.Current.Pets;
        if (name.Length > settings.NameMaxLength)
        {
            return PetNameCheck.TooLong;
        }

        if (name.Length == 0 || !Matches(AllowedCharacters(settings.NameAllowedChars), name))
        {
            return PetNameCheck.ForbiddenCharacters;
        }

        return _profanityFilter.IsUnacceptable(name) ? PetNameCheck.Bobba : PetNameCheck.Ok;
    }

    private static bool Matches(Regex regex, string name)
    {
        try
        {
            return regex.IsMatch(name);
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
            _logger.LogWarning(ex, "global_game_settings.pet_name_allowed_chars isn't a valid regex. I will use the default.");
            regex = new Regex(PetSettings.DefaultNameAllowedChars, RegexOptions.CultureInvariant, MatchTimeout);
        }

        _allowed = (pattern, regex);
        return regex;
    }
}
