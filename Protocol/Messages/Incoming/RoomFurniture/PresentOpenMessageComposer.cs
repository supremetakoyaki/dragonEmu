using DragonEmu.Game.Fuses;
using DragonEmu.Domain;
using DragonEmu.Game.Achievements;
using DragonEmu.Game.Furniture;
using DragonEmu.Game.Rooms;
using DragonEmu.Game.Subscriptions;
using DragonEmu.Gateway;
using DragonEmu.Persistence;
using DragonEmu.Protocol.Messages.Outgoing.Handshake;
using DragonEmu.Protocol.Messages.Outgoing.InventoryFurni;
using DragonEmu.Protocol.Messages.Outgoing.RoomEngine;
using DragonEmu.Protocol.Messages.Outgoing.RoomFurniture;
using DragonEmu.Protocol.Messages.Outgoing.Users;
using DragonEmu.Game.Settings;

namespace DragonEmu.Protocol.Messages.Incoming.RoomFurniture;

public sealed class PresentOpenMessageComposer : IMessageComposer
{
    private readonly IRoomManager _roomManager;
    private readonly IUserItemRepository _userItemRepository;
    private readonly IUserGiftRepository _userGiftRepository;
    private readonly FurnitureDefinitionCache _furnitureDefinitions;
    private readonly IUserRepository _userRepository;
    private readonly SubscriptionManager _subscriptionManager;
    private readonly FuseService _fuses;
    private readonly RoomBroadcaster _roomBroadcaster;
    private readonly RoomRightsService _roomRights;
    private readonly AchievementService _achievementService;
    private readonly TeleportPairGrantService _teleportPairGrantService;
    private readonly HotelClock _clock;

    private int _objectId;

    public PresentOpenMessageComposer(
        IRoomManager roomManager,
        IUserItemRepository userItemRepository,
        IUserGiftRepository userGiftRepository,
        FurnitureDefinitionCache furnitureDefinitions,
        IUserRepository userRepository,
        SubscriptionManager subscriptionManager,
        FuseService fuses,
        RoomBroadcaster roomBroadcaster,
        RoomRightsService roomRights,
        AchievementService achievementService,
        TeleportPairGrantService teleportPairGrantService,
        HotelClock clock)
    {
        _clock = clock;
        _roomManager = roomManager;
        _userItemRepository = userItemRepository;
        _userGiftRepository = userGiftRepository;
        _furnitureDefinitions = furnitureDefinitions;
        _userRepository = userRepository;
        _subscriptionManager = subscriptionManager;
        _fuses = fuses;
        _roomBroadcaster = roomBroadcaster;
        _roomRights = roomRights;
        _achievementService = achievementService;
        _teleportPairGrantService = teleportPairGrantService;
    }

    public void Parse(ClientPacket packet)
    {
        _objectId = packet.PopInt32();
    }

    public async ValueTask HandleAsync(IGameSession session, CancellationToken cancellationToken)
    {
        if (session.AuthenticatedUser is not { } account || session.CurrentRoomId is not { } roomId)
        {
            return;
        }

        if (!_roomManager.TryGetLoadedRoom(roomId, out Room? room))
        {
            return;
        }

        UserItem? item = await _userItemRepository.GetByIdAsync(_objectId, cancellationToken);
        if (item is null || item.RoomRef != roomId || item.LogicClass is not (FurnitureLogicClass.Present or FurnitureLogicClass.EcotronBox))
        {
            return;
        }

        if (item.UserId != account.Id && !await _roomRights.CanUseFurnitureAsync(account, room!.Data, cancellationToken))
        {
            return;
        }

        UserGift? gift = await _userGiftRepository.GetByItemIdAsync(_objectId, cancellationToken);

        bool removedFromFloor = item.WallPosition is null;
        if (removedFromFloor)
        {
            room!.RemoveFloorItem(_objectId);
            _roomBroadcaster.Broadcast(room, new ObjectRemoveMessageEvent(_objectId).Compose());
        }
        else
        {
            room!.RemoveWallItem(_objectId);
            _roomBroadcaster.Broadcast(room, new ItemRemoveMessageEvent(_objectId).Compose());
        }

        await _userItemRepository.DeleteAsync(_objectId, item.UserId, cancellationToken);
        await _userGiftRepository.DeleteAsync(_objectId, cancellationToken);

        session.Send(BuildPresentOpenedEvent(gift).Compose());

        if (gift is null)
        {
            return;
        }

        foreach (GiftContent content in gift.Contents)
        {
            switch (content.Type)
            {
                case GiftContentType.Furni:
                    await GrantFurniAsync(session, item.UserId, content, gift, cancellationToken);
                    break;

                case GiftContentType.Subscription:
                    await GrantSubscriptionAsync(session, account, content, cancellationToken);
                    break;

                case GiftContentType.Effect:
                    break;
            }
        }

        if (gift.SenderUserId is { } senderId && senderId != item.UserId)
        {
            await _achievementService.IncrementAsync(item.UserId, "ACH_GiftReceiver", cancellationToken: cancellationToken);
        }
    }

    private PresentOpenedMessageEvent BuildPresentOpenedEvent(UserGift? gift)
    {
        GiftContent? content = gift?.Contents.FirstOrDefault(c => c.Type is GiftContentType.Furni or GiftContentType.Subscription);
        if (content is null)
        {
            return new PresentOpenedMessageEvent(refFurnidata: 0, isWallItem: false);
        }

        if (content.Type == GiftContentType.Subscription)
        {
            return PresentOpenedMessageEvent.Subscription();
        }

        if (!int.TryParse(content.Reference, out int furniDefId))
        {
            return new PresentOpenedMessageEvent(refFurnidata: 0, isWallItem: false);
        }

        FurnitureDefinition? definition = _furnitureDefinitions.GetById(furniDefId);
        return definition is null
            ? new PresentOpenedMessageEvent(refFurnidata: 0, isWallItem: false)
            : new PresentOpenedMessageEvent(definition.RefFurnidata, definition.Type == FurnitureType.Wall);
    }

    private async ValueTask GrantFurniAsync(
        IGameSession session, int userId, GiftContent content, UserGift gift, CancellationToken cancellationToken)
    {
        if (!int.TryParse(content.Reference, out int furniDefId))
        {
            return;
        }

        FurnitureDefinition? definition = _furnitureDefinitions.GetById(furniDefId);
        if (definition is null)
        {
            return;
        }

        string? extraData = definition.LogicClass == FurnitureLogicClass.Trophy
            ? await BuildGiftedTrophyExtraDataAsync(gift, content.ExtraData, cancellationToken)
            : string.IsNullOrEmpty(content.ExtraData) ? null : content.ExtraData;

        IReadOnlyList<int> newItemIds = await _teleportPairGrantService.GrantAsync(userId, definition, extraData, seen: false, count: 1, cancellationToken);

        foreach (UserItem newItem in await _userItemRepository.GetByIdsAsync([.. newItemIds], cancellationToken))
        {
            session.Send(new FurniListInsertEvent(newItem).Compose());
        }
    }

    private async ValueTask<string> BuildGiftedTrophyExtraDataAsync(UserGift gift, string inscription, CancellationToken cancellationToken)
    {
        string senderName = "Habbo";
        if (gift.SenderUserId is { } senderId)
        {
            UserAccount? sender = await _userRepository.GetByIdAsync(senderId, cancellationToken);
            if (sender is not null)
            {
                senderName = sender.Username;
            }
        }

        return $"{senderName}\t{_clock.TodayText}\t{inscription}";
    }

    private async ValueTask GrantSubscriptionAsync(IGameSession session, UserAccount account, GiftContent content, CancellationToken cancellationToken)
    {
        if (!int.TryParse(content.ExtraData, out int months) || months <= 0)
        {
            return;
        }

        SubscriptionGrantResult grant = await _subscriptionManager.GrantMonthsAsync(account.Id, months, cancellationToken);

        session.Send(ScrSendUserInfoEvent.HabboClubJustSubscribed(grant.ClubExpiresAt).Compose());

        await _fuses.SendHeldAsync(session, account, cancellationToken);
    }
}
