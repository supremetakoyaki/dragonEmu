namespace DragonEmu.Protocol.Messages.Outgoing.Trax;

public sealed class InvalidSongLengthMessageEvent : IMessageEvent
{
    public static InvalidSongLengthMessageEvent Instance { get; } = new();

    public ServerPacket Compose() => new(OutgoingOpcodes.Trax.InvalidSongLengthMessageEvent);
}
