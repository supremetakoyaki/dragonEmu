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

    public async Task<IReadOnlyDictionary<Language, ModerationPresets>> GetPresetsAsync(CancellationToken cancellationToken = default)
    {
        await using MySqlConnection connection = await _connectionFactory.CreateOpenConnectionAsync(cancellationToken);
        await using var command = new MySqlCommand(
            "SELECT type, category, name, message FROM definition_moderation_presets ORDER BY order_num, id;", connection);

        var userMessages = new List<LocalizedText>();
        var roomMessages = new List<LocalizedText>();
        var offences = new List<(LocalizedText Category, LocalizedText Name, LocalizedText Message)>();
        await using MySqlDataReader reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            switch (reader.GetString(0))
            {
                case "user_message":
                    userMessages.Add(LocalizedText.Parse(reader.GetString(3)));
                    break;
                case "room_message":
                    roomMessages.Add(LocalizedText.Parse(reader.GetString(3)));
                    break;
                default:
                    offences.Add((LocalizedText.Parse(reader.GetString(1)), LocalizedText.Parse(reader.GetString(2)), LocalizedText.Parse(reader.GetString(3))));
                    break;
            }
        }

        return LanguageExtensions.FallbackOrder.ToDictionary(language => language, language => new ModerationPresets(
            [.. userMessages.Select(message => message.For(language))],
            [.. roomMessages.Select(message => message.For(language))],
            [.. offences
                .GroupBy(offence => offence.Category.For(Language.En))
                .Select(group => new OffenceCategory(
                    group.First().Category.For(language),
                    [.. group.Select(offence => (offence.Name.For(language), offence.Message.For(language)))]))]));
    }

    public async Task<IReadOnlyDictionary<Language, IReadOnlyList<FaqCategory>>> GetFaqAsync(CancellationToken cancellationToken = default)
    {
        await using MySqlConnection connection = await _connectionFactory.CreateOpenConnectionAsync(cancellationToken);

        var entries = new Dictionary<int, List<(int Id, LocalizedText Question, LocalizedText Answer, FaqFrontPage FrontPage)>>();
        await using (var entryQuery = new MySqlCommand(
            "SELECT id, category_ref, question, answer, front_page FROM definition_help_faq_entries ORDER BY order_num, id;", connection))
        await using (MySqlDataReader reader = await entryQuery.ExecuteReaderAsync(cancellationToken))
        {
            while (await reader.ReadAsync(cancellationToken))
            {
                int categoryId = reader.GetInt32(1);
                if (!entries.TryGetValue(categoryId, out var list))
                {
                    entries[categoryId] = list = [];
                }

                list.Add((reader.GetInt32(0), LocalizedText.Parse(reader.GetString(2)), LocalizedText.Parse(reader.GetString(3)),
                    reader.GetString(4) switch { "urgent" => FaqFrontPage.Urgent, "normal" => FaqFrontPage.Normal, _ => FaqFrontPage.None }));
            }
        }

        var categories = new List<(int Id, LocalizedText Name, LocalizedText Description)>();
        await using (var categoryQuery = new MySqlCommand(
            "SELECT id, name, description FROM definition_help_faq_categories ORDER BY order_num, id;", connection))
        await using (MySqlDataReader reader = await categoryQuery.ExecuteReaderAsync(cancellationToken))
        {
            while (await reader.ReadAsync(cancellationToken))
            {
                categories.Add((reader.GetInt32(0), LocalizedText.Parse(reader.GetString(1)), LocalizedText.Parse(reader.GetString(2))));
            }
        }

        return LanguageExtensions.FallbackOrder.ToDictionary(language => language, language => (IReadOnlyList<FaqCategory>)
            [.. categories.Select(category => new FaqCategory(
                category.Id,
                category.Name.For(language),
                category.Description.For(language),
                entries.TryGetValue(category.Id, out var list)
                    ? [.. list.Select(entry => new FaqEntry(entry.Id, category.Id, entry.Question.For(language), entry.Answer.For(language), entry.FrontPage))]
                    : []))]);
    }
}
