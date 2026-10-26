using DragonEmu.Game.Moderation;
using DragonEmu.Game.Rooms;
using DragonEmu.Protocol.Messages.Outgoing.RoomChat;

namespace DragonEmu.Protocol.Messages.Incoming.RoomChat;

public sealed class StartTypingMessageComposer : IMessageComposer
{
    private readonly IRoomManager _roomManager;
    private readonly RoomBroadcaster _broadcaster;
    private readonly ModerationService _moderation;

    public StartTypingMessageComposer(IRoomManager roomManager, RoomBroadcaster broadcaster, ModerationService moderation)
    {
        _roomManager = roomManager;
        _broadcaster = broadcaster;
        _moderation = moderation;
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

        Room? room = await _roomManager.GetOrLoadRoomAsync(roomId, cancellationToken);
        if (room is null || !room.TryGetActor(session.Id, out RoomActor? actor)
            || (await _moderation.WhyCantChatAsync(session, account, room, cancellationToken)).Blocked)
        {
            return;
        }

        _broadcaster.BroadcastFrom(room, new UserTypingMessageEvent(actor!.VirtualId, true).Compose(), actor.AccountId);
    }
}
