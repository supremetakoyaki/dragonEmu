using DragonEmu.Domain;
using DragonEmu.Game.Furniture;
using DragonEmu.Game.Rooms;
using DragonEmu.Gateway;
using DragonEmu.Persistence;
using DragonEmu.Protocol.Messages.Incoming.RoomEngine.AuxiliaryFunctions;
using DragonEmu.Protocol.Messages.Outgoing.InventoryPurse;

namespace DragonEmu.Protocol.Messages.Incoming.RoomFurniture;

public sealed class CreditFurniRedeemMessageComposer : IMessageComposer
{
    private readonly IRoomManager _roomManager;
    private readonly IUserItemRepository _userItemRepository;
    private readonly FurnitureDefinitionCache _furnitureDefinitions;
    private readonly RoomRightsService _roomRights;
    private readonly ICurrencyRepository _currencyRepository;
    private readonly RoomBroadcaster _roomBroadcaster;
    private readonly ClientSessionManager _sessionManager;

    private int _itemId;

    public CreditFurniRedeemMessageComposer(
        IRoomManager roomManager, IUserItemRepository userItemRepository, FurnitureDefinitionCache furnitureDefinitions,
        RoomRightsService roomRights, ICurrencyRepository currencyRepository, RoomBroadcaster roomBroadcaster, ClientSessionManager sessionManager)
    {
        _roomManager = roomManager;
        _userItemRepository = userItemRepository;
        _furnitureDefinitions = furnitureDefinitions;
        _roomRights = roomRights;
        _currencyRepository = currencyRepository;
        _roomBroadcaster = roomBroadcaster;
        _sessionManager = sessionManager;
    }

    public void Parse(ClientPacket packet)
    {
        _itemId = packet.PopInt32();
    }

    public async ValueTask HandleAsync(IGameSession session, CancellationToken cancellationToken)
    {
        if (session.AuthenticatedUser is not { } account
            || session.CurrentRoomId is not { } roomId
            || !_roomManager.TryGetLoadedRoom(roomId, out Room? room))
        {
            return;
        }

        if (await _userItemRepository.GetByIdAsync(_itemId, cancellationToken) is not { Type: FurnitureType.Floor } item
            || item.RoomRef != roomId
            || _furnitureDefinitions.GetById(item.FurniDefId) is not { } definition
            || CreditFurni.ValueOf(definition.Classname) is not { } credits
            || item.ExpiresAt is not null)
        {
            return;
        }

        if (item.UserId != account.Id && !await _roomRights.HoldsAnyRoomControllerFuseAsync(account, cancellationToken))
        {
            return;
        }

        if (!await _userItemRepository.DeleteAsync(item.Id, item.UserId, cancellationToken))
        {
            return;
        }

        FloorItemRemoval.RemoveFromRoom(room!, item.Id, _roomBroadcaster);

        long balance = await _currencyRepository.AddBalanceAsync(item.UserId, CurrencyType.Credits, credits, cancellationToken);
        if (_sessionManager.TryGetByUserId(item.UserId, out ClientSession? owner))
        {
            owner!.EnqueueSend(new CreditBalanceEvent(balance).Compose());
        }
    }
}
