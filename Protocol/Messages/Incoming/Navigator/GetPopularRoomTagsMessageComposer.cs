using DragonEmu.Game.Rooms;
using DragonEmu.Protocol.Messages.Outgoing.Navigator;

namespace DragonEmu.Protocol.Messages.Incoming.Navigator;

public sealed class GetPopularRoomTagsMessageComposer : IMessageComposer
{
    public const int MaxTags = 25;

    private readonly IRoomManager _roomManager;

    public GetPopularRoomTagsMessageComposer(IRoomManager roomManager)
    {
        _roomManager = roomManager;
    }

    public void Parse(ClientPacket packet)
    {
    }

    public ValueTask HandleAsync(IGameSession session, CancellationToken cancellationToken)
    {
        var tags = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        foreach (Room room in _roomManager.LoadedRooms)
        {
            int usersNow = room.UserCount;
            if (usersNow == 0)
            {
                continue;
            }

            foreach (string tag in room.Data.Tags)
            {
                tags[tag] = tags.GetValueOrDefault(tag) + usersNow;
            }
        }

        List<(string Tag, int UsersNow)> popular = tags
            .OrderByDescending(tag => tag.Value)
            .ThenBy(tag => tag.Key, StringComparer.OrdinalIgnoreCase)
            .Take(MaxTags)
            .Select(tag => (tag.Key, tag.Value))
            .ToList();

        session.Send(new PopularRoomTagsResultEvent(popular).Compose());
        return ValueTask.CompletedTask;
    }
}
