namespace DragonEmu.Protocol.Messages.Outgoing.Trax;

public sealed class SongLockedMessageEvent : IMessageEvent
{
    public static SongLockedMessageEvent Instance { get; } = new();

    public ServerPacket Compose() => new(OutgoingOpcodes.Trax.SongLockedMessageEvent);
}
