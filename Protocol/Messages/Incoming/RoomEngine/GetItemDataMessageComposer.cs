using DragonEmu.Game.Furniture;
using DragonEmu.Game.Rooms;
using DragonEmu.Protocol.Messages.Outgoing.RoomEngine;

namespace DragonEmu.Protocol.Messages.Incoming.RoomEngine;

public sealed class GetItemDataMessageComposer : IMessageComposer
{
    private readonly IRoomManager _roomManager;

    private int _itemId;

    public GetItemDataMessageComposer(IRoomManager roomManager)
    {
        _roomManager = roomManager;
    }

    public void Parse(ClientPacket packet)
    {
        _itemId = packet.PopInt32();
    }

    public ValueTask HandleAsync(IGameSession session, CancellationToken cancellationToken)
    {
        if (session.CurrentRoomId is { } roomId
            && _roomManager.TryGetLoadedRoom(roomId, out Room? room)
            && StickieService.NoteIn(room!, _itemId) is { } note)
        {
            session.Send(new ItemDataUpdateMessageEvent(note.Id, note.ExtraData ?? string.Empty).Compose());
        }

        return ValueTask.CompletedTask;
    }
}
