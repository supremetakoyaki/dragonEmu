namespace DragonEmu.Infrastructure;

public static class DragonSettingsFile
{
    public const string FileName = "dragon_settings.json";

    public static string Path => System.IO.Path.Combine(AppContext.BaseDirectory, FileName);

    public static bool EnsureExists(string path)
    {
        if (File.Exists(path))
        {
            return true;
        }

        File.WriteAllText(path, Template);
        return false;
    }

    public const string Template =
        """
        {
          "GameServer": {
            "BindAddress": "0.0.0.0",
            "Port": 30000,
            "MaxConnections": 500,
            "IdleTimeoutSeconds": 300,
            "ReceiveBufferSize": 4096,
            "MaxFrameSize": 65536
          },

          "MySql": {
            "Host": "localhost",
            "Port": 3306,
            "Database": "dragon48",
            "Username": "root",
            "Password": "",
            "MinPoolSize": 5,
            "MaxPoolSize": 30
          },

          "Logging": {
            "LogLevel": {
              "Default": "Debug",
              "Microsoft": "Information"
            }
          }
        }

        """;
}
