using System.Security.Cryptography;
using DragonEmu.Domain;
using MySqlConnector;

namespace DragonEmu.Persistence;

public sealed class UserRepository : IUserRepository
{
    private readonly IDbConnectionFactory _connectionFactory;

    public UserRepository(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<(UserAccount Account, int LoginStreakDays)?> ValidateAndConsumeSsoTokenAsync(
        string token, string? ipAddress, DateTime todayStartUtc, DateTime yesterdayStartUtc, CancellationToken cancellationToken = default)
    {
        await using MySqlConnection connection = await _connectionFactory.CreateOpenConnectionAsync(cancellationToken);
        await using MySqlTransaction transaction = await connection.BeginTransactionAsync(cancellationToken);

        UserAccount? account = null;

        await using (var select = new MySqlCommand(
            """
            SELECT u.id, u.username, u.email, u.motto, u.`rank`, u.gender, u.figure, u.created_at, u.ref_favourite_group, g.badge
            FROM users_token t
            INNER JOIN users u ON u.id = t.user_id
            LEFT JOIN `groups` g ON g.id = u.ref_favourite_group
            WHERE t.token = @token AND t.expires_at > UTC_TIMESTAMP()
            FOR UPDATE;
            """, connection, transaction))
        {
            select.Parameters.AddWithValue("@token", token);

            await using MySqlDataReader reader = await select.ExecuteReaderAsync(cancellationToken);
            if (await reader.ReadAsync(cancellationToken))
            {
                account = new UserAccount(
                    reader.GetInt32(0),
                    reader.GetString(1),
                    reader.GetString(2),
                    reader.GetString(3),
                    reader.GetInt32(4),
                    reader.GetString(5)[0],
                    reader.GetString(6),
                    reader.GetDateTime(7),
                    reader.IsDBNull(8) ? null : new GroupBadge(reader.GetInt32(8), reader.GetString(9)));
            }
        }

        if (account is null)
        {
            await transaction.RollbackAsync(cancellationToken);
            return null;
        }

        await using (var deleteToken = new MySqlCommand("DELETE FROM users_token WHERE token = @token;", connection, transaction))
        {
            deleteToken.Parameters.AddWithValue("@token", token);
            await deleteToken.ExecuteNonQueryAsync(cancellationToken);
        }

        await using (var updateLogin = new MySqlCommand(
            """
            UPDATE users
            SET login_streak_days = CASE
                    WHEN last_login_at >= @todayStart THEN login_streak_days
                    WHEN last_login_at >= @yesterdayStart THEN login_streak_days + 1
                    ELSE 1
                END,
                last_login_at = UTC_TIMESTAMP(),
                last_login_ip = @ip
            WHERE id = @id;
            """, connection, transaction))
        {
            updateLogin.Parameters.AddWithValue("@ip", (object?)ipAddress ?? DBNull.Value);
            updateLogin.Parameters.AddWithValue("@todayStart", todayStartUtc);
            updateLogin.Parameters.AddWithValue("@yesterdayStart", yesterdayStartUtc);
            updateLogin.Parameters.AddWithValue("@id", account.Id);
            await updateLogin.ExecuteNonQueryAsync(cancellationToken);
        }

        int loginStreakDays;
        await using (var selectStreak = new MySqlCommand("SELECT login_streak_days FROM users WHERE id = @id;", connection, transaction))
        {
            selectStreak.Parameters.AddWithValue("@id", account.Id);
            loginStreakDays = Convert.ToInt32(await selectStreak.ExecuteScalarAsync(cancellationToken));
        }

        await transaction.CommitAsync(cancellationToken);
        return (account, loginStreakDays);
    }

    public async Task<bool> IsUsernameOrEmailTakenAsync(string username, string email, CancellationToken cancellationToken = default)
    {
        await using MySqlConnection connection = await _connectionFactory.CreateOpenConnectionAsync(cancellationToken);
        await using var command = new MySqlCommand(
            "SELECT 1 FROM users WHERE username = @username OR email = @email LIMIT 1;", connection);
        command.Parameters.AddWithValue("@username", username);
        command.Parameters.AddWithValue("@email", email);

        object? result = await command.ExecuteScalarAsync(cancellationToken);
        return result is not null;
    }

    public async Task<UserAccount> CreateUserAsync(
        string username, string email, string passwordHash, string motto, char gender, string figure, CancellationToken cancellationToken = default)
    {
        await using MySqlConnection connection = await _connectionFactory.CreateOpenConnectionAsync(cancellationToken);
        await using var command = new MySqlCommand(
            """
            INSERT INTO users (username, email, password_hash, motto, gender, figure, `rank`)
            VALUES (@username, @email, @passwordHash, @motto, @gender, @figure, 1);
            """, connection);
        command.Parameters.AddWithValue("@username", username);
        command.Parameters.AddWithValue("@email", email);
        command.Parameters.AddWithValue("@passwordHash", passwordHash);
        command.Parameters.AddWithValue("@motto", motto);
        command.Parameters.AddWithValue("@gender", gender.ToString());
        command.Parameters.AddWithValue("@figure", figure);

        await command.ExecuteNonQueryAsync(cancellationToken);
        return new UserAccount((int)command.LastInsertedId, username, email, motto, 1, gender, figure, DateTime.UtcNow);
    }

    public async Task<(UserAccount Account, string PasswordHash)?> FindForLoginAsync(
        string username, CancellationToken cancellationToken = default)
    {
        await using MySqlConnection connection = await _connectionFactory.CreateOpenConnectionAsync(cancellationToken);
        await using var command = new MySqlCommand(
            "SELECT id, username, email, motto, `rank`, gender, figure, password_hash, created_at FROM users WHERE username = @username;", connection);
        command.Parameters.AddWithValue("@username", username);

        await using MySqlDataReader reader = await command.ExecuteReaderAsync(cancellationToken);
        if (!await reader.ReadAsync(cancellationToken))
        {
            return null;
        }

        var account = new UserAccount(
            reader.GetInt32(0), reader.GetString(1), reader.GetString(2), reader.GetString(3),
            reader.GetInt32(4), reader.GetString(5)[0], reader.GetString(6), reader.GetDateTime(8));
        return (account, reader.GetString(7));
    }

    public async Task<UserAccount?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        await using MySqlConnection connection = await _connectionFactory.CreateOpenConnectionAsync(cancellationToken);
        await using var command = new MySqlCommand(
            "SELECT u.id, u.username, u.email, u.motto, u.`rank`, u.gender, u.figure, u.created_at, u.ref_favourite_group, g.badge FROM users u LEFT JOIN `groups` g ON g.id = u.ref_favourite_group WHERE u.id = @id;", connection);
        command.Parameters.AddWithValue("@id", id);

        await using MySqlDataReader reader = await command.ExecuteReaderAsync(cancellationToken);
        if (!await reader.ReadAsync(cancellationToken))
        {
            return null;
        }

        return new UserAccount(
            reader.GetInt32(0), reader.GetString(1), reader.GetString(2), reader.GetString(3),
            reader.GetInt32(4), reader.GetString(5)[0], reader.GetString(6), reader.GetDateTime(7),
            reader.IsDBNull(8) ? null : new GroupBadge(reader.GetInt32(8), reader.GetString(9)));
    }

    public async Task<UserAccount?> GetByUsernameAsync(string username, CancellationToken cancellationToken = default)
    {
        await using MySqlConnection connection = await _connectionFactory.CreateOpenConnectionAsync(cancellationToken);
        await using var command = new MySqlCommand(
            "SELECT u.id, u.username, u.email, u.motto, u.`rank`, u.gender, u.figure, u.created_at, u.ref_favourite_group, g.badge FROM users u LEFT JOIN `groups` g ON g.id = u.ref_favourite_group WHERE u.username = @username;", connection);
        command.Parameters.AddWithValue("@username", username);

        await using MySqlDataReader reader = await command.ExecuteReaderAsync(cancellationToken);
        if (!await reader.ReadAsync(cancellationToken))
        {
            return null;
        }

        return new UserAccount(
            reader.GetInt32(0), reader.GetString(1), reader.GetString(2), reader.GetString(3),
            reader.GetInt32(4), reader.GetString(5)[0], reader.GetString(6), reader.GetDateTime(7),
            reader.IsDBNull(8) ? null : new GroupBadge(reader.GetInt32(8), reader.GetString(9)));
    }

    public async Task<string> IssueSsoTokenAsync(int userId, TimeSpan ttl, CancellationToken cancellationToken = default)
    {
        string token = Convert.ToHexString(RandomNumberGenerator.GetBytes(24));

        await using MySqlConnection connection = await _connectionFactory.CreateOpenConnectionAsync(cancellationToken);
        await using var command = new MySqlCommand(
            "INSERT INTO users_token (user_id, token, expires_at) VALUES (@userId, @token, DATE_ADD(UTC_TIMESTAMP(), INTERVAL @seconds SECOND));",
            connection);
        command.Parameters.AddWithValue("@userId", userId);
        command.Parameters.AddWithValue("@token", token);
        command.Parameters.AddWithValue("@seconds", (int)ttl.TotalSeconds);

        await command.ExecuteNonQueryAsync(cancellationToken);
        return token;
    }

    public async Task UpdateFigureAsync(int userId, string figure, char gender, CancellationToken cancellationToken = default)
    {
        await using MySqlConnection connection = await _connectionFactory.CreateOpenConnectionAsync(cancellationToken);
        await using var command = new MySqlCommand(
            "UPDATE users SET figure = @figure, gender = @gender WHERE id = @id;", connection);
        command.Parameters.AddWithValue("@figure", figure);
        command.Parameters.AddWithValue("@gender", gender.ToString());
        command.Parameters.AddWithValue("@id", userId);

        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    public async Task UpdateMottoAsync(int userId, string motto, CancellationToken cancellationToken = default)
    {
        await using MySqlConnection connection = await _connectionFactory.CreateOpenConnectionAsync(cancellationToken);
        await using var command = new MySqlCommand("UPDATE users SET motto = @motto WHERE id = @id;", connection);
        command.Parameters.AddWithValue("@motto", motto);
        command.Parameters.AddWithValue("@id", userId);

        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    public async Task<string> GetPoolFigureAsync(int userId, CancellationToken cancellationToken = default)
    {
        await using MySqlConnection connection = await _connectionFactory.CreateOpenConnectionAsync(cancellationToken);
        await using var command = new MySqlCommand("SELECT pool_figure FROM users WHERE id = @id;", connection);
        command.Parameters.AddWithValue("@id", userId);

        return await command.ExecuteScalarAsync(cancellationToken) as string ?? string.Empty;
    }

    public async Task UpdatePoolFigureAsync(int userId, string poolFigure, CancellationToken cancellationToken = default)
    {
        await using MySqlConnection connection = await _connectionFactory.CreateOpenConnectionAsync(cancellationToken);
        await using var command = new MySqlCommand("UPDATE users SET pool_figure = @poolFigure WHERE id = @id;", connection);
        command.Parameters.AddWithValue("@poolFigure", poolFigure);
        command.Parameters.AddWithValue("@id", userId);

        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    public async Task<(DateTime? ClubExpiresAt, int ClubMonthsAccumulated, int ClubGiftsClaimed)> GetClubStatusAsync(
        int userId, CancellationToken cancellationToken = default)
    {
        await using MySqlConnection connection = await _connectionFactory.CreateOpenConnectionAsync(cancellationToken);
        await using var command = new MySqlCommand(
            "SELECT club_expires_at, club_months_accumulated, club_gifts_claimed FROM users WHERE id = @id;", connection);
        command.Parameters.AddWithValue("@id", userId);

        await using MySqlDataReader reader = await command.ExecuteReaderAsync(cancellationToken);
        if (!await reader.ReadAsync(cancellationToken))
        {
            return (null, 0, 0);
        }

        DateTime? clubExpiresAt = reader.IsDBNull(0) ? null : reader.GetDateTime(0);
        return (clubExpiresAt, reader.GetInt32(1), reader.GetInt32(2));
    }

    public async Task UpdateClubStatusAsync(
        int userId, DateTime clubExpiresAt, int clubMonthsAccumulated, CancellationToken cancellationToken = default)
    {
        await using MySqlConnection connection = await _connectionFactory.CreateOpenConnectionAsync(cancellationToken);
        await using var command = new MySqlCommand(
            "UPDATE users SET club_expires_at = @clubExpiresAt, club_months_accumulated = @clubMonthsAccumulated WHERE id = @id;",
            connection);
        command.Parameters.AddWithValue("@clubExpiresAt", clubExpiresAt);
        command.Parameters.AddWithValue("@clubMonthsAccumulated", clubMonthsAccumulated);
        command.Parameters.AddWithValue("@id", userId);

        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    public async Task<bool> TryClaimClubGiftAsync(int userId, CancellationToken cancellationToken = default)
    {
        await using MySqlConnection connection = await _connectionFactory.CreateOpenConnectionAsync(cancellationToken);
        await using var command = new MySqlCommand(
            """
            UPDATE users SET club_gifts_claimed = club_gifts_claimed + 1
            WHERE id = @id AND club_months_accumulated - club_gifts_claimed >= 1;
            """, connection);
        command.Parameters.AddWithValue("@id", userId);

        int affected = await command.ExecuteNonQueryAsync(cancellationToken);
        return affected > 0;
    }

    public async Task UpdateRankAsync(int userId, int rank, CancellationToken cancellationToken = default)
    {
        await using MySqlConnection connection = await _connectionFactory.CreateOpenConnectionAsync(cancellationToken);
        await using var command = new MySqlCommand("UPDATE users SET `rank` = @rank WHERE id = @id;", connection);
        command.Parameters.AddWithValue("@rank", rank);
        command.Parameters.AddWithValue("@id", userId);

        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<int>> GetUserIdsWhoseClubEndedAsync(DateTime after, DateTime upTo, CancellationToken cancellationToken = default)
    {
        await using MySqlConnection connection = await _connectionFactory.CreateOpenConnectionAsync(cancellationToken);
        await using var command = new MySqlCommand(
            "SELECT id FROM users WHERE club_expires_at > @after AND club_expires_at <= @upTo;",
            connection);
        command.Parameters.AddWithValue("@after", after);
        command.Parameters.AddWithValue("@upTo", upTo);

        var ids = new List<int>();
        await using MySqlDataReader reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            ids.Add(reader.GetInt32(0));
        }

        return ids;
    }

    public async Task<IReadOnlyDictionary<int, int>> AddOnlineSecondsAsync(
        IReadOnlyDictionary<int, int> secondsByUser, CancellationToken cancellationToken = default)
    {
        if (secondsByUser.Count == 0)
        {
            return new Dictionary<int, int>();
        }

        string[] users = [.. Enumerable.Range(0, secondsByUser.Count).Select(i => $"@u{i}")];
        string cases = string.Join(" ", users.Select((user, i) => $"WHEN {user} THEN @s{i}"));
        string ids = string.Join(", ", users);

        await using MySqlConnection connection = await _connectionFactory.CreateOpenConnectionAsync(cancellationToken);

        await using (var update = new MySqlCommand(
            $"UPDATE users SET total_online_seconds = total_online_seconds + CASE id {cases} END WHERE id IN ({ids});", connection))
        {
            BindOnlineSeconds(update, secondsByUser, withSeconds: true);
            await update.ExecuteNonQueryAsync(cancellationToken);
        }

        await using var select = new MySqlCommand($"SELECT id, total_online_seconds FROM users WHERE id IN ({ids});", connection);
        BindOnlineSeconds(select, secondsByUser, withSeconds: false);

        var totals = new Dictionary<int, int>(secondsByUser.Count);
        await using MySqlDataReader reader = await select.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            totals[reader.GetInt32(0)] = (int)Math.Min(reader.GetUInt32(1), int.MaxValue);
        }

        return totals;
    }

    private static void BindOnlineSeconds(MySqlCommand command, IReadOnlyDictionary<int, int> secondsByUser, bool withSeconds)
    {
        int index = 0;
        foreach ((int userId, int seconds) in secondsByUser)
        {
            command.Parameters.AddWithValue($"@u{index}", userId);
            if (withSeconds)
            {
                command.Parameters.AddWithValue($"@s{index}", seconds);
            }

            index++;
        }
    }

    public async Task<int> GetOnlineSecondsAsync(int userId, CancellationToken cancellationToken = default)
    {
        await using MySqlConnection connection = await _connectionFactory.CreateOpenConnectionAsync(cancellationToken);
        await using var select = new MySqlCommand("SELECT total_online_seconds FROM users WHERE id = @id;", connection);
        select.Parameters.AddWithValue("@id", userId);
        return Convert.ToInt32(await select.ExecuteScalarAsync(cancellationToken) ?? 0);
    }

    public async Task SetEmailVerifiedAsync(int userId, bool verified, CancellationToken cancellationToken = default)
    {
        await using MySqlConnection connection = await _connectionFactory.CreateOpenConnectionAsync(cancellationToken);
        await using var command = new MySqlCommand("UPDATE users SET email_verified = @verified WHERE id = @id;", connection);
        command.Parameters.AddWithValue("@verified", verified);
        command.Parameters.AddWithValue("@id", userId);

        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    public async Task<UsernameChangeStatus> GetUsernameChangeStatusAsync(int userId, CancellationToken cancellationToken = default)
    {
        await using MySqlConnection connection = await _connectionFactory.CreateOpenConnectionAsync(cancellationToken);
        await using var command = new MySqlCommand(
            "SELECT username_changed_at, username_changes, can_change_username FROM users WHERE id = @userId;", connection);
        command.Parameters.AddWithValue("@userId", userId);

        await using MySqlDataReader reader = await command.ExecuteReaderAsync(cancellationToken);
        return await reader.ReadAsync(cancellationToken)
            ? new UsernameChangeStatus(reader.IsDBNull(0) ? null : reader.GetDateTime(0), reader.GetInt32(1), reader.GetString(2) == "1")
            : new UsernameChangeStatus(null, 0, false);
    }

    public async Task<bool> TryChangeUsernameAsync(int userId, string oldUsername, string newUsername, CancellationToken cancellationToken = default)
    {
        await using MySqlConnection connection = await _connectionFactory.CreateOpenConnectionAsync(cancellationToken);
        await using MySqlTransaction transaction = await connection.BeginTransactionAsync(cancellationToken);

        try
        {
            await using (var update = new MySqlCommand(
                """
                UPDATE users
                SET username = @new, username_changed_at = UTC_TIMESTAMP(), username_changes = username_changes + 1
                WHERE id = @userId AND username = @old;
                """, connection, transaction))
            {
                update.Parameters.AddWithValue("@new", newUsername);
                update.Parameters.AddWithValue("@old", oldUsername);
                update.Parameters.AddWithValue("@userId", userId);
                if (await update.ExecuteNonQueryAsync(cancellationToken) == 0)
                {
                    await transaction.RollbackAsync(cancellationToken);
                    return false;
                }
            }

            await using (var history = new MySqlCommand(
                "INSERT INTO historical_username_changes (ref_user, old_username, new_username) VALUES (@userId, @old, @new);", connection, transaction))
            {
                history.Parameters.AddWithValue("@userId", userId);
                history.Parameters.AddWithValue("@old", oldUsername);
                history.Parameters.AddWithValue("@new", newUsername);
                await history.ExecuteNonQueryAsync(cancellationToken);
            }

            await transaction.CommitAsync(cancellationToken);
            return true;
        }
        catch (MySqlException ex) when (ex.ErrorCode == MySqlErrorCode.DuplicateKeyEntry)
        {
            await transaction.RollbackAsync(cancellationToken);
            return false;
        }
    }
}
