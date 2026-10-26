using DragonEmu.Game.Furniture;
using DragonEmu.Game.Rooms;

namespace DragonEmu.Protocol.Messages.Incoming.RoomEngine;

public sealed class RemoveItemMessageComposer : IMessageComposer
{
    private readonly IRoomManager _roomManager;
    private readonly StickieService _stickies;

    private int _itemId;

    public RemoveItemMessageComposer(IRoomManager roomManager, StickieService stickies)
    {
        _roomManager = roomManager;
        _stickies = stickies;
    }

    public void Parse(ClientPacket packet)
    {
        _itemId = packet.PopInt32();
    }

    public async ValueTask HandleAsync(IGameSession session, CancellationToken cancellationToken)
    {
        if (session.AuthenticatedUser is { } account
            && session.CurrentRoomId is { } roomId
            && _roomManager.TryGetLoadedRoom(roomId, out Room? room))
        {
            await _stickies.RemoveAsync(account, room!, _itemId, cancellationToken);
        }
    }
}
