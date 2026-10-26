using DragonEmu.Domain;

namespace DragonEmu.Protocol.Messages.Outgoing.Pets;

public sealed class PetAddedToInventoryEvent : IMessageEvent
{
    private readonly PetRecord _pet;

    public PetAddedToInventoryEvent(PetRecord pet)
    {
        _pet = pet;
    }

    public ServerPacket Compose()
    {
        var packet = new ServerPacket(OutgoingOpcodes.InventoryPets.PetAddedToInventoryEvent);
        PetData.Append(packet, _pet);
        packet.AppendBoolean(false);
        return packet;
    }
}
