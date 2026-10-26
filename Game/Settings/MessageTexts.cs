using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using DragonEmu.Persistence;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace DragonEmu.Game.Settings;

public enum DurationUnit
{
    Minute,
    Hour,
    Day,
}

public sealed partial class MessageTexts : IHostedService
{
    private readonly IMessageTextRepository? _repository;
    private readonly ILogger<MessageTexts>? _logger;
    private readonly HashSet<string> _reported = [];

    private volatile IReadOnlyDictionary<string, string> _texts;

    public MessageTexts(IMessageTextRepository repository, ILogger<MessageTexts> logger)
    {
        _repository = repository;
        _logger = logger;
        _texts = new Dictionary<string, string>();
    }

    private MessageTexts(IReadOnlyDictionary<string, string> texts)
    {
        _texts = texts;
    }

    public static MessageTexts From(IReadOnlyDictionary<string, string> texts) => new(texts);

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        _texts = await _repository!.GetAllAsync(cancellationToken);

        string[] missing = [.. MessageKeys.All.Where(key => !_texts.ContainsKey(key))];
        if (missing.Length > 0)
        {
            _logger!.LogWarning("global_message_texts has no row for: {Keys}.", string.Join(", ", missing));
        }
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    public string Get(string key)
    {
        if (_texts.TryGetValue(key, out string? text))
        {
            return text;
        }

        lock (_reported)
        {
            if (_reported.Add(key))
            {
                _logger?.LogWarning("No text for message key '{Key}' in global_message_texts.", key);
            }
        }

        return key;
    }

    public string Format(string key, params (string Name, object? Value)[] values)
    {
        string text = Get(key);
        if (values.Length == 0)
        {
            return text;
        }

        return Placeholder().Replace(text, match =>
        {
            foreach ((string name, object? value) in values)
            {
                if (name == match.Groups[1].Value)
                {
                    return Convert.ToString(value, System.Globalization.CultureInfo.InvariantCulture) ?? string.Empty;
                }
            }

            return match.Value;
        });
    }

    public string Duration(DurationUnit unit, int count) => count == 1
        ? Get(unit switch { DurationUnit.Minute => MessageKeys.DurationOneMinute, DurationUnit.Hour => MessageKeys.DurationOneHour, _ => MessageKeys.DurationOneDay })
        : Format(unit switch { DurationUnit.Minute => MessageKeys.DurationMinutes, DurationUnit.Hour => MessageKeys.DurationHours, _ => MessageKeys.DurationDays }, ("count", count));

    [GeneratedRegex(@"\{([a-z_]+)\}")]
    private static partial Regex Placeholder();
}
