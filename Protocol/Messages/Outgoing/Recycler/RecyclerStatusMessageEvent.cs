namespace DragonEmu.Protocol.Messages.Outgoing.Recycler;

public sealed class RecyclerStatusMessageEvent : IMessageEvent
{
    private const int ReadyStatus = 1;
    private const int ClosedStatus = 2;
    private const int CooldownStatus = 3;

    private readonly int _status;
    private readonly int _seconds;

    private RecyclerStatusMessageEvent(int status, int seconds)
    {
        _status = status;
        _seconds = seconds;
    }

    public static RecyclerStatusMessageEvent Ready() => new(ReadyStatus, 0);

    public static RecyclerStatusMessageEvent Closed() => new(ClosedStatus, 0);

    public static RecyclerStatusMessageEvent Cooldown(int secondsLeft) => new(CooldownStatus, secondsLeft);

    public ServerPacket Compose()
    {
        var packet = new ServerPacket(OutgoingOpcodes.Recycler.RecyclerStatusMessageEvent);
        packet.AppendInt32(_status);
        packet.AppendInt32(_seconds);
        return packet;
    }
}
