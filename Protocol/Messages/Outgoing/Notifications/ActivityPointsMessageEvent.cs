namespace DragonEmu.Protocol.Messages.Outgoing.Notifications;

public sealed class ActivityPointsMessageEvent : IMessageEvent
{
    private readonly IReadOnlyList<(int Type, long Amount)> _balances;

    public ActivityPointsMessageEvent(IReadOnlyList<(int Type, long Amount)> balances)
    {
        _balances = balances;
    }

    public ServerPacket Compose()
    {
        var packet = new ServerPacket(OutgoingOpcodes.Notifications.ActivityPointsMessageEvent);
        packet.AppendInt32(_balances.Count);
        foreach ((int type, long amount) in _balances)
        {
            packet.AppendInt32(type);
            packet.AppendInt32((int)amount);
        }

        return packet;
    }
}
