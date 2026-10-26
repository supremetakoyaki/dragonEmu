using DragonEmu.Domain;

namespace DragonEmu.Protocol.Messages.Outgoing.Pets;

public sealed class PetInfoMessageEvent : IMessageEvent
{
    private readonly int _webId;
    private readonly string _name;
    private readonly PetStats _stats;
    private readonly int _maxLevel;
    private readonly PetLevel _level;
    private readonly string _figure;
    private readonly int _ownerId;
    private readonly int _ageDays;
    private readonly string _ownerName;

    public PetInfoMessageEvent(int webId, string name, PetStats stats, int maxLevel, PetLevel level, string figure, int ownerId, int ageDays, string ownerName)
    {
        _webId = webId;
        _name = name;
        _stats = stats;
        _maxLevel = maxLevel;
        _level = level;
        _figure = figure;
        _ownerId = ownerId;
        _ageDays = ageDays;
        _ownerName = ownerName;
    }

    public ServerPacket Compose()
    {
        var packet = new ServerPacket(OutgoingOpcodes.RoomPets.PetInfoMessageEvent_601);
        packet.AppendInt32(_webId);
        packet.AppendString(_name);
        packet.AppendInt32(_stats.Level);
        packet.AppendInt32(_maxLevel);
        packet.AppendInt32(_stats.Experience);
        packet.AppendInt32(_level.ExperienceCap);
        packet.AppendInt32(_stats.Energy);
        packet.AppendInt32(_level.EnergyCap);
        packet.AppendInt32(_stats.Nutrition);
        packet.AppendInt32(_level.NutritionCap);
        packet.AppendString(_figure);
        packet.AppendInt32(_stats.Respect);
        packet.AppendInt32(_ownerId);
        packet.AppendInt32(_ageDays);
        packet.AppendString(_ownerName);
        return packet;
    }
}
