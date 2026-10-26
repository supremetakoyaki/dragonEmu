namespace DragonEmu.Persistence;

public sealed class MySqlOptions
{
    public const string SectionName = "MySql";

    public string Host { get; set; } = "localhost";
    public int Port { get; set; } = 3306;
    public string Database { get; set; } = "";
    public string Username { get; set; } = "";
    public string Password { get; set; } = "";
    public int MinPoolSize { get; set; } = 5;
    public int MaxPoolSize { get; set; } = 30;
}
