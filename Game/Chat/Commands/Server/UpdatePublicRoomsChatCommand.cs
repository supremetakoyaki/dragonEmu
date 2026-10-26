using DragonEmu.Game.Rooms;
using DragonEmu.Protocol.Messages;
using DragonEmu.Protocol.Messages.Outgoing.Moderation;
using DragonEmu.Game.Settings;

namespace DragonEmu.Game.Chat.Commands;

public sealed class UpdatePublicRoomsChatCommand : IChatCommand
{
    private readonly PublicRoomCache _publicRooms;
    private readonly IRoomManager _roomManager;
    private readonly MessageTexts _texts;

    public UpdatePublicRoomsChatCommand(PublicRoomCache publicRooms, IRoomManager roomManager, MessageTexts texts)
    {
        _texts = texts;
        _publicRooms = publicRooms;
        _roomManager = roomManager;
    }

    public string Name => "update_public_rooms";

    public string RequiredFuse => RoomRightsService.AdministratorAccessFuse;

    public async ValueTask ExecuteAsync(IGameSession session, string[] args, CancellationToken cancellationToken)
    {
        await _publicRooms.ReloadAsync(cancellationToken);

        foreach (Room room in _roomManager.LoadedRooms.Where(room => room.Data.OwnerId is null))
        {
            room.WarpTiles = _publicRooms.WarpTilesOf(room.Data.HeightmapLabel);
            if (room.PublicExitTiles is not null)
            {
                room.PublicExitTiles = _publicRooms.ExitTilesOf(room.Data.HeightmapLabel);
            }
        }

        session.Send(new ModMessageEvent(
            _texts.Format(MessageKeys.PublicRoomsReloaded, ("rooms", _publicRooms.Rooms.Count), ("folders", _publicRooms.Categories.Count))).Compose());
    }
}
