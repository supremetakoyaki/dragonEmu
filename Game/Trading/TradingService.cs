using DragonEmu.Domain;
using DragonEmu.Game.Achievements;
using DragonEmu.Game.Fuses;
using DragonEmu.Game.Rooms;
using DragonEmu.Game.Settings;
using DragonEmu.Gateway;
using DragonEmu.Persistence;
using DragonEmu.Protocol.Messages;
using DragonEmu.Protocol.Messages.Outgoing.InventoryFurni;
using DragonEmu.Protocol.Messages.Outgoing.InventoryTrading;
using DragonEmu.Protocol.Messages.Outgoing.Moderation;
using Microsoft.Extensions.Logging;

namespace DragonEmu.Game.Trading;

public sealed class TradingService
{
    public const string TradeFuse = "fuse_trade";

    private const string TraderPass = "ACH_TraderPass";

    private readonly IRoomManager _roomManager;
    private readonly ClientSessionManager _sessionManager;
    private readonly FuseService _fuses;
    private readonly GameSettingsCache _gameSettings;
    private readonly IUserItemRepository _userItemRepository;
    private readonly ITradeRepository _tradeRepository;
    private readonly AchievementService _achievementService;
    private readonly ILogger<TradingService> _logger;

    private readonly HotelStatus _hotel;
    private readonly MessageTexts _texts;

    public TradingService(
        IRoomManager roomManager, ClientSessionManager sessionManager, FuseService fuses, GameSettingsCache gameSettings,
        IUserItemRepository userItemRepository, ITradeRepository tradeRepository, AchievementService achievementService,
        HotelStatus hotel, ILogger<TradingService> logger, MessageTexts texts)
    {
        _texts = texts;
        _hotel = hotel;
        _roomManager = roomManager;
        _sessionManager = sessionManager;
        _fuses = fuses;
        _gameSettings = gameSettings;
        _userItemRepository = userItemRepository;
        _tradeRepository = tradeRepository;
        _achievementService = achievementService;
        _logger = logger;
    }

    public Trade? TradeOf(IGameSession session) =>
        session.CurrentRoomId is { } roomId && _roomManager.TryGetLoadedRoom(roomId, out Room? room)
        && room!.TryGetActor(session.Id, out RoomActor? actor)
            ? actor!.Trade
            : null;

    public bool IsOffered(IGameSession session, int itemId) =>
        session.AuthenticatedUser is { } account && TradeOf(session)?.IsOffered(account.Id, itemId) == true;

    public async Task OpenAsync(IGameSession session, UserAccount account, int partnerVirtualId, CancellationToken cancellationToken)
    {
        if (_hotel.IsShuttingDown)
        {
            session.Send(new TradingYouAreNotAllowedEvent().Compose());
            return;
        }

        if (!_gameSettings.Current.TradingEnabled
            || session.CurrentRoomId is not { } roomId || !_roomManager.TryGetLoadedRoom(roomId, out Room? room)
            || !room!.Data.AllowTrading
            || !room.TryGetActor(session.Id, out RoomActor? opener)
            || !room.TryGetActorByVirtualId(partnerVirtualId, out RoomActor? partner)
            || partner!.Type != ActorType.Player || partner.AccountId == account.Id
            || !_sessionManager.TryGet(partner.SessionId, out ClientSession? partnerSession)
            || partnerSession!.AuthenticatedUser is not { } partnerAccount)
        {
            return;
        }

        if (opener!.Trade is not null)
        {
            session.Send(new TradingAlreadyOpenEvent().Compose());
            return;
        }

        if (partner.Trade is not null)
        {
            return;
        }

        if (!await _fuses.HoldsAsync(account, TradeFuse, cancellationToken: cancellationToken))
        {
            session.Send(new TradingYouAreNotAllowedEvent().Compose());
            return;
        }

        bool partnerCanOffer = await _fuses.HoldsAsync(partnerAccount, TradeFuse, cancellationToken: cancellationToken);
        var trade = new Trade(
            room, new TradeSide(session, opener, canOffer: true), new TradeSide(partnerSession, partner, partnerCanOffer),
            _gameSettings.Current.MaxItemsInATrade);

        if (!opener.TryJoinTrade(trade))
        {
            session.Send(new TradingAlreadyOpenEvent().Compose());
            return;
        }

        if (!partner.TryJoinTrade(trade))
        {
            opener.LeaveTrade(trade);
            return;
        }

        trade.TryStart();
    }

    public async Task OfferAsync(IGameSession session, UserAccount account, int itemId, CancellationToken cancellationToken)
    {
        if (TradeOf(session) is { } trade && await _userItemRepository.GetByIdAsync(itemId, cancellationToken) is { } item)
        {
            trade.TryOffer(account.Id, item);
        }
    }

    public void TakeBack(IGameSession session, UserAccount account, int itemId) => TradeOf(session)?.TryTakeBack(account.Id, itemId);

    public void Accept(IGameSession session, UserAccount account) => TradeOf(session)?.Accept(account.Id);

    public void Unaccept(IGameSession session, UserAccount account) => TradeOf(session)?.Unaccept(account.Id);

    public void Cancel(IGameSession session, UserAccount account) => TradeOf(session)?.Cancel(account.Id);

    public async Task ConfirmAsync(IGameSession session, UserAccount account)
    {
        if (TradeOf(session) is { } trade && trade.Confirm(account.Id))
        {
            await CompleteAsync(trade);
        }
    }

    private async Task CompleteAsync(Trade trade)
    {
        TradeSide opener = trade.Opener;
        TradeSide partner = trade.Partner;
        var completed = new CompletedTrade(
            trade.Room.Id,
            new CompletedTradeSide(opener.UserId, opener.Session.RemoteAddress, [.. opener.Items.Select(item => item.Id)]),
            new CompletedTradeSide(partner.UserId, partner.Session.RemoteAddress, [.. partner.Items.Select(item => item.Id)]));

        int? tradeId = null;
        try
        {
            tradeId = await _tradeRepository.CompleteAsync(completed, CancellationToken.None);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Trade between users {User1} and {User2} failed to complete.", opener.UserId, partner.UserId);
        }

        trade.Finish(completed: tradeId is not null);

        if (tradeId is null)
        {
            _logger.LogWarning("Trade between users {User1} and {User2} not completed, as an item was no longer theirs to give.", opener.UserId, partner.UserId);
            opener.Session.Send(new ModMessageEvent(_texts.Get(MessageKeys.TradeNothingExchanged)).Compose());
            partner.Session.Send(new ModMessageEvent(_texts.Get(MessageKeys.TradeNothingExchanged)).Compose());
            return;
        }

        Deliver(opener, given: opener.Items, received: partner.Items);
        Deliver(partner, given: partner.Items, received: opener.Items);
        _logger.LogDebug(
            "Trade {TradeId} in room {RoomId}: user {User1} gave {Count1} items, user {User2} gave {Count2}.",
            tradeId, trade.Room.Id, opener.UserId, opener.Items.Count, partner.UserId, partner.Items.Count);

        await _achievementService.IncrementFromAsync(opener.UserId, TraderPass, from: 1, cancellationToken: CancellationToken.None);
        await _achievementService.IncrementFromAsync(partner.UserId, TraderPass, from: 1, cancellationToken: CancellationToken.None);
    }

    private static void Deliver(TradeSide side, IReadOnlyList<UserItem> given, IReadOnlyList<UserItem> received)
    {
        foreach (UserItem item in given)
        {
            side.Session.Send(new FurniListRemoveEvent(item.Id).Compose());
        }

        foreach (UserItem item in received)
        {
            side.Session.Send(new FurniListInsertEvent(item with { UserId = side.UserId }).Compose());
        }
    }
}
