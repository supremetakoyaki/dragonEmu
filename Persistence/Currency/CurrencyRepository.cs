using DragonEmu.Domain;
using MySqlConnector;

namespace DragonEmu.Persistence;

public sealed class CurrencyRepository : ICurrencyRepository
{
    private static readonly int[] DefaultCurrencies = [CurrencyType.Credits, CurrencyType.ActivityPoints];

    private readonly IDbConnectionFactory _connectionFactory;

    public CurrencyRepository(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<long> GetBalanceAsync(int userId, int currency, CancellationToken cancellationToken = default)
    {
        await using MySqlConnection connection = await _connectionFactory.CreateOpenConnectionAsync(cancellationToken);
        await using var command = new MySqlCommand(
            "SELECT value FROM users_currency WHERE user_ref = @userId AND currency = @currency;", connection);
        command.Parameters.AddWithValue("@userId", userId);
        command.Parameters.AddWithValue("@currency", currency);

        object? result = await command.ExecuteScalarAsync(cancellationToken);
        return result is null ? 0 : Convert.ToInt64(result);
    }

    public async Task SetBalanceAsync(int userId, int currency, long value, CancellationToken cancellationToken = default)
    {
        await using MySqlConnection connection = await _connectionFactory.CreateOpenConnectionAsync(cancellationToken);
        await using var command = new MySqlCommand(
            """
            INSERT INTO users_currency (user_ref, currency, value) VALUES (@userId, @currency, @value)
            ON DUPLICATE KEY UPDATE value = VALUES(value);
            """, connection);
        command.Parameters.AddWithValue("@userId", userId);
        command.Parameters.AddWithValue("@currency", currency);
        command.Parameters.AddWithValue("@value", value);

        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    public async Task SeedDefaultBalancesAsync(int userId, CancellationToken cancellationToken = default)
    {
        foreach (int currency in DefaultCurrencies)
        {
            await SetBalanceAsync(userId, currency, 0, cancellationToken);
        }
    }

    public async Task<long> AddBalanceAsync(int userId, int currency, long delta, CancellationToken cancellationToken = default)
    {
        await using MySqlConnection connection = await _connectionFactory.CreateOpenConnectionAsync(cancellationToken);

        await using (var upsert = new MySqlCommand(
            """
            INSERT INTO users_currency (user_ref, currency, value) VALUES (@userId, @currency, @delta)
            ON DUPLICATE KEY UPDATE value = value + VALUES(value);
            """, connection))
        {
            upsert.Parameters.AddWithValue("@userId", userId);
            upsert.Parameters.AddWithValue("@currency", currency);
            upsert.Parameters.AddWithValue("@delta", delta);
            await upsert.ExecuteNonQueryAsync(cancellationToken);
        }

        await using var select = new MySqlCommand(
            "SELECT value FROM users_currency WHERE user_ref = @userId AND currency = @currency;", connection);
        select.Parameters.AddWithValue("@userId", userId);
        select.Parameters.AddWithValue("@currency", currency);
        return Convert.ToInt64(await select.ExecuteScalarAsync(cancellationToken));
    }

    public async Task<IReadOnlyDictionary<int, long>> AddBalanceToManyAsync(
        IReadOnlyCollection<int> userIds, int currency, long delta, CancellationToken cancellationToken = default)
    {
        var balances = new Dictionary<int, long>(userIds.Count);
        if (userIds.Count == 0)
        {
            return balances;
        }

        string[] users = [.. Enumerable.Range(0, userIds.Count).Select(i => $"@u{i}")];
        await using MySqlConnection connection = await _connectionFactory.CreateOpenConnectionAsync(cancellationToken);

        await using (var upsert = new MySqlCommand(
            $"""
            INSERT INTO users_currency (user_ref, currency, value) VALUES {string.Join(", ", users.Select(user => $"({user}, @currency, @delta)"))}
            ON DUPLICATE KEY UPDATE value = value + VALUES(value);
            """, connection))
        {
            Bind(upsert);
            upsert.Parameters.AddWithValue("@delta", delta);
            await upsert.ExecuteNonQueryAsync(cancellationToken);
        }

        await using var select = new MySqlCommand(
            $"SELECT user_ref, value FROM users_currency WHERE currency = @currency AND user_ref IN ({string.Join(", ", users)});", connection);
        Bind(select);
        await using MySqlDataReader reader = await select.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            balances[reader.GetInt32(0)] = Convert.ToInt64(reader.GetValue(1));
        }

        return balances;

        void Bind(MySqlCommand command)
        {
            int index = 0;
            foreach (int userId in userIds)
            {
                command.Parameters.AddWithValue($"@u{index++}", userId);
            }

            command.Parameters.AddWithValue("@currency", currency);
        }
    }
}
