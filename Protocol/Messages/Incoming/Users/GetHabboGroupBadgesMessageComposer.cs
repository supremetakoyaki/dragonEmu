using DragonEmu.Domain;
using DragonEmu.Game.Rooms;
using DragonEmu.Protocol.Messages.Outgoing.Users;

namespace DragonEmu.Protocol.Messages.Incoming.Users;

public sealed class GetHabboGroupBadgesMessageComposer : IMessageComposer
{
    private readonly IRoomManager _roomManager;

    public GetHabboGroupBadgesMessageComposer(IRoomManager roomManager)
    {
        _roomManager = roomManager;
    }

    public void Parse(ClientPacket packet)
    {
    }

    public ValueTask HandleAsync(IGameSession session, CancellationToken cancellationToken)
    {
        IReadOnlyList<GroupBadge> badges = session.CurrentRoomId is { } roomId && _roomManager.TryGetLoadedRoom(roomId, out Room? room)
            ? [.. room!.Actors.Select(actor => actor.FavouriteGroup).OfType<GroupBadge>().DistinctBy(badge => badge.GroupId)]
            : [];

        session.Send(new HabboGroupBadgesMessageEvent(badges).Compose());
        return ValueTask.CompletedTask;
    }
}
