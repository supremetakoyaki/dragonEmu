using DragonEmu.Domain;
using MySqlConnector;

namespace DragonEmu.Persistence;

public sealed class DefinitionModerationRepository : IDefinitionModerationRepository
{
    private readonly IDbConnectionFactory _connectionFactory;

    public DefinitionModerationRepository(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<ModerationPresets> GetPresetsAsync(CancellationToken cancellationToken = default)
    {
        await using MySqlConnection connection = await _connectionFactory.CreateOpenConnectionAsync(cancellationToken);
        await using var command = new MySqlCommand(
            "SELECT type, category, name, message FROM definition_moderation_presets ORDER BY order_num, id;", connection);

        var userMessages = new List<string>();
        var roomMessages = new List<string>();
        var offences = new List<(string Category, string Name, string Message)>();
        await using MySqlDataReader reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            switch (reader.GetString(0))
            {
                case "user_message":
                    userMessages.Add(reader.GetString(3));
                    break;
                case "room_message":
                    roomMessages.Add(reader.GetString(3));
                    break;
                default:
                    offences.Add((reader.GetString(1), reader.GetString(2), reader.GetString(3)));
                    break;
            }
        }

        List<OffenceCategory> categories = offences
            .GroupBy(offence => offence.Category)
            .Select(group => new OffenceCategory(group.Key, group.Select(offence => (offence.Name, offence.Message)).ToList()))
            .ToList();
        return new ModerationPresets(userMessages, roomMessages, categories);
    }

    public async Task<IReadOnlyList<FaqCategory>> GetFaqAsync(CancellationToken cancellationToken = default)
    {
        await using MySqlConnection connection = await _connectionFactory.CreateOpenConnectionAsync(cancellationToken);

        var entries = new Dictionary<int, List<FaqEntry>>();
        await using (var entryQuery = new MySqlCommand(
            "SELECT id, category_ref, question, answer, front_page FROM definition_help_faq_entries ORDER BY order_num, id;", connection))
        await using (MySqlDataReader reader = await entryQuery.ExecuteReaderAsync(cancellationToken))
        {
            while (await reader.ReadAsync(cancellationToken))
            {
                var entry = new FaqEntry(
                    reader.GetInt32(0), reader.GetInt32(1), reader.GetString(2), reader.GetString(3),
                    reader.GetString(4) switch { "urgent" => FaqFrontPage.Urgent, "normal" => FaqFrontPage.Normal, _ => FaqFrontPage.None });
                if (!entries.TryGetValue(entry.CategoryId, out List<FaqEntry>? list))
                {
                    entries[entry.CategoryId] = list = [];
                }

                list.Add(entry);
            }
        }

        var categories = new List<FaqCategory>();
        await using (var categoryQuery = new MySqlCommand(
            "SELECT id, name, description FROM definition_help_faq_categories ORDER BY order_num, id;", connection))
        await using (MySqlDataReader reader = await categoryQuery.ExecuteReaderAsync(cancellationToken))
        {
            while (await reader.ReadAsync(cancellationToken))
            {
                int id = reader.GetInt32(0);
                categories.Add(new FaqCategory(id, reader.GetString(1), reader.GetString(2), entries.TryGetValue(id, out List<FaqEntry>? list) ? list : []));
            }
        }

        return categories;
    }
}
