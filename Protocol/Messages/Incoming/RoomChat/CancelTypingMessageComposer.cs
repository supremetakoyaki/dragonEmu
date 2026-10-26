using DragonEmu.Game.Rooms;
using DragonEmu.Protocol.Messages.Outgoing.RoomChat;

namespace DragonEmu.Protocol.Messages.Incoming.RoomChat;

public sealed class CancelTypingMessageComposer : IMessageComposer
{
    private readonly IRoomManager _roomManager;
    private readonly RoomBroadcaster _broadcaster;

    public CancelTypingMessageComposer(IRoomManager roomManager, RoomBroadcaster broadcaster)
    {
        _roomManager = roomManager;
        _broadcaster = broadcaster;
    }

    public void Parse(ClientPacket packet)
    {
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

        _broadcaster.BroadcastFrom(room, new UserTypingMessageEvent(actor!.VirtualId, false).Compose(), actor.AccountId);
    }
}
