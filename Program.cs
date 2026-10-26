using DragonEmu.Infrastructure;
using DragonEmu.Logging;
using DragonEmu.Persistence;
using DbUp.Engine.Output;

ConsoleBanner.Print();

// settings file
string settingsPath = DragonSettingsFile.Path;
if (!DragonSettingsFile.EnsureExists(settingsPath))
{
    Console.ForegroundColor = ConsoleColor.Yellow;
    Console.WriteLine($"{DragonSettingsFile.FileName} was created at:");
    Console.WriteLine($"  {settingsPath}");
    Console.WriteLine("Please fill in this settings file and restart the emulator.");
    Console.ResetColor();
    if (!Console.IsInputRedirected)
    {
        Console.WriteLine("Press any key to exit.");
        Console.ReadKey(intercept: true);
    }

    return 1;
}

// DB migration
bool migrate = args.Contains("--migrate");
bool markApplied = args.Contains("--mark-applied");
string[] settingArgs = args.Where(arg => arg is not "--migrate" and not "--mark-applied").ToArray();

var builder = Host.CreateApplicationBuilder(settingArgs);

builder.Configuration.AddJsonFile(settingsPath, optional: false, reloadOnChange: false);
builder.Configuration.AddEnvironmentVariables();
builder.Configuration.AddCommandLine(settingArgs);

var migrator = new DatabaseMigrator(builder.Configuration.GetSection(MySqlOptions.SectionName).Get<MySqlOptions>() ?? new MySqlOptions());
if (migrate)
{
    if (markApplied)
    {
        migrator.MarkApplied(new ConsoleUpgradeLog());
        Console.WriteLine("Migration wasn't run since it was marked as applied.");
        return 0;
    }

    return migrator.Migrate(new ConsoleUpgradeLog()) ? 0 : 1;
}

try
{
    IReadOnlyList<string> pending = migrator.Pending();
    if (pending.Count > 0)
    {
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine($"The database is behind. {pending.Count} migration(s) not applied: {string.Join(", ", pending)}.");
        Console.WriteLine("Run the emulator with --migrate to apply them, or run with --migrate --mark-applied if you have them.");
        Console.ResetColor();
        return 1;
    }
}
catch (Exception ex) when (ex is MySqlConnector.MySqlException or InvalidOperationException)
{
    Console.ForegroundColor = ConsoleColor.Red;
    Console.WriteLine($"Can't check the database's migrations: {ex.Message}");
    Console.ResetColor();
    return 1;
}

builder.AddDragonConsoleLogging();

builder.ConfigureContainer(new DefaultServiceProviderFactory(new ServiceProviderOptions { ValidateOnBuild = true, ValidateScopes = true }));

builder.Services
    .AddPersistence(builder.Configuration)
    .AddSettingsAndCaches()
    .AddHistory()
    .AddUsersAndRewards()
    .AddFurniture()
    .AddRooms()
    .AddGateway(builder.Configuration);

IHost host = builder.Build();

DragonEmu.Gateway.GameMessageDispatcher.EnsureEveryHandlerCanBeMade(host.Services);
DragonEmu.Game.Chat.Commands.ChatCommandDispatcher.EnsureEveryCommandCanBeMade(host.Services);

host.Run();
return 0;
