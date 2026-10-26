using DragonEmu.Domain;

namespace DragonEmu.Protocol.Messages.Outgoing.Pets;

public sealed class PetLevelNotificationEvent : IMessageEvent
{
    private readonly int _petId;
    private readonly string _name;
    private readonly int _level;
    private readonly string _figure;

    public PetLevelNotificationEvent(int petId, string name, int level, string figure)
    {
        _petId = petId;
        _name = name;
        _level = level;
        _figure = figure;
    }

    public ServerPacket Compose()
    {
        var packet = new ServerPacket(OutgoingOpcodes.Notifications.PetLevelNotificationEvent);
        packet.AppendInt32(_petId);
        packet.AppendString(_name);
        packet.AppendInt32(_level);
        packet.AppendString(_figure);
        packet.AppendInt32(0);
        return packet;
    }
}
