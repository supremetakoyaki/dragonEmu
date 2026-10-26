using DragonEmu.Domain;

namespace DragonEmu.Protocol.Messages.Outgoing.Pets;

public sealed class PetRemovedFromInventoryEvent : IMessageEvent
{
    private readonly int _petId;

    public PetRemovedFromInventoryEvent(int petId)
    {
        _petId = petId;
    }

    public ServerPacket Compose()
    {
        var packet = new ServerPacket(OutgoingOpcodes.InventoryPets.PetRemovedFromInventoryEvent);
        packet.AppendInt32(_petId);
        return packet;
    }
}
