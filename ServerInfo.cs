using System.Reflection;

namespace DragonEmu;

public static class ServerInfo
{
    public const string Name = "dragonEmulator [R48]";

    public static string Version { get; } = ReadVersion();

    public static string Display => $"{Name} - version {Version}";

    public static string Build => Version[(Version.LastIndexOf('.') + 1)..];

    private static string ReadVersion()
    {
        string? informational = typeof(ServerInfo).Assembly
            .GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion;

        if (string.IsNullOrEmpty(informational))
        {
            return "unknown";
        }

        int revisionSuffix = informational.IndexOf('+');
        return revisionSuffix >= 0 ? informational[..revisionSuffix] : informational;
    }
}
