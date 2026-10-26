namespace DragonEmu.Protocol.Messages.Outgoing.Availability;

public sealed class AvailabilityStatusMessageEvent : IMessageEvent
{
    private readonly bool _open;
    private readonly bool _onShutdown;

    public AvailabilityStatusMessageEvent(bool open = true, bool onShutdown = false)
    {
        _open = open;
        _onShutdown = onShutdown;
    }

    public ServerPacket Compose()
    {
        var packet = new ServerPacket(OutgoingOpcodes.Availability.AvailabilityStatusMessageEvent);
        packet.AppendBoolean(_open);
        packet.AppendBoolean(_onShutdown);
        return packet;
    }
}
