using System.Globalization;
using DragonEmu.Game.Settings;
using DragonEmu.Domain;
using DragonEmu.Game.Catalog;
using DragonEmu.Game.Trading;
using DragonEmu.Persistence;
using DragonEmu.Protocol.Messages.Outgoing.InventoryFurni;
using DragonEmu.Protocol.Messages.Outgoing.Recycler;

namespace DragonEmu.Protocol.Messages.Incoming.Recycler;

public sealed class RecycleItemsMessageComposer : IMessageComposer
{
    private const int ItemsPerRecycle = 5;

    private readonly GameSettingsCache _gameSettings;
    private readonly IRecyclerRepository _recyclerRepository;
    private readonly IUserItemRepository _userItemRepository;
    private readonly RecyclerPrizeCache _prizes;
    private readonly TradingService _trading;
    private readonly HotelClock _clock;

    private readonly List<int> _itemIds = [];

    private readonly HotelStatus _hotel;

    public RecycleItemsMessageComposer(
        GameSettingsCache gameSettings, IRecyclerRepository recyclerRepository, IUserItemRepository userItemRepository,
        RecyclerPrizeCache prizes, TradingService trading, HotelClock clock, HotelStatus hotel)
    {
        _hotel = hotel;
        _clock = clock;
        _gameSettings = gameSettings;
        _recyclerRepository = recyclerRepository;
        _userItemRepository = userItemRepository;
        _prizes = prizes;
        _trading = trading;
    }

    public void Parse(ClientPacket packet)
    {
        if (packet.PopInt32() != ItemsPerRecycle)
        {
            return;
        }

        for (int i = 0; i < ItemsPerRecycle; i++)
        {
            _itemIds.Add(packet.PopInt32());
        }
    }

    public async ValueTask HandleAsync(IGameSession session, CancellationToken cancellationToken)
    {
        if (session.AuthenticatedUser is not { } account)
        {
            return;
        }

        if (_trading.TradeOf(session) is null && await RecycleAsync(account, cancellationToken) is { } box)
        {
            foreach (int itemId in _itemIds)
            {
                session.Send(new FurniListRemoveEvent(itemId).Compose());
            }

            session.Send(new FurniListInsertEvent(box).Compose());
            session.Send(RecyclerFinishedMessageEvent.Success(box.Id).Compose());
        }
        else
        {
            session.Send(RecyclerFinishedMessageEvent.Failure().Compose());
        }
    }

    private async Task<UserItem?> RecycleAsync(UserAccount account, CancellationToken cancellationToken)
    {
        RecyclerSettings settings = _gameSettings.Current.Recycler;
        if (!settings.Open || _hotel.IsShuttingDown || _itemIds.Count != ItemsPerRecycle || _itemIds.Distinct().Count() != ItemsPerRecycle
            || _prizes.Draw(Random.Shared) is not { } prize)
        {
            return null;
        }

        DateTime now = DateTime.UtcNow.ToWholeSecond();
        var request = new RecycleRequest(
            account.Id, _itemIds, settings.PrizeBoxFurniDefId, _clock.TodayText,
            new GiftContent(GiftContentType.Furni, prize.FurniDefId.ToString(CultureInfo.InvariantCulture), string.Empty),
            settings.CooldownSeconds, now);

        return await _recyclerRepository.TryRecycleAsync(request, cancellationToken) is { } boxId
            ? await _userItemRepository.GetByIdAsync(boxId, cancellationToken)
            : null;
    }
}
