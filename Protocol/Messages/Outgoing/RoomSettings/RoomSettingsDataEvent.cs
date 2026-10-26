using DragonEmu.Domain;

namespace DragonEmu.Protocol.Messages.Outgoing.RoomSettings;

public sealed class RoomSettingsDataEvent : IMessageEvent
{
    private const int MaxVisitorsCeiling = 25;

    private readonly int _roomId;
    private readonly string _name;
    private readonly string _description;
    private readonly int _accessType;
    private readonly int _categoryId;
    private readonly int _maxUsers;
    private readonly IReadOnlyList<string> _tags;
    private readonly IReadOnlyList<RoomRightsHolder> _holders;
    private readonly int _totalHolderCount;
    private readonly bool _allowPets;
    private readonly bool _allowPetsEat;
    private readonly bool _allowWalkthrough;

    public RoomSettingsDataEvent(
        int roomId, string name, string description, int accessType, int categoryId, int maxUsers,
        IReadOnlyList<string> tags, IReadOnlyList<RoomRightsHolder> holders, int totalHolderCount,
        bool allowPets, bool allowPetsEat, bool allowWalkthrough)
    {
        _roomId = roomId;
        _name = name;
        _description = description;
        _accessType = accessType;
        _categoryId = categoryId;
        _maxUsers = maxUsers;
        _tags = tags;
        _holders = holders;
        _totalHolderCount = totalHolderCount;
        _allowPets = allowPets;
        _allowPetsEat = allowPetsEat;
        _allowWalkthrough = allowWalkthrough;
    }

    public ServerPacket Compose()
    {
        var packet = new ServerPacket(OutgoingOpcodes.RoomSettings.RoomSettingsDataEvent);
        packet.AppendInt32(_roomId);
        packet.AppendString(_name);
        packet.AppendString(_description);
        packet.AppendInt32(_accessType);
        packet.AppendInt32(_categoryId);
        packet.AppendInt32(_maxUsers);
        packet.AppendInt32(MaxVisitorsCeiling);

        packet.AppendInt32(_tags.Count);
        foreach (string tag in _tags)
        {
            packet.AppendString(tag);
        }

        packet.AppendInt32(_holders.Count);
        foreach (RoomRightsHolder holder in _holders)
        {
            packet.AppendInt32(holder.UserId);
            packet.AppendString(holder.Username);
        }

        packet.AppendInt32(_totalHolderCount);
        packet.AppendInt32(_allowPets ? 1 : 0);
        packet.AppendInt32(_allowPetsEat ? 1 : 0);
        packet.AppendInt32(_allowWalkthrough ? 1 : 0);

        return packet;
    }
}
