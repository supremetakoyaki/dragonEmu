using DragonEmu.Domain;
using DragonEmu.Game.Rooms;
using DragonEmu.Persistence;
using DragonEmu.Protocol.Messages.Outgoing.Users;

namespace DragonEmu.Protocol.Messages.Incoming.InventoryBadges;

public sealed class SetActivatedBadgesComposer : IMessageComposer
{
    private const int FirstSlot = 1;
    private const int LastSlot = 5;

    private readonly IUserBadgeRepository _userBadgeRepository;
    private readonly IRoomManager _roomManager;
    private readonly RoomBroadcaster _roomBroadcaster;

    private readonly List<(int Slot, string BadgeCode)> _worn = [];

    public SetActivatedBadgesComposer(IUserBadgeRepository userBadgeRepository, IRoomManager roomManager, RoomBroadcaster roomBroadcaster)
    {
        _userBadgeRepository = userBadgeRepository;
        _roomManager = roomManager;
        _roomBroadcaster = roomBroadcaster;
    }

    public void Parse(ClientPacket packet)
    {
        while (packet.RemainingLength > 0)
        {
            int slot = packet.PopInt32();
            string badgeCode = packet.PopString();
            if (badgeCode.Length > 0)
            {
                _worn.Add((slot, badgeCode));
            }
        }
    }

    public async ValueTask HandleAsync(IGameSession session, CancellationToken cancellationToken)
    {
        if (session.AuthenticatedUser is not { } account)
        {
            return;
        }

        IReadOnlyList<UserBadge> owned = await _userBadgeRepository.GetAllAsync(account.Id, cancellationToken);
        HashSet<string> ownedCodes = owned.Select(b => b.BadgeCode).ToHashSet();

        bool valid = _worn.All(badge => badge.Slot is >= FirstSlot and <= LastSlot && ownedCodes.Contains(badge.BadgeCode))
            && _worn.Select(badge => badge.Slot).Distinct().Count() == _worn.Count
            && _worn.Select(badge => badge.BadgeCode).Distinct().Count() == _worn.Count;
        if (!valid)
        {
            return;
        }

        List<(int Slot, string BadgeCode)> worn = _worn.OrderBy(badge => badge.Slot).ToList();
        await _userBadgeRepository.SetWornAsync(account.Id, worn, cancellationToken);

        ServerPacket badges = new HabboUserBadgesMessageEvent(account.Id, worn).Compose();
        if (session.CurrentRoomId is { } roomId && _roomManager.TryGetLoadedRoom(roomId, out Room? room))
        {
            _roomBroadcaster.Broadcast(room!, badges);
        }
        else
        {
            session.Send(badges);
        }
    }
}
