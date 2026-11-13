using DragonEmu.Domain;
using DragonEmu.Game.Avatars;
using DragonEmu.Game.Rooms;
using DragonEmu.Persistence;
using DragonEmu.Protocol.Messages.Outgoing.RoomEngine;

namespace DragonEmu.Protocol.Messages.Incoming.RoomEngine;

public sealed class SetClothingChangeDataMessageComposer : IMessageComposer
{
    private readonly IRoomManager _roomManager;
    private readonly RoomRightsService _roomRights;
    private readonly FigureDataCache _figureData;
    private readonly IUserItemRepository _userItems;
    private readonly RoomBroadcaster _broadcaster;

    private const int NoClub = 0;

    private int _objectId;
    private string _gender = string.Empty;
    private string _figure = string.Empty;

    public SetClothingChangeDataMessageComposer(
        IRoomManager roomManager, RoomRightsService roomRights, FigureDataCache figureData, IUserItemRepository userItems,
        RoomBroadcaster broadcaster)
    {
        _roomManager = roomManager;
        _roomRights = roomRights;
        _figureData = figureData;
        _userItems = userItems;
        _broadcaster = broadcaster;
    }

    public int ObjectId => _objectId;

    public string Gender => _gender;

    public string Figure => _figure;

    public void Parse(ClientPacket packet)
    {
        _objectId = packet.PopInt32();
        _gender = packet.PopString();
        _figure = packet.PopString();
    }

    public async ValueTask HandleAsync(IGameSession session, CancellationToken cancellationToken)
    {
        if (session.AuthenticatedUser is not { } account || session.CurrentRoomId is not { } roomId
            || _gender.Length == 0 || char.ToUpperInvariant(_gender[0]) is not ('M' or 'F'))
        {
            return;
        }

        if (!_roomManager.TryGetLoadedRoom(roomId, out Room? room) || room!.GetFloorItem(_objectId) is not { IsClothingGate: true }
            || !await _roomRights.CanUseFurnitureAsync(account, room.Data, cancellationToken))
        {
            return;
        }

        char gender = char.ToUpperInvariant(_gender[0]);
        string figure = new(_figure.Where(c => c is not (char)0x01 and not (char)0x02 and not ',').ToArray());
        if (figure.Length == 0 || FigureSanitizer.Sanitize(_figureData, figure, gender, NoClub) is not { } sanitized)
        {
            return;
        }

        if (room.SetGateOutfit(_objectId, gender, sanitized) is not { } data)
        {
            return;
        }

        await _userItems.UpdateExtraDataAsync(_objectId, data, cancellationToken);
        _broadcaster.Broadcast(room, new ObjectDataUpdateMessageEvent(_objectId, data).Compose());
    }
}
