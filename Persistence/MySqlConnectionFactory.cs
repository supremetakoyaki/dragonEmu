using Microsoft.Extensions.Options;
using MySqlConnector;

namespace DragonEmu.Persistence;

public sealed class MySqlConnectionFactory : IDbConnectionFactory
{
    private readonly string _connectionString;

    public MySqlConnectionFactory(IOptions<MySqlOptions> options)
    {
        MySqlOptions o = options.Value;

        _connectionString = new MySqlConnectionStringBuilder
        {
            Server = o.Host,
            Port = (uint)o.Port,
            Database = o.Database,
            UserID = o.Username,
            Password = o.Password,
            Pooling = true,
            MinimumPoolSize = (uint)o.MinPoolSize,
            MaximumPoolSize = (uint)o.MaxPoolSize,
        }.ConnectionString;
    }

    public async Task<MySqlConnection> CreateOpenConnectionAsync(CancellationToken cancellationToken = default)
    {
        var connection = new MySqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);
        return connection;
    }
}
