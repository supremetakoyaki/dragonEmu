using System.Text.Json;

namespace DragonEmu.Domain;

public sealed class LocalizedText
{
    public static LocalizedText Empty { get; } = new(new Dictionary<Language, string>());

    private readonly IReadOnlyDictionary<Language, string> _texts;

    private LocalizedText(IReadOnlyDictionary<Language, string> texts)
    {
        _texts = texts;
    }

    public static LocalizedText English(string text) => new(new Dictionary<Language, string> { [Language.En] = text });

    public static LocalizedText Of(IReadOnlyDictionary<Language, string> texts) =>
        new(texts.Where(entry => !string.IsNullOrEmpty(entry.Value)).ToDictionary(entry => entry.Key, entry => entry.Value));

    public static LocalizedText Parse(string? json)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            return Empty;
        }

        JsonDocument document;
        try
        {
            document = JsonDocument.Parse(json);
        }
        catch (JsonException)
        {
            return English(json);
        }

        using (document)
        {
            return document.RootElement.ValueKind == JsonValueKind.Object ? Of(document.RootElement) : English(json);
        }
    }

    private static LocalizedText Of(JsonElement json)
    {
        var texts = new Dictionary<Language, string>();
        foreach (JsonProperty property in json.EnumerateObject())
        {
            if (LanguageExtensions.FromCode(property.Name) is { } language && property.Value.ValueKind == JsonValueKind.String)
            {
                texts[language] = property.Value.GetString()!;
            }
        }

        return Of(texts);
    }

    public string For(Language language)
    {
        if (_texts.TryGetValue(language, out string? text))
        {
            return text;
        }

        foreach (Language fallback in LanguageExtensions.FallbackOrder)
        {
            if (_texts.TryGetValue(fallback, out text))
            {
                return text;
            }
        }

        return string.Empty;
    }

    public IEnumerable<string> All => _texts.Values;

    public bool IsEmpty => _texts.Count == 0;

    public override bool Equals(object? obj) =>
        obj is LocalizedText other && other._texts.Count == _texts.Count
        && _texts.All(entry => other._texts.TryGetValue(entry.Key, out string? text) && text == entry.Value);

    public override int GetHashCode() =>
        _texts.OrderBy(entry => entry.Key).Aggregate(0, (hash, entry) => HashCode.Combine(hash, entry.Key, entry.Value));

    public override string ToString() => For(Language.En);
}
