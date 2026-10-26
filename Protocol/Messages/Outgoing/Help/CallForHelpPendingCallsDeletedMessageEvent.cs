using DragonEmu.Domain;

namespace DragonEmu.Protocol.Messages.Outgoing.Help;

public sealed class CallForHelpPendingCallsDeletedMessageEvent : IMessageEvent
{
    public ServerPacket Compose() => new(OutgoingOpcodes.Help.CallForHelpPendingCallsDeletedMessageEvent);
}
