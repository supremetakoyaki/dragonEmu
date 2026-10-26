using DragonEmu.Domain;

namespace DragonEmu.Protocol.Messages.Outgoing.Pets;

public sealed class PetPlacingErrorEvent : IMessageEvent
{
    private readonly PetPlacingError _error;

    public PetPlacingErrorEvent(PetPlacingError error)
    {
        _error = error;
    }

    public ServerPacket Compose()
    {
        var packet = new ServerPacket(OutgoingOpcodes.RoomPets.PetPlacingErrorEvent);
        packet.AppendInt32((int)_error);
        return packet;
    }
}
