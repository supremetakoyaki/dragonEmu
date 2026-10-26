using DragonEmu.Game.Moderation;
using DragonEmu.Game.Furniture;
using DragonEmu.Game.Rooms;

namespace DragonEmu.Protocol.Messages.Incoming.RoomEngine;

public sealed class SetItemDataMessageComposer : IMessageComposer
{
    private readonly IRoomManager _roomManager;
    private readonly StickieService _stickies;

    private int _itemId;
    private string _data = string.Empty;

    public SetItemDataMessageComposer(IRoomManager roomManager, StickieService stickies)
    {
        _roomManager = roomManager;
        _stickies = stickies;
    }

    public void Parse(ClientPacket packet)
    {
        _itemId = packet.PopInt32();
        _data = packet.PopString();
    }

    public async ValueTask HandleAsync(IGameSession session, CancellationToken cancellationToken)
    {
        if (session.AuthenticatedUser is { } account
            && session.CurrentRoomId is { } roomId
            && _roomManager.TryGetLoadedRoom(roomId, out Room? room)
            && !ModerationService.IsMuted(session))
        {
            await _stickies.WriteAsync(account, room!, _itemId, _data, cancellationToken);
        }
    }
}
