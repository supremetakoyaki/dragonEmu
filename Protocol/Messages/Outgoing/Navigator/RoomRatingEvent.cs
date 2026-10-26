namespace DragonEmu.Protocol.Messages.Outgoing.Navigator;

public sealed class RoomRatingEvent : IMessageEvent
{
    private readonly int _score;

    public RoomRatingEvent(int score)
    {
        _score = score;
    }

    public ServerPacket Compose()
    {
        var packet = new ServerPacket(OutgoingOpcodes.Navigator.RoomRatingEvent);
        packet.AppendInt32(_score);
        return packet;
    }
}
