using System.Text.Json;
using DragonEmu.Persistence;

namespace DragonEmu.Game.Rooms;

public static class BotDefinitionParser
{
    public const int DefaultMinTicks = 60;
    public const int DefaultMaxTicks = 180;

    public const int DefaultCooldownTicks = 10;

    private const int MaxCoordinate = 255;

    public static BotDefinition Parse(RoomBotRecord record, Action<string> warn)
    {
        IReadOnlySet<(int X, int Y)>? area = null;
        if (record.WalkableAreaJson is { } areaJson)
        {
            try
            {
                area = ParseArea(areaJson);
            }
            catch (Exception ex) when (ex is JsonException or FormatException)
            {
                warn($"bot {record.Id} ({record.Name})'s walkable_area isn't [x, y] tiles and [x1, y1, x2, y2] rectangles ({ex.Message}). It will stay put.");
                area = new HashSet<(int X, int Y)>();
            }
        }

        var actions = new List<BotAction>();
        foreach (RoomBotActionRecord action in record.Actions)
        {
            try
            {
                actions.Add(ParseAction(action));
            }
            catch (Exception ex) when (ex is JsonException or FormatException or InvalidOperationException or KeyNotFoundException)
            {
                warn($"bot {record.Id} ({record.Name}): action {action.Id} left out because: {ex.Message}");
            }
        }

        return new BotDefinition(
            record.Id, record.Name, record.Gender, record.Figure, record.Motto, record.X, record.Y, record.Z,
            ((record.Rotation % 8) + 8) % 8, area, record.DiagonalMoveAllowed, actions);
    }

    public static IReadOnlySet<(int X, int Y)> ParseArea(string json)
    {
        using JsonDocument document = JsonDocument.Parse(json);
        if (document.RootElement.ValueKind != JsonValueKind.Array)
        {
            throw new FormatException("not a list");
        }

        var tiles = new HashSet<(int X, int Y)>();
        foreach (JsonElement entry in document.RootElement.EnumerateArray())
        {
            int[] numbers = entry.ValueKind == JsonValueKind.Array ? entry.EnumerateArray().Select(Coordinate).ToArray() : [];
            switch (numbers.Length)
            {
                case 2:
                    tiles.Add((numbers[0], numbers[1]));
                    break;
                case 4:
                    for (int x = Math.Min(numbers[0], numbers[2]); x <= Math.Max(numbers[0], numbers[2]); x++)
                    {
                        for (int y = Math.Min(numbers[1], numbers[3]); y <= Math.Max(numbers[1], numbers[3]); y++)
                        {
                            tiles.Add((x, y));
                        }
                    }

                    break;
                default:
                    throw new FormatException($"{entry.GetRawText()} is neither [x, y] nor [x1, y1, x2, y2]");
            }
        }

        return tiles;
    }

    private static int Coordinate(JsonElement number) =>
        number.ValueKind == JsonValueKind.Number && number.TryGetInt32(out int value) && value is >= 0 and <= MaxCoordinate
            ? value
            : throw new FormatException($"{number.GetRawText()} isn't a coordinate (0-{MaxCoordinate})");

    private static BotAction ParseAction(RoomBotActionRecord record)
    {
        BotTrigger trigger = record.TriggerType switch
        {
            "user_says" => BotTrigger.UserSays,
            "user_shouts" => BotTrigger.UserShouts,
            "do_at_random" => BotTrigger.Random,
            "user_chats" => BotTrigger.UserChats,
            "on_arrival" => BotTrigger.OnArrival,
            _ => throw new FormatException($"unknown trigger_type '{record.TriggerType}'"),
        };

        using JsonDocument? triggerData = record.TriggerDataJson is { } dataJson ? JsonDocument.Parse(dataJson) : null;
        JsonElement? data = triggerData?.RootElement.ValueKind == JsonValueKind.Object ? triggerData.RootElement : null;

        PhraseMatcher? phrases = null;
        int minTicks = DefaultMinTicks, maxTicks = DefaultMaxTicks, cooldownTicks = DefaultCooldownTicks;
        if (trigger == BotTrigger.Random)
        {
            minTicks = Int(data, "min_ticks", DefaultMinTicks, minimum: 1);
            maxTicks = Int(data, "max_ticks", Math.Max(minTicks, DefaultMaxTicks), minimum: minTicks);
        }
        else if (trigger != BotTrigger.OnArrival)
        {
            if (data is not { } chat || !chat.TryGetProperty("phrases", out JsonElement list) || list.ValueKind != JsonValueKind.Array)
            {
                throw new FormatException("trigger_data needs \"phrases\": [...]");
            }

            PhraseMatch match = String(data, "match") switch
            {
                null or "exact" => PhraseMatch.Exact,
                "contains" => PhraseMatch.Contains,
                "starts_with" => PhraseMatch.StartsWith,
                string other => throw new FormatException($"unknown match '{other}'"),
            };

            bool caseSensitive = chat.TryGetProperty("case_sensitive", out JsonElement sensitive) && sensitive.ValueKind == JsonValueKind.True;
            phrases = new PhraseMatcher(list.EnumerateArray().Where(p => p.ValueKind == JsonValueKind.String).Select(p => p.GetString()!), match, caseSensitive);
            if (phrases.IsEmpty)
            {
                throw new FormatException("no phrases");
            }

            cooldownTicks = Int(data, "cooldown_ticks", DefaultCooldownTicks, minimum: 0);
        }

        using JsonDocument responses = JsonDocument.Parse(record.ResponsesJson);
        if (responses.RootElement.ValueKind != JsonValueKind.Array || responses.RootElement.GetArrayLength() == 0)
        {
            throw new FormatException("responses must be a non-empty list");
        }

        var alternatives = new List<IReadOnlyList<BotStep>>();
        foreach (JsonElement alternative in responses.RootElement.EnumerateArray())
        {
            List<BotStep> steps = alternative.ValueKind == JsonValueKind.Array
                ? alternative.EnumerateArray().Select(ParseStep).ToList()
                : [ParseStep(alternative)];
            if (steps.Count == 0)
            {
                throw new FormatException("an empty list of steps");
            }

            alternatives.Add(steps);
        }

        return new BotAction(record.Id, trigger, phrases, minTicks, maxTicks, cooldownTicks, record.TileDistanceRequired, alternatives);
    }

    private static BotStep ParseStep(JsonElement step)
    {
        if (step.ValueKind != JsonValueKind.Object)
        {
            throw new FormatException($"{step.GetRawText()} isn't a step");
        }

        return String(step, "type") switch
        {
            "say" => new BotStep(BotStepType.Say, Text(step)),
            "shout" => new BotStep(BotStepType.Shout, Text(step)),
            "whisper" => new BotStep(BotStepType.Whisper, Text(step)),
            "walk" => new BotStep(
                BotStepType.Walk,
                Text: String(step, "to") switch
                {
                    null => string.Empty,
                    "center" => "center",
                    string other => throw new FormatException($"a walk goes \"to\": \"center\", not '{other}'"),
                },
                Value: Int(step, "max_distance", 0, minimum: 0),
                Tile: step.TryGetProperty("x", out _) || step.TryGetProperty("y", out _)
                    ? (Int(step, "x", -1, minimum: 0), Int(step, "y", -1, minimum: 0))
                    : null),
            "wave" => new BotStep(BotStepType.Wave),
            "dance" => new BotStep(BotStepType.Dance, Value: Int(step, "ticks", -1, minimum: 1)),
            "look_at" => new BotStep(BotStepType.LookAt, String(step, "who") switch
            {
                "owner" => "owner",
                "user" => "user",
                _ => throw new FormatException($"{step.GetRawText()}: \"who\" is \"owner\" or \"user\""),
            }),
            "hand_item" => new BotStep(BotStepType.HandItem, Value: Int(step, "id", -1, minimum: 0)),
            "wait" => new BotStep(BotStepType.Wait, Value: Int(step, "ticks", -1, minimum: 1)),
            string other => throw new FormatException($"unknown step type '{other}'"),
            null => throw new FormatException($"{step.GetRawText()} has no \"type\""),
        };
    }

    private static string Text(JsonElement step) =>
        String(step, "text") is { Length: > 0 } text ? text : throw new FormatException($"{step.GetRawText()} needs a \"text\"");

    private static string? String(JsonElement? element, string name) =>
        element is { } e && e.TryGetProperty(name, out JsonElement value) && value.ValueKind == JsonValueKind.String ? value.GetString() : null;

    private static int Int(JsonElement? element, string name, int fallback, int minimum)
    {
        if (element is not { } e || !e.TryGetProperty(name, out JsonElement value))
        {
            return fallback >= minimum ? fallback : throw new FormatException($"\"{name}\" is missing");
        }

        return value.ValueKind == JsonValueKind.Number && value.TryGetInt32(out int number) && number >= minimum
            ? number
            : throw new FormatException($"\"{name}\" must be a whole number of at least {minimum}");
    }
}
