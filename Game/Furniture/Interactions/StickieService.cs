using DragonEmu.Domain;
using DragonEmu.Game.Chat;
using DragonEmu.Game.Rooms;
using DragonEmu.Persistence;
using DragonEmu.Protocol.Messages.Outgoing.RoomEngine;

namespace DragonEmu.Game.Furniture;

public sealed class StickieService
{
    private readonly IUserItemRepository _userItemRepository;
    private readonly RoomRightsService _roomRights;
    private readonly ProfanityFilterCache _profanityFilter;
    private readonly RoomBroadcaster _broadcaster;

    public StickieService(
        IUserItemRepository userItemRepository, RoomRightsService roomRights, ProfanityFilterCache profanityFilter, RoomBroadcaster broadcaster)
    {
        _userItemRepository = userItemRepository;
        _roomRights = roomRights;
        _profanityFilter = profanityFilter;
        _broadcaster = broadcaster;
    }

    public static UserItem? NoteIn(Room room, int itemId) =>
        room.GetWallItem(itemId) is { LogicClass: FurnitureLogicClass.Stickie } note ? note : null;

    public async Task WriteAsync(UserAccount account, Room room, int noteId, string data, CancellationToken cancellationToken)
    {
        if (NoteIn(room, noteId) is not { } note
            || Stickie.ValidColour(note.Classname, Stickie.ColourOf(data)) is not { } colour
            || !await _roomRights.CanWriteOnStickiesAsync(account, room.Data, cancellationToken))
        {
            return;
        }

        bool recoloured = !string.Equals(colour, Stickie.ColourOf(note.ExtraData), StringComparison.OrdinalIgnoreCase);
        if (recoloured && !await _roomRights.CanManageStickiesAsync(account, room.Data, cancellationToken))
        {
            return;
        }

        string text = RoomTextSanitizer.Sanitize(_profanityFilter.Filter(Stickie.TextOf(data)), Stickie.MaxTextLength);
        string noteData = Stickie.NoteData(colour, text);
        if (noteData == note.ExtraData || !await _userItemRepository.UpdateExtraDataAsync(noteId, noteData, cancellationToken))
        {
            return;
        }

        if (room.SetWallItemExtraData(noteId, noteData) is { } updated && recoloured)
        {
            _broadcaster.Broadcast(room, new ItemRemoveMessageEvent(noteId).Compose());
            _broadcaster.Broadcast(room, new ItemAddMessageEvent(updated).Compose());
        }
    }

    public async Task RemoveAsync(UserAccount account, Room room, int noteId, CancellationToken cancellationToken)
    {
        if (NoteIn(room, noteId) is null
            || !await _roomRights.CanRemoveStickiesAsync(account, room.Data, cancellationToken)
            || !await _userItemRepository.DeleteFromRoomAsync(noteId, room.Id, cancellationToken))
        {
            return;
        }

        room.RemoveWallItem(noteId);
        _broadcaster.Broadcast(room, new ItemRemoveMessageEvent(noteId).Compose());
    }
}
