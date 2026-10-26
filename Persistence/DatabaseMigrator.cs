using System.Reflection;
using DbUp;
using DbUp.Engine;
using DbUp.Engine.Output;
using MySqlConnector;

namespace DragonEmu.Persistence;

public sealed class DatabaseMigrator
{
    private const string ScriptNamespace = "DragonEmu.Persistence.Migrations.";

    private readonly string _connectionString;

    public DatabaseMigrator(MySqlOptions options)
    {
        _connectionString = new MySqlConnectionStringBuilder
        {
            Server = options.Host,
            Port = (uint)options.Port,
            Database = options.Database,
            UserID = options.Username,
            Password = options.Password,
            AllowUserVariables = true,
        }.ConnectionString;
    }

    public IReadOnlyList<string> Pending() =>
        Upgrader(new NoOpUpgradeLog()).GetScriptsToExecute().Select(script => ShortName(script.Name)).ToList();

    public bool Migrate(IUpgradeLog log)
    {
        EnsureDatabase.For.MySqlDatabase(_connectionString, log);
        DatabaseUpgradeResult result = Upgrader(log).PerformUpgrade();
        return result.Successful;
    }

    public void MarkApplied(IUpgradeLog log) => Upgrader(log).MarkAsExecuted();

    private UpgradeEngine Upgrader(IUpgradeLog log) =>
        DeployChanges.To
            .MySqlDatabase(_connectionString)
            .WithScriptsEmbeddedInAssembly(Assembly.GetExecutingAssembly(), name => name.StartsWith(ScriptNamespace, StringComparison.Ordinal))
            .WithVariablesDisabled()
            .LogTo(log)
            .Build();

    private static string ShortName(string resourceName) =>
        resourceName.StartsWith(ScriptNamespace, StringComparison.Ordinal) ? resourceName[ScriptNamespace.Length..] : resourceName;
}
