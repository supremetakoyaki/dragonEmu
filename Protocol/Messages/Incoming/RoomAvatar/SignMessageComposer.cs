using DragonEmu.Game.Rooms;
using DragonEmu.Protocol.Messages.Outgoing.RoomEngine;

namespace DragonEmu.Protocol.Messages.Incoming.RoomAvatar;

public sealed class SignMessageComposer : IMessageComposer
{
    private const int MaxSignId = 14;

    private readonly IRoomManager _roomManager;
    private readonly RoomBroadcaster _broadcaster;

    private int _signId;

    public SignMessageComposer(IRoomManager roomManager, RoomBroadcaster broadcaster)
    {
        _roomManager = roomManager;
        _broadcaster = broadcaster;
    }

    public void Parse(ClientPacket packet)
    {
        _signId = packet.PopInt32();
    }

    public async ValueTask HandleAsync(IGameSession session, CancellationToken cancellationToken)
    {
        if (_signId is < 0 or > MaxSignId)
        {
            return;
        }

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
            actor.PendingSignId = _signId;
        }

        _broadcaster.Broadcast(room, new UserUpdateMessageEvent([actor]).Compose());
    }
}
