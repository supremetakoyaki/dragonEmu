using System.Text.RegularExpressions;
using DragonEmu.Game.Settings;
using DragonEmu.Persistence;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace DragonEmu.Game.Chat;

public sealed class ProfanityFilterCache : IHostedService
{
    private readonly GameSettingsCache _gameSettings;
    private readonly IProfanityWordRepository _profanityWordRepository;
    private readonly ILogger<ProfanityFilterCache> _logger;

    private volatile Snapshot _snapshot = Snapshot.Empty;

    public ProfanityFilterCache(
        GameSettingsCache gameSettings, IProfanityWordRepository profanityWordRepository, ILogger<ProfanityFilterCache> logger)
    {
        _gameSettings = gameSettings;
        _profanityWordRepository = profanityWordRepository;
        _logger = logger;
    }

    public string Filter(string text)
    {
        Snapshot snapshot = _snapshot;
        if (!snapshot.Enabled || string.IsNullOrEmpty(text) || snapshot.Pattern is null)
        {
            return text;
        }

        return snapshot.Pattern.Replace(text, snapshot.ReplacementWord);
    }

    public bool IsUnacceptable(string text)
    {
        Snapshot snapshot = _snapshot;
        return snapshot.Enabled && !string.IsNullOrEmpty(text) && snapshot.Pattern?.IsMatch(text) == true;
    }

    public async Task ReloadAsync(CancellationToken cancellationToken = default)
    {
        (bool enabled, string replacementWord) = (_gameSettings.Current.ProfanityFilterEnabled, _gameSettings.Current.ProfanityFilterReplacementWord);
        IReadOnlyList<string> words = await _profanityWordRepository.GetAllAsync(cancellationToken);

        Regex? pattern = words.Count == 0 ? null : BuildPattern(words);
        _snapshot = new Snapshot(enabled, replacementWord, pattern);

        _logger.LogInformation(
            "Profanity filter cache loaded: enabled={Enabled}, {WordCount} words, replacement=\"{ReplacementWord}\".",
            enabled, words.Count, replacementWord);
    }

    public Task StartAsync(CancellationToken cancellationToken) => ReloadAsync(cancellationToken);

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    private static Regex BuildPattern(IReadOnlyList<string> words)
    {
        string alternation = string.Join('|', words.OrderByDescending(w => w.Length).Select(Regex.Escape));
        return new Regex($@"\b(?:{alternation})\b", RegexOptions.IgnoreCase | RegexOptions.Compiled | RegexOptions.CultureInvariant);
    }

    private sealed record Snapshot(bool Enabled, string ReplacementWord, Regex? Pattern)
    {
        public static readonly Snapshot Empty = new(false, "bobba", null);
    }
}
