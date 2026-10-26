using DragonEmu.Game.Rooms;
using DragonEmu.Persistence;
using DragonEmu.Protocol.Messages.Outgoing.RoomEngine;

namespace DragonEmu.Protocol.Messages.Incoming.RoomPublicRoom;

public sealed class ExitLockerRoomMessageComposer : IMessageComposer
{
    private readonly IRoomManager _roomManager;
    private readonly IUserRepository _users;
    private readonly RoomBroadcaster _broadcaster;

    public ExitLockerRoomMessageComposer(IRoomManager roomManager, IUserRepository users, RoomBroadcaster broadcaster)
    {
        _roomManager = roomManager;
        _users = users;
        _broadcaster = broadcaster;
    }

    public void Parse(ClientPacket packet)
    {
    }

    public async ValueTask HandleAsync(IGameSession session, CancellationToken cancellationToken)
    {
        if (session.AuthenticatedUser is not { } account || session.CurrentRoomId is not { } roomId
            || !_roomManager.TryGetLoadedRoom(roomId, out Room? room) || !room!.TryGetActor(session.Id, out RoomActor? actor)
            || !actor!.InLockerRoom)
        {
            return;
        }

        string swimsuit = await _users.GetPoolFigureAsync(account.Id, cancellationToken);
        if (!Room.IsSwimsuit(swimsuit) && !actor.HasSwimwear)
        {
            swimsuit = Room.RandomSwimsuit();
            await _users.UpdatePoolFigureAsync(account.Id, swimsuit, cancellationToken);
        }

        if (room.LeaveLockerRoom(actor, swimsuit))
        {
            session.Swimsuit = actor.Swimsuit;
            _broadcaster.Broadcast(room, new UsersMessageEvent([actor]).Compose());
            _broadcaster.Broadcast(room, new UserUpdateMessageEvent([actor]).Compose());
        }
    }
}
