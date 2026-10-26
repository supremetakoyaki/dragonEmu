using DragonEmu.Domain;
using DragonEmu.Persistence;
using DragonEmu.Protocol.Messages.Outgoing.RoomEngine;

namespace DragonEmu.Game.Rooms;

public sealed class RoomPaintService
{
    private readonly IPrivateRoomRepository _privateRoomRepository;
    private readonly RoomBroadcaster _roomBroadcaster;

    public RoomPaintService(IPrivateRoomRepository privateRoomRepository, RoomBroadcaster roomBroadcaster)
    {
        _privateRoomRepository = privateRoomRepository;
        _roomBroadcaster = roomBroadcaster;
    }

    public static RoomPaintType? PaintTypeFor(string? classname) => classname switch
    {
        "wallpaper" => RoomPaintType.Wallpaper,
        "floor" => RoomPaintType.Floor,
        "landscape" => RoomPaintType.Landscape,
        _ => null,
    };

    public async Task ApplyAsync(Room room, RoomPaintType type, string value, CancellationToken cancellationToken)
    {
        await _privateRoomRepository.UpdatePaintAsync(room.Id, type, value, cancellationToken);

        string key;
        switch (type)
        {
            case RoomPaintType.Wallpaper:
                room.Wallpaper = value;
                key = "wallpaper";
                break;
            case RoomPaintType.Floor:
                room.Floor = value;
                key = "floor";
                break;
            case RoomPaintType.Landscape:
                room.Landscape = value;
                key = "landscape";
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(type), type, null);
        }

        _roomBroadcaster.Broadcast(room, new RoomPropertyMessageEvent(key, value).Compose());
    }
}
