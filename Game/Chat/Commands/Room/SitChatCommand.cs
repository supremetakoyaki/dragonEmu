using DragonEmu.Game.Rooms;
using DragonEmu.Protocol.Messages;
using DragonEmu.Protocol.Messages.Outgoing.Moderation;
using DragonEmu.Protocol.Messages.Outgoing.RoomAction;
using DragonEmu.Protocol.Messages.Outgoing.RoomEngine;
using DragonEmu.Game.Settings;

namespace DragonEmu.Game.Chat.Commands;

public abstract class RestChatCommand : IChatCommand
{
    private readonly IRoomManager _roomManager;
    private readonly RoomBroadcaster _roomBroadcaster;
    private readonly MessageTexts _texts;

    protected RestChatCommand(IRoomManager roomManager, RoomBroadcaster roomBroadcaster, MessageTexts texts)
    {
        _texts = texts;
        _roomManager = roomManager;
        _roomBroadcaster = roomBroadcaster;
    }

    public abstract string Name { get; }

    public abstract string? RequiredFuse { get; }

    protected abstract ActorPosture Posture { get; }

    protected virtual bool IsAllowedIn(Room room) => true;

    public ValueTask ExecuteAsync(IGameSession session, string[] args, CancellationToken cancellationToken)
    {
        if (session.CurrentRoomId is not { } roomId
            || !_roomManager.TryGetLoadedRoom(roomId, out Room? room) || room is null
            || !room.TryGetActor(session.Id, out RoomActor? actor) || actor is null)
        {
            session.Send(new ModMessageEvent(_texts.Get(MessageKeys.NotInRoom)).Compose());
            return ValueTask.CompletedTask;
        }

        if (IsAllowedIn(room) && Room.TryRestWhereStanding(actor, Posture, out bool stoppedDancing))
        {
            if (stoppedDancing)
            {
                _roomBroadcaster.Broadcast(room, new DanceMessageEvent(actor.VirtualId, 0).Compose());
            }

            _roomBroadcaster.Broadcast(room, new UserUpdateMessageEvent([actor]).Compose());
        }

        return ValueTask.CompletedTask;
    }
}

public sealed class SitChatCommand(IRoomManager roomManager, RoomBroadcaster roomBroadcaster, MessageTexts texts) : RestChatCommand(roomManager, roomBroadcaster, texts)
{
    public override string Name => "sit";

    public override string? RequiredFuse => null;

    protected override ActorPosture Posture => ActorPosture.Sit;
}

public sealed class LayChatCommand(IRoomManager roomManager, RoomBroadcaster roomBroadcaster, PublicRoomCache publicRooms, MessageTexts texts)
    : RestChatCommand(roomManager, roomBroadcaster, texts)
{
    public override string Name => "lay";

    public override string? RequiredFuse => RoomRightsService.AdministratorAccessFuse;

    protected override ActorPosture Posture => ActorPosture.Lay;

    protected override bool IsAllowedIn(Room room) => !publicRooms.IsPublic(room.Id);
}
