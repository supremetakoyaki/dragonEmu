using DragonEmu.Domain;
using DragonEmu.Game.Settings;
using DragonEmu.Gateway;
using DragonEmu.Persistence;
using DragonEmu.Protocol.Messages.Outgoing.InventoryFurni;
using DragonEmu.Protocol.Messages.Outgoing.RoomSession;
using Microsoft.Extensions.Logging;

namespace DragonEmu.Game.Rooms;

public sealed class RoomDeletionService
{
    private readonly IRoomManager _roomManager;
    private readonly IPrivateRoomRepository _privateRoomRepository;
    private readonly ClientSessionManager _sessionManager;
    private readonly RoomRightsService _roomRights;
    private readonly GameSettingsCache _gameSettings;
    private readonly ILogger<RoomDeletionService> _logger;

    public RoomDeletionService(
        IRoomManager roomManager, IPrivateRoomRepository privateRoomRepository, ClientSessionManager sessionManager,
        RoomRightsService roomRights, GameSettingsCache gameSettings, ILogger<RoomDeletionService> logger)
    {
        _roomManager = roomManager;
        _privateRoomRepository = privateRoomRepository;
        _sessionManager = sessionManager;
        _roomRights = roomRights;
        _gameSettings = gameSettings;
        _logger = logger;
    }

    public async Task DeleteAsync(UserAccount account, int roomId, CancellationToken cancellationToken)
    {
        PrivateRoomSummary? data = _roomManager.TryGetLoadedRoom(roomId, out Room? loaded)
            ? loaded!.Data
            : await _privateRoomRepository.GetByIdAsync(roomId, cancellationToken);
        if (data is not { OwnerId: not null } || roomId == _gameSettings.Current.InfobusRoomRef
            || (!RoomRightsService.IsOwner(account, data) && !await _roomRights.HoldsAnyRoomControllerFuseAsync(account, cancellationToken))
            || await _privateRoomRepository.DeleteAsync(roomId, cancellationToken) is not { } returnedTo)
        {
            return;
        }

        if (_roomManager.TryUnload(roomId, out Room? room))
        {
            byte[] toHotelView = new CloseConnectionMessageEvent().Compose().ToArray();
            foreach (RoomActor actor in room!.Actors)
            {
                room.TryRemoveActor(actor.SessionId, out _);
                if (_sessionManager.TryGet(actor.SessionId, out ClientSession? session) && session!.CurrentRoomId == roomId)
                {
                    session.SetCurrentRoom(null);
                    session.EnqueueSend(toHotelView);
                }
            }
        }

        byte[] fetchInventory = new FurniListUpdateEvent().Compose().ToArray();
        foreach (int userId in returnedTo)
        {
            if (_sessionManager.TryGetByUserId(userId, out ClientSession? owner))
            {
                owner!.EnqueueSend(fetchInventory);
            }
        }

        _logger.LogDebug("User {UserId} deleted room {RoomId} and furni went back to {Owners} owner(s).", account.Id, roomId, returnedTo.Count);
    }
}
