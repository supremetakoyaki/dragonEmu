using DragonEmu.Game.Rooms;
using DragonEmu.Domain;

namespace DragonEmu.Protocol.Messages.Incoming.Navigator;

public sealed class CreateEventMessageComposer : IMessageComposer
{
    private readonly RoomEventService _events;

    private int _category;
    private string _name = string.Empty;
    private string _description = string.Empty;
    private readonly List<string> _tags = [];

    public CreateEventMessageComposer(RoomEventService events)
    {
        _events = events;
    }

    public void Parse(ClientPacket packet)
    {
        _category = packet.PopInt32();
        _name = packet.PopString();
        _description = packet.PopString();
        int tagCount = packet.PopInt32();
        for (int i = 0; i < tagCount && i < RoomEventService.MaxTags; i++)
        {
            _tags.Add(packet.PopString());
        }
    }

    public async ValueTask HandleAsync(IGameSession session, CancellationToken cancellationToken)
    {
        if (session.AuthenticatedUser is { } account)
        {
            await _events.CreateAsync(session, account, _category, _name, _description, _tags, cancellationToken);
        }
    }
}
