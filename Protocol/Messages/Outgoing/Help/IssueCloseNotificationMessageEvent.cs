using DragonEmu.Domain;

namespace DragonEmu.Protocol.Messages.Outgoing.Help;

public sealed class IssueCloseNotificationMessageEvent : IMessageEvent
{
    private readonly IssueResolution _resolution;

    public IssueCloseNotificationMessageEvent(IssueResolution resolution)
    {
        _resolution = resolution;
    }

    public ServerPacket Compose()
    {
        var packet = new ServerPacket(OutgoingOpcodes.Help.IssueCloseNotificationMessageEvent);
        packet.AppendInt32((int)_resolution);
        return packet;
    }
}
