namespace DragonEmu.Protocol.Messages.Outgoing.Recycler;

public sealed class RecyclerFinishedMessageEvent : IMessageEvent
{
    private const int SuccessStatus = 1;
    private const int FailureStatus = 2;

    private readonly int _status;
    private readonly int _prizeBoxItemId;

    private RecyclerFinishedMessageEvent(int status, int prizeBoxItemId)
    {
        _status = status;
        _prizeBoxItemId = prizeBoxItemId;
    }

    public static RecyclerFinishedMessageEvent Success(int prizeBoxItemId) => new(SuccessStatus, prizeBoxItemId);

    public static RecyclerFinishedMessageEvent Failure() => new(FailureStatus, 0);

    public ServerPacket Compose()
    {
        var packet = new ServerPacket(OutgoingOpcodes.Recycler.RecyclerFinishedMessageEvent);
        packet.AppendInt32(_status);
        packet.AppendInt32(_prizeBoxItemId);
        return packet;
    }
}
