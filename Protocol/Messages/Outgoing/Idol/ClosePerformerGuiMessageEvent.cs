namespace DragonEmu.Protocol.Messages.Outgoing.Idol;

public sealed class ClosePerformerGuiMessageEvent : IMessageEvent
{
    public ServerPacket Compose() => new(OutgoingOpcodes.Idol.ClosePerformerGuiMessageEvent);
}
