namespace DragonEmu.Gateway;

public sealed class GameServerOptions
{
    public const string SectionName = "GameServer";

    public string BindAddress { get; set; } = "0.0.0.0";
    public int Port { get; set; } = 30000;
    public int MaxConnections { get; set; } = 500;
    public int ReceiveBufferSize { get; set; } = 4096;
    public int MaxFrameSize { get; set; } = 64 * 1024;
    public int IdleTimeoutSeconds { get; set; } = 300;
}
