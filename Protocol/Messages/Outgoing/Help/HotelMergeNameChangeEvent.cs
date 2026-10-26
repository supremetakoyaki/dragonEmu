namespace DragonEmu.Protocol.Messages.Outgoing.Help;

public sealed class HotelMergeNameChangeEvent : IMessageEvent
{
    public ServerPacket Compose() => new(OutgoingOpcodes.Help.HotelMergeNameChangeEvent);
}
