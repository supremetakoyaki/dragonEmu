using DragonEmu.Domain;

namespace DragonEmu.Protocol.Messages.Outgoing.Pets;

public sealed class PetRespectFailedEvent : IMessageEvent
{
    private readonly int _requiredDays;
    private readonly int _accountAgeDays;

    public PetRespectFailedEvent(int requiredDays, int accountAgeDays)
    {
        _requiredDays = requiredDays;
        _accountAgeDays = accountAgeDays;
    }

    public ServerPacket Compose()
    {
        var packet = new ServerPacket(OutgoingOpcodes.RoomPets.PetRespectFailedEvent);
        packet.AppendInt32(_requiredDays);
        packet.AppendInt32(_accountAgeDays);
        return packet;
    }
}
