using DragonEmu.Game.Avatars;
using DragonEmu.Game.Rooms;
using DragonEmu.Protocol.Messages.Outgoing.Users;

namespace DragonEmu.Protocol.Messages.Incoming.Users;

public sealed class GetUserTagsMessageComposer : IMessageComposer
{
    private readonly IRoomManager _roomManager;
    private readonly UserTagService _tags;

    private int _userId;

    public GetUserTagsMessageComposer(IRoomManager roomManager, UserTagService tags)
    {
        _roomManager = roomManager;
        _tags = tags;
    }

    public void Parse(ClientPacket packet)
    {
        _userId = packet.PopInt32();
    }

    public async ValueTask HandleAsync(IGameSession session, CancellationToken cancellationToken)
    {
        if (session.AuthenticatedUser is not { } account)
        {
            return;
        }

        bool inMyRoom = session.CurrentRoomId is { } roomId && _roomManager.TryGetLoadedRoom(roomId, out Room? room)
            && room!.Actors.Any(actor => actor.Type == ActorType.Player && actor.AccountId == _userId);
        if (_userId != account.Id && !inMyRoom)
        {
            return;
        }

        session.Send(new UserTagsMessageEvent(_userId, await _tags.GetAsync(_userId, cancellationToken)).Compose());
    }
}
