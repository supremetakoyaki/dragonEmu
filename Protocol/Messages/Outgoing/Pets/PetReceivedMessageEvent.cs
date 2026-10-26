using DragonEmu.Domain;

namespace DragonEmu.Protocol.Messages.Outgoing.Pets;

public sealed class PetReceivedMessageEvent : IMessageEvent
{
    private readonly PetRecord _pet;

    public PetReceivedMessageEvent(PetRecord pet)
    {
        _pet = pet;
    }

    public ServerPacket Compose()
    {
        var packet = new ServerPacket(OutgoingOpcodes.InventoryPets.PetReceivedMessageEvent);
        packet.AppendBoolean(false);
        PetData.Append(packet, _pet);
        return packet;
    }
}
