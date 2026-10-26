using DragonEmu.Game.Rooms;
using DragonEmu.Protocol.Messages.Outgoing.RoomEngine;

namespace DragonEmu.Protocol.Messages.Incoming.RoomEngine;

public sealed class MoveAvatarMessageComposer : IMessageComposer
{
    private readonly IRoomManager _roomManager;
    private readonly RoomMovementScheduler _movementScheduler;
    private readonly RoomBroadcaster _roomBroadcaster;

    private int _targetX;
    private int _targetY;

    public MoveAvatarMessageComposer(IRoomManager roomManager, RoomMovementScheduler movementScheduler, RoomBroadcaster roomBroadcaster)
    {
        _roomManager = roomManager;
        _movementScheduler = movementScheduler;
        _roomBroadcaster = roomBroadcaster;
    }

    public void Parse(ClientPacket packet)
    {
        _targetX = packet.PopInt32();
        _targetY = packet.PopInt32();
    }

    public async ValueTask HandleAsync(IGameSession session, CancellationToken cancellationToken)
    {
        if (session.AuthenticatedUser is null || session.CurrentRoomId is not { } roomId)
        {
            return;
        }

        Room? room = await _roomManager.GetOrLoadRoomAsync(roomId, cancellationToken);
        if (room is null || !room.TryGetActor(session.Id, out RoomActor? actor))
        {
            return;
        }

        lock (actor!.Lock)
        {
            if (actor.IsHeldByFurniture || actor.IsKicked)
            {
                return;
            }
        }

        if (session.TeleportMode)
        {
            if (room.TeleportActor(actor!, _targetX, _targetY, session.TeleportHeight))
            {
                _roomBroadcaster.Broadcast(room, new UserUpdateMessageEvent([actor!]).Compose());
            }

            return;
        }

        (int goalX, int goalY) = room.RedirectToBedLayTile(_targetX, _targetY, session.Id) is { } layTile
            ? (layTile.X, layTile.Y)
            : (_targetX, _targetY);

        if (room.TryStartWalk(actor!, goalX, goalY))
        {
            _movementScheduler.MarkActive(room);
        }
    }
}
