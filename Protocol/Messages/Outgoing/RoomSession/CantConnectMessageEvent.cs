namespace DragonEmu.Protocol.Messages.Outgoing.RoomSession;

public sealed class CantConnectMessageEvent : IMessageEvent
{
    public const int Banned = 4;

    private const int ReasonQueueError = 3;
    private const string ClubOnlyParameter = "c";

    private readonly int _reason;
    private readonly string _parameter;

    public CantConnectMessageEvent(int reason, string parameter = "")
    {
        _reason = reason;
        _parameter = parameter;
    }

    public static CantConnectMessageEvent ClubOnly() => new(ReasonQueueError, ClubOnlyParameter);

    public ServerPacket Compose()
    {
        var packet = new ServerPacket(OutgoingOpcodes.RoomSession.CantConnectMessageEvent);
        packet.AppendInt32(_reason);
        if (_reason == ReasonQueueError)
        {
            packet.AppendString(_parameter);
        }

        return packet;
    }
}
