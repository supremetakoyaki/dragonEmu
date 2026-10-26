using DragonEmu.Game.Settings;
using DragonEmu.Domain;
using DragonEmu.Game.Rooms;
using DragonEmu.Persistence;
using DragonEmu.Protocol.Messages.Outgoing.RoomSettings;

namespace DragonEmu.Protocol.Messages.Incoming.RoomSettings;

public sealed class GetRoomSettingsMessageComposer : IMessageComposer
{
    private readonly IRoomManager _roomManager;
    private readonly PublicRoomCache _publicRooms;
    private readonly RoomRightsService _roomRights;
    private readonly IRoomRightsRepository _roomRightsRepository;
    private readonly GameSettingsCache _gameSettings;

    private int _roomId;

    public GetRoomSettingsMessageComposer(
        IRoomManager roomManager, PublicRoomCache publicRooms,
        RoomRightsService roomRights, IRoomRightsRepository roomRightsRepository, GameSettingsCache gameSettings)
    {
        _roomManager = roomManager;
        _publicRooms = publicRooms;
        _roomRights = roomRights;
        _roomRightsRepository = roomRightsRepository;
        _gameSettings = gameSettings;
    }

    public void Parse(ClientPacket packet)
    {
        _roomId = packet.PopInt32();
    }

    public async ValueTask HandleAsync(IGameSession session, CancellationToken cancellationToken)
    {
        if (session.AuthenticatedUser is not { } account)
        {
            return;
        }

        if (session.CurrentRoomId != _roomId || !_roomManager.TryGetLoadedRoom(_roomId, out Room? room))
        {
            return;
        }

        if (_publicRooms.IsPublic(_roomId))
        {
            session.Send(new NoSuchFlatEvent(_roomId).Compose());
            return;
        }

        if (!await _roomRights.CanEditAsync(account, room!.Data, cancellationToken))
        {
            session.Send(new RoomSettingsErrorEvent(_roomId).Compose());
            return;
        }

        int listLimit = _gameSettings.Current.RoomControllersListLimit;
        IReadOnlyList<RoomRightsHolder> holders = await _roomRightsRepository.GetHoldersAsync(_roomId, listLimit, cancellationToken);
        int totalHolderCount = await _roomRightsRepository.GetHolderCountAsync(_roomId, cancellationToken);

        session.Send(new RoomSettingsDataEvent(
            _roomId, room.Data.Label, room.Data.Description, room.Data.AccessType, room.Data.CategoryId, room.Data.MaxUsers,
            room.Data.Tags, holders, totalHolderCount, room.Data.AllowPets, room.Data.AllowPetsEat, room.Data.AllowWalkthrough).Compose());
    }
}
