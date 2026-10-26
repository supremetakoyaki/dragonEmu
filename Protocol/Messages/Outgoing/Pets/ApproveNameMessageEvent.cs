using DragonEmu.Domain;

namespace DragonEmu.Protocol.Messages.Outgoing.Pets;

public sealed class ApproveNameMessageEvent : IMessageEvent
{
    private readonly PetNameCheck _result;

    public ApproveNameMessageEvent(PetNameCheck result)
    {
        _result = result;
    }

    public ServerPacket Compose()
    {
        var packet = new ServerPacket(OutgoingOpcodes.Users.ApproveNameMessageEvent);
        packet.AppendInt32((int)_result);
        return packet;
    }
}
