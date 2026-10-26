using DragonEmu.Domain;
using MySqlConnector;

namespace DragonEmu.Persistence;

public sealed class VoucherRepository : IVoucherRepository
{
    private readonly IDbConnectionFactory _connectionFactory;

    public VoucherRepository(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<VoucherCode?> GetActiveByHashAsync(string hashCode, CancellationToken cancellationToken = default)
    {
        await using MySqlConnection connection = await _connectionFactory.CreateOpenConnectionAsync(cancellationToken);
        await using var command = new MySqlCommand(
            "SELECT id, redeemable_type, redeemable_data FROM voucher_codes WHERE hash_code = @hashCode AND active = 1 LIMIT 1;", connection);
        command.Parameters.AddWithValue("@hashCode", hashCode);

        await using MySqlDataReader reader = await command.ExecuteReaderAsync(cancellationToken);
        if (!await reader.ReadAsync(cancellationToken))
        {
            return null;
        }

        return new VoucherCode(reader.GetInt32(0), reader.GetString(1), reader.GetString(2));
    }

    public async Task<bool> TryClaimAsync(int id, int userId, string userIp, CancellationToken cancellationToken = default)
    {
        await using MySqlConnection connection = await _connectionFactory.CreateOpenConnectionAsync(cancellationToken);
        await using var command = new MySqlCommand(
            """
            UPDATE voucher_codes
            SET active = 2, redeemed_date = UTC_TIMESTAMP(), redeemed_user_ref = @userId, redeemed_user_ip = @userIp
            WHERE id = @id AND active = 1;
            """, connection);
        command.Parameters.AddWithValue("@userId", userId);
        command.Parameters.AddWithValue("@userIp", userIp);
        command.Parameters.AddWithValue("@id", id);

        return await command.ExecuteNonQueryAsync(cancellationToken) > 0;
    }
}
