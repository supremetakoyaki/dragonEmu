using DragonEmu.Domain;
using DragonEmu.Game.Rooms;
using DragonEmu.Gateway;
using DragonEmu.Persistence;
using DragonEmu.Protocol.Messages.Outgoing.RoomPermissions;
using DragonEmu.Protocol.Messages.Outgoing.RoomSettings;

namespace DragonEmu.Protocol.Messages.Incoming.RoomAction;

public sealed class RemoveAllRightsMessageComposer : IMessageComposer
{
    private readonly IRoomManager _roomManager;
    private readonly RoomRightsService _roomRights;
    private readonly IRoomRightsRepository _roomRightsRepository;
    private readonly RoomBroadcaster _roomBroadcaster;
    private readonly ClientSessionManager _sessionManager;

    public RemoveAllRightsMessageComposer(
        IRoomManager roomManager, RoomRightsService roomRights, IRoomRightsRepository roomRightsRepository,
        RoomBroadcaster roomBroadcaster, ClientSessionManager sessionManager)
    {
        _roomManager = roomManager;
        _roomRights = roomRights;
        _roomRightsRepository = roomRightsRepository;
        _roomBroadcaster = roomBroadcaster;
        _sessionManager = sessionManager;
    }

    public void Parse(ClientPacket packet)
    {
    }

    public async ValueTask HandleAsync(IGameSession session, CancellationToken cancellationToken)
    {
        if (session.AuthenticatedUser is not { } account || session.CurrentRoomId is not { } roomId)
        {
            return;
        }

        if (!_roomManager.TryGetLoadedRoom(roomId, out Room? room) || !await _roomRights.CanEditAsync(account, room!.Data, cancellationToken))
        {
            return;
        }

        IReadOnlyList<int> revokedUserIds = await _roomRightsRepository.RevokeAllAsync(roomId, cancellationToken);

        foreach (int targetUserId in revokedUserIds)
        {
            _roomBroadcaster.Broadcast(room, new FlatControllerRemovedEvent(roomId, targetUserId).Compose());

            RoomActor? target = room.Actors.FirstOrDefault(a => a.Type == ActorType.Player && a.AccountId == targetUserId);
            if (target is null)
            {
                continue;
            }

            FlatControlStatus.Show(room, target, hasRights: false, _roomBroadcaster);
            if (_sessionManager.TryGet(target.SessionId, out ClientSession? targetSession))
            {
                targetSession!.EnqueueSend(new YouAreNotControllerMessageEvent().Compose());
            }
        }
    }
}
