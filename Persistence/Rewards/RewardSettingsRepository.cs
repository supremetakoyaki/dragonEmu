using DragonEmu.Domain;
using MySqlConnector;

namespace DragonEmu.Persistence;

public sealed class RewardSettingsRepository : IRewardSettingsRepository
{
    private readonly IDbConnectionFactory _connectionFactory;

    public RewardSettingsRepository(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<OnlineRewardSettings> GetAsync(CancellationToken cancellationToken = default)
    {
        await using MySqlConnection connection = await _connectionFactory.CreateOpenConnectionAsync(cancellationToken);
        await using var command = new MySqlCommand(
            """
            SELECT rewards_enabled, credit_reward_interval, credit_reward_amount, pixel_reward_interval, pixel_reward_amount
            FROM global_reward_settings
            LIMIT 1;
            """, connection);

        await using MySqlDataReader reader = await command.ExecuteReaderAsync(cancellationToken);
        if (!await reader.ReadAsync(cancellationToken))
        {
            return OnlineRewardSettings.Default;
        }

        return new OnlineRewardSettings(
            Enabled: reader.GetString(0) == "1",
            CreditInterval: reader.GetInt32(1),
            CreditAmount: reader.GetInt32(2),
            PixelInterval: reader.GetInt32(3),
            PixelAmount: reader.GetInt32(4));
    }
}
