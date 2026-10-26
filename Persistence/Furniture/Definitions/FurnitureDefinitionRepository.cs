using System.Text.Json;
using DragonEmu.Domain;
using MySqlConnector;

namespace DragonEmu.Persistence;

public sealed class FurnitureDefinitionRepository : IFurnitureDefinitionRepository
{
    private const string BaseSelect =
        """
        SELECT id, type, ref_furnidata, classname, x_dim, y_dim, z_dim, localization_name, localization_desc,
               logic_class, visualization_class, interaction_logic, states, interaction_ids, is_sittable,
               is_bed, is_walkable, is_stackable, allow_trade, allow_recycle
        FROM definition_furniture
        """;

    private readonly IDbConnectionFactory _connectionFactory;

    public FurnitureDefinitionRepository(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<IReadOnlyList<FurnitureDefinition>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        await using MySqlConnection connection = await _connectionFactory.CreateOpenConnectionAsync(cancellationToken);
        await using var command = new MySqlCommand($"{BaseSelect};", connection);

        var results = new List<FurnitureDefinition>();
        await using MySqlDataReader reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            results.Add(MapRow(reader));
        }

        return results;
    }

    private static FurnitureDefinition MapRow(MySqlDataReader reader) =>
        new(
            Id: reader.GetInt32(0),
            Type: reader.GetString(1) == "s" ? FurnitureType.Floor : FurnitureType.Wall,
            RefFurnidata: reader.GetInt32(2),
            Classname: reader.GetString(3),
            XDim: reader.IsDBNull(4) ? null : reader.GetInt32(4),
            YDim: reader.IsDBNull(5) ? null : reader.GetInt32(5),
            ZDim: reader.IsDBNull(6) ? null : reader.GetDouble(6),
            LocalizationName: reader.GetString(7),
            LocalizationDesc: reader.GetString(8),
            LogicClass: FurnitureLogicClassExtensions.FromDbString(reader.GetString(9)),
            VisualizationClass: FurnitureVisualizationClassExtensions.FromDbString(reader.GetString(10)),
            InteractionLogic: FurnitureInteractionLogicExtensions.FromDbString(reader.GetString(11)),
            States: reader.IsDBNull(12) ? null : ParseIntArray(reader.GetString(12)),
            InteractionIds: reader.IsDBNull(13) ? null : ParseIntArray(reader.GetString(13)),
            IsSittable: reader.GetBoolean(14),
            IsBed: reader.GetBoolean(15),
            IsWalkable: reader.GetBoolean(16),
            IsStackable: reader.GetBoolean(17),
            AllowTrade: reader.GetBoolean(18),
            AllowRecycle: reader.GetBoolean(19));

    private static IReadOnlyList<int> ParseIntArray(string json) =>
        JsonSerializer.Deserialize<List<int>>(json) ?? [];
}
