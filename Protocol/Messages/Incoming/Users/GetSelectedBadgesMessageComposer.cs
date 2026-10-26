using DragonEmu.Game.Rooms;
using DragonEmu.Persistence;
using DragonEmu.Protocol.Messages.Outgoing.Users;

namespace DragonEmu.Protocol.Messages.Incoming.Users;

public sealed class GetSelectedBadgesMessageComposer : IMessageComposer
{
    private readonly IRoomManager _roomManager;
    private readonly IUserBadgeRepository _userBadgeRepository;

    private int _targetUserId;

    public GetSelectedBadgesMessageComposer(IRoomManager roomManager, IUserBadgeRepository userBadgeRepository)
    {
        _roomManager = roomManager;
        _userBadgeRepository = userBadgeRepository;
    }

    public void Parse(ClientPacket packet)
    {
        _targetUserId = packet.PopInt32();
    }

    public async ValueTask HandleAsync(IGameSession session, CancellationToken cancellationToken)
    {
        if (session.AuthenticatedUser is null || session.CurrentRoomId is not { } roomId
            || !_roomManager.TryGetLoadedRoom(roomId, out Room? room))
        {
            return;
        }

        RoomActor? target = room!.Actors.FirstOrDefault(a => a.Type == ActorType.Player && a.AccountId == _targetUserId);
        if (target is null)
        {
            return;
        }

        IReadOnlyList<(int Slot, string BadgeCode)> worn = await _userBadgeRepository.GetWornAsync(_targetUserId, cancellationToken);
        session.Send(new HabboUserBadgesMessageEvent(_targetUserId, worn).Compose());
    }
}
