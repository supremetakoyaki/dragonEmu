using DragonEmu.Domain;

namespace DragonEmu.Protocol.Messages.Outgoing.Polls;

public sealed class PollOfferEvent : IMessageEvent
{
    private readonly int _pollId;
    private readonly string _headline;

    public PollOfferEvent(int pollId, string headline)
    {
        _pollId = pollId;
        _headline = headline;
    }

    public ServerPacket Compose()
    {
        var packet = new ServerPacket(OutgoingOpcodes.Poll.PollOfferEvent);
        packet.AppendInt32(_pollId);
        packet.AppendString(_headline);
        return packet;
    }
}
