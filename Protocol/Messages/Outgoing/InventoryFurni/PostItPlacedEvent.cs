namespace DragonEmu.Protocol.Messages.Outgoing.InventoryFurni;

public sealed class PostItPlacedEvent : IMessageEvent
{
    private readonly int _padId;
    private readonly int _notesLeft;

    public PostItPlacedEvent(int padId, int notesLeft)
    {
        _padId = padId;
        _notesLeft = notesLeft;
    }

    public ServerPacket Compose()
    {
        var packet = new ServerPacket(OutgoingOpcodes.InventoryFurni.PostItPlacedEvent);
        packet.AppendInt32(_padId);
        packet.AppendInt32(_notesLeft);
        return packet;
    }
}
