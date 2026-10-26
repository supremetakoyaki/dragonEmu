using DragonEmu.Game.Rooms;
using DragonEmu.Game.Rooms.Pathfinding;
using DragonEmu.Protocol.Messages.Outgoing.RoomEngine;

namespace DragonEmu.Protocol.Messages.Incoming.RoomAvatar;

public sealed class LookToMessageComposer : IMessageComposer
{
    private readonly IRoomManager _roomManager;
    private readonly RoomBroadcaster _roomBroadcaster;

    private int _targetX;
    private int _targetY;

    public LookToMessageComposer(IRoomManager roomManager, RoomBroadcaster roomBroadcaster)
    {
        _roomManager = roomManager;
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
            if (actor.IsWalking || actor.Posture != ActorPosture.Stand || (_targetX == actor.X && _targetY == actor.Y))
            {
                return;
            }

            int rotation = RotationHelper.Calculate(actor.X, actor.Y, _targetX, _targetY);
            actor.RotBody = rotation;
            actor.RotHead = rotation;
        }

        _roomBroadcaster.Broadcast(room, new UserUpdateMessageEvent([actor!]).Compose());
    }
}
