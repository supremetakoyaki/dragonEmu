using DragonEmu.Domain;

namespace DragonEmu.Protocol.Messages.Outgoing.Pets;

public sealed class PetExperienceEvent : IMessageEvent
{
    private readonly int _webId;
    private readonly int _virtualId;
    private readonly int _amount;

    public PetExperienceEvent(int webId, int virtualId, int amount)
    {
        _webId = webId;
        _virtualId = virtualId;
        _amount = amount;
    }

    public ServerPacket Compose()
    {
        var packet = new ServerPacket(OutgoingOpcodes.RoomPets.PetExperienceEvent);
        packet.AppendInt32(_webId);
        packet.AppendInt32(_virtualId);
        packet.AppendInt32(_amount);
        return packet;
    }
}
