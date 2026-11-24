using System.Collections.Concurrent;
using DragonEmu.Domain;
using DragonEmu.Game.Achievements;
using DragonEmu.Game.Rooms;
using DragonEmu.Persistence;
using DragonEmu.Protocol;
using DragonEmu.Protocol.Messages;
using DragonEmu.Protocol.Messages.Outgoing.Catalog;
using DragonEmu.Protocol.Messages.Outgoing.InventoryPurse;
using DragonEmu.Protocol.Messages.Outgoing.Notifications;
using DragonEmu.Protocol.Messages.Outgoing.RoomEngine;
using Microsoft.Extensions.Logging;

namespace DragonEmu.Game.Furniture;

public sealed class FireworkService
{
    private const int Launch = 0;
    private const int OpenDialog = 1;
    private const int Charge = 2;

    private readonly IUserItemRepository _items;
    private readonly ICurrencyRepository _currency;
    private readonly IFireworkRepository _fireworks;
    private readonly RoomBroadcaster _broadcaster;
    private readonly AchievementService _achievements;
    private readonly ILogger<FireworkService> _logger;
    private readonly ConcurrentDictionary<int, byte> _launching = new();

    public FireworkService(IUserItemRepository items, ICurrencyRepository currency, IFireworkRepository fireworks, RoomBroadcaster broadcaster,
        AchievementService achievements, ILogger<FireworkService> logger)
    {
        _items = items;
        _currency = currency;
        _fireworks = fireworks;
        _broadcaster = broadcaster;
        _achievements = achievements;
        _logger = logger;
    }

    public async Task UseAsync(IGameSession session, int userId, Room room, RoomFloorItem firework, int param, CancellationToken cancellationToken)
    {
        if (await _items.GetByIdAsync(firework.Id, cancellationToken) is not { } stored || stored.RoomRef != room.Id
            || await _fireworks.GetAsync(stored.FurniDefId, cancellationToken) is not { } definition)
        {
            return;
        }

        int charges = stored.Extras ?? 0;
        if (param == Charge)
        {
            await ChargeAsync(session, userId, room, firework, charges, definition, cancellationToken);
            return;
        }

        if (param == OpenDialog || charges <= 0)
        {
            session.Send(ChargeInfo(firework.Id, charges, definition));
            return;
        }

        if (param != Launch)
        {
            return;
        }

        if (!_launching.TryAdd(firework.Id, 0))
        {
            return;
        }

        string after = charges > 1 ? "1" : "0";
        await _items.UpdateExtrasAsync(firework.Id, charges - 1, cancellationToken);
        await _items.UpdateExtraDataAsync(firework.Id, after, cancellationToken);
        _broadcaster.Broadcast(room, new ObjectDataUpdateMessageEvent(firework.Id, "2").Compose());
        _ = EndLaunchAsync(room, firework.Id, definition.LaunchDuration).ContinueWith(
            t => _logger.LogError(t.Exception, "Ending firework {ItemId}'s launch failed.", firework.Id), TaskContinuationOptions.OnlyOnFaulted);
    }

    private async Task EndLaunchAsync(Room room, int itemId, TimeSpan duration)
    {
        try
        {
            await Task.Delay(duration);
            if (room.GetFloorItem(itemId) is not null && await _items.GetByIdAsync(itemId) is { RoomRef: { } roomId } stored && roomId == room.Id)
            {
                _broadcaster.Broadcast(room, new ObjectDataUpdateMessageEvent(itemId, stored.ExtraData ?? "0").Compose());
            }
        }
        finally
        {
            _launching.TryRemove(itemId, out _);
        }
    }

    private async Task ChargeAsync(IGameSession session, int userId, Room room, RoomFloorItem firework, int charges, FireworkDefinition price,
        CancellationToken cancellationToken)
    {
        int pointsCurrency = CurrencyType.OfActivityPoints(price.ActivityPointType);
        long credits = await _currency.GetBalanceAsync(userId, CurrencyType.Credits, cancellationToken);
        long points = await _currency.GetBalanceAsync(userId, pointsCurrency, cancellationToken);
        if (credits < price.PriceCredits || points < price.PriceActivityPoints)
        {
            session.Send(new NotEnoughBalanceMessageEvent(credits < price.PriceCredits, points < price.PriceActivityPoints, price.ActivityPointType).Compose());
            return;
        }

        if (price.PriceCredits > 0)
        {
            if (await _currency.TrySpendAsync(userId, CurrencyType.Credits, price.PriceCredits, cancellationToken) is not { } creditsLeft)
            {
                return;
            }

            session.Send(new CreditBalanceEvent(creditsLeft).Compose());
        }

        if (price.PriceActivityPoints > 0)
        {
            if (await _currency.TrySpendAsync(userId, pointsCurrency, price.PriceActivityPoints, cancellationToken) is not { } pointsLeft)
            {
                if (price.PriceCredits > 0)
                {
                    session.Send(new CreditBalanceEvent(
                        await _currency.AddBalanceAsync(userId, CurrencyType.Credits, price.PriceCredits, cancellationToken)).Compose());
                }

                return;
            }

            session.Send(new HabboActivityPointNotificationMessageEvent(pointsLeft, type: price.ActivityPointType).Compose());
        }

        charges += price.ChargeAmount;
        await _items.UpdateExtrasAsync(firework.Id, charges, cancellationToken);
        await _items.UpdateExtraDataAsync(firework.Id, "1", cancellationToken);
        if (!_launching.ContainsKey(firework.Id))
        {
            _broadcaster.Broadcast(room, new ObjectDataUpdateMessageEvent(firework.Id, "1").Compose());
        }

        session.Send(ChargeInfo(firework.Id, charges, price));

        if (price.ActivityPointType == ActivityPointType.Pixels && price.PriceActivityPoints > 0)
        {
            await _achievements.IncrementAsync(userId, "ACH_FireworksCharger", price.PriceActivityPoints, cancellationToken);
        }
    }

    private static ServerPacket ChargeInfo(int itemId, int charges, FireworkDefinition price) =>
        new ChargeInfoMessageEvent(itemId, charges, price.PriceCredits, price.PriceActivityPoints, price.ActivityPointType, price.ChargeAmount).Compose();
}
