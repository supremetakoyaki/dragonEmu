namespace DragonEmu.Protocol.Messages.Outgoing.Trax;

public sealed class InvalidSongNameMessageEvent : IMessageEvent
{
    public static InvalidSongNameMessageEvent Instance { get; } = new();

    public ServerPacket Compose() => new(OutgoingOpcodes.Trax.InvalidSongNameMessageEvent);
}
