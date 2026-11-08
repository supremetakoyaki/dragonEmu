using System.Text.Json;

namespace DragonEmu.Domain;

public sealed class LocalizedList
{
    public static LocalizedList Empty { get; } = new(new Dictionary<Language, IReadOnlyList<string?>>());

    private readonly IReadOnlyDictionary<Language, IReadOnlyList<string?>> _lists;

    private LocalizedList(IReadOnlyDictionary<Language, IReadOnlyList<string?>> lists)
    {
        _lists = lists;
    }

    public static LocalizedList English(IReadOnlyList<string?> list) => new(new Dictionary<Language, IReadOnlyList<string?>> { [Language.En] = list });

    public static LocalizedList Parse(string? json)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            return Empty;
        }

        using JsonDocument document = JsonDocument.Parse(json);
        JsonElement root = document.RootElement;
        if (root.ValueKind == JsonValueKind.Array)
        {
            return English(Strings(root));
        }

        var lists = new Dictionary<Language, IReadOnlyList<string?>>();
        if (root.ValueKind == JsonValueKind.Object)
        {
            foreach (JsonProperty property in root.EnumerateObject())
            {
                if (LanguageExtensions.FromCode(property.Name) is { } language && property.Value.ValueKind == JsonValueKind.Array)
                {
                    lists[language] = Strings(property.Value);
                }
            }
        }

        return new LocalizedList(lists);
    }

    private static List<string?> Strings(JsonElement array) =>
        [.. array.EnumerateArray().Select(item => item.ValueKind == JsonValueKind.String ? item.GetString() : null)];

    public IReadOnlyList<string> For(Language language)
    {
        Language[] order = [language, .. LanguageExtensions.FallbackOrder.Where(other => other != language)];
        int count = _lists.Values.Select(list => list.Count).DefaultIfEmpty(0).Max();
        var result = new string[count];
        for (int slot = 0; slot < count; slot++)
        {
            result[slot] = string.Empty;
            foreach (Language candidate in order)
            {
                if (_lists.TryGetValue(candidate, out IReadOnlyList<string?>? list) && slot < list.Count && list[slot] is { } text)
                {
                    result[slot] = text;
                    break;
                }
            }
        }

        return result;
    }
}
