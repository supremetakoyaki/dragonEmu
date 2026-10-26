using DragonEmu.Domain;
using DragonEmu.Game.Rooms;
using DragonEmu.Persistence;
using DragonEmu.Protocol.Messages.Outgoing.Navigator;

namespace DragonEmu.Protocol.Messages.Incoming.Navigator;

public sealed class UpdateRoomThumbnailMessageComposer : IMessageComposer
{
    private const int MaxImageId = 99;
    private const int MaxPosition = 10;
    private const int MaxStickers = MaxPosition + 1;

    private readonly IRoomManager _roomManager;
    private readonly IPrivateRoomRepository _privateRoomRepository;
    private readonly RoomRightsService _roomRights;

    private int _roomId;
    private int _background;
    private int _foreground;
    private List<RoomIconItem>? _stickers = [];

    public UpdateRoomThumbnailMessageComposer(IRoomManager roomManager, IPrivateRoomRepository privateRoomRepository, RoomRightsService roomRights)
    {
        _roomManager = roomManager;
        _privateRoomRepository = privateRoomRepository;
        _roomRights = roomRights;
    }

    public void Parse(ClientPacket packet)
    {
        _roomId = packet.PopInt32();
        _background = packet.PopInt32();
        _foreground = packet.PopInt32();

        int count = packet.PopInt32();
        if (count is < 0 or > MaxStickers)
        {
            _stickers = null;
            return;
        }

        for (int i = 0; i < count; i++)
        {
            _stickers!.Add(new RoomIconItem(packet.PopInt32(), packet.PopInt32()));
        }
    }

    public async ValueTask HandleAsync(IGameSession session, CancellationToken cancellationToken)
    {
        if (session.AuthenticatedUser is not { } account || !IsValid())
        {
            return;
        }

        Room? room = await _roomManager.GetOrLoadRoomAsync(_roomId, cancellationToken);
        if (room is null
            || !(RoomRightsService.IsOwner(account, room.Data) || await _roomRights.HoldsAnyRoomControllerFuseAsync(account, cancellationToken))
            || !session.TryUseCooldown(ActionCooldowns.RoomThumbnail, ActionCooldowns.Default))
        {
            return;
        }

        List<RoomIconItem> stickers = _stickers!.OrderBy(sticker => sticker.Position).ToList();
        await _privateRoomRepository.SaveIconAsync(_roomId, _background, _foreground, stickers, cancellationToken);
        room.ApplySettingsUpdate(room.Data with { IconBackground = _background, IconForeground = _foreground, IconItems = stickers });

        session.Send(new RoomThumbnailUpdateResultEvent(_roomId).Compose());
        session.Send(new RoomInfoUpdatedEvent(_roomId).Compose());
    }

    private bool IsValid() =>
        _stickers is not null
        && _background is >= 0 and <= MaxImageId
        && _foreground is >= 0 and <= MaxImageId
        && _stickers.All(sticker => sticker.Position is >= 0 and <= MaxPosition && sticker.ItemId is > 0 and <= MaxImageId)
        && _stickers.Select(sticker => sticker.Position).Distinct().Count() == _stickers.Count;
}
