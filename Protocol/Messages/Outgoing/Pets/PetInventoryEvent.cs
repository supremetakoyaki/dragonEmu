using DragonEmu.Domain;

namespace DragonEmu.Protocol.Messages.Outgoing.Pets;

public sealed class PetInventoryEvent : IMessageEvent
{
    private readonly IReadOnlyList<PetRecord> _pets;

    public PetInventoryEvent(IReadOnlyList<PetRecord> pets)
    {
        _pets = pets;
    }

    public ServerPacket Compose()
    {
        var packet = new ServerPacket(OutgoingOpcodes.InventoryPets.PetInventoryEvent);
        packet.AppendInt32(_pets.Count);
        foreach (PetRecord pet in _pets)
        {
            PetData.Append(packet, pet);
        }

        return packet;
    }
}
