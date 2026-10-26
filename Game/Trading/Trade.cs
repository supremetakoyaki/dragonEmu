using System.Globalization;
using DragonEmu.Domain;
using DragonEmu.Game.Rooms;
using DragonEmu.Protocol;
using DragonEmu.Protocol.Messages.Outgoing.InventoryFurni;
using DragonEmu.Protocol.Messages.Outgoing.InventoryTrading;

namespace DragonEmu.Game.Trading;

public enum TradeState
{
    Opening,

    Offering,

    Confirming,

    Completing,

    Done,
}

public sealed class Trade
{
    public const int MaxStacks = 9;

    public static readonly TimeSpan ConfirmCountdown = TimeSpan.FromSeconds(3);

    private readonly object _lock = new();
    private readonly TimeProvider _time;
    private readonly int _maxItemsPerSide;
    private TradeState _state = TradeState.Opening;
    private long _confirmingSince;

    public Trade(Room room, TradeSide opener, TradeSide partner, int maxItemsPerSide, TimeProvider? time = null)
    {
        Room = room;
        Opener = opener;
        Partner = partner;
        _maxItemsPerSide = maxItemsPerSide;
        _time = time ?? TimeProvider.System;
    }

    public Room Room { get; }

    public TradeSide Opener { get; }

    public TradeSide Partner { get; }

    public TradeState State
    {
        get
        {
            lock (_lock)
            {
                return _state;
            }
        }
    }

    public bool TryStart()
    {
        lock (_lock)
        {
            if (_state != TradeState.Opening)
            {
                return false;
            }

            if (!IsInTheRoom(Opener) || !IsInTheRoom(Partner))
            {
                EndLocked();
                return false;
            }

            _state = TradeState.Offering;
            SendToBoth(new TradingOpenEvent(Opener.UserId, Opener.CanOffer, Partner.UserId, Partner.CanOffer).Compose());
            return true;
        }
    }

    public bool TryOffer(int userId, UserItem item)
    {
        lock (_lock)
        {
            if (_state != TradeState.Offering || SideOf(userId) is not { CanOffer: true, Accepted: false } side
                || item.UserId != userId || item.RoomRef is not null || !item.AllowTrade || item.ExpiresAt is not null || item.OnMarketplace
                || side.Offered.ContainsKey(item.Id) || side.Offered.Count >= _maxItemsPerSide || !FitsTheKinds(side, item))
            {
                return false;
            }

            side.Offered.Add(item.Id, item);
            OffersChangedLocked();
            return true;
        }
    }

    public bool TryTakeBack(int userId, int itemId)
    {
        lock (_lock)
        {
            if (_state != TradeState.Offering || SideOf(userId) is not { Accepted: false } side || !side.Offered.Remove(itemId))
            {
                return false;
            }

            OffersChangedLocked();
            return true;
        }
    }

    public bool Accept(int userId)
    {
        lock (_lock)
        {
            if (_state != TradeState.Offering || SideOf(userId) is not { Accepted: false } side
                || Opener.Offered.Count + Partner.Offered.Count == 0)
            {
                return false;
            }

            side.Accepted = true;
            bool final = OtherThan(side).Accepted;
            if (final)
            {
                _state = TradeState.Confirming;
                Opener.Accepted = Partner.Accepted = false;
                _confirmingSince = _time.GetTimestamp();
            }

            SendToBoth(new TradingAcceptEvent(userId, accepted: true).Compose());
            if (final)
            {
                SendToBoth(new TradingConfirmationEvent().Compose());
            }

            return true;
        }
    }

    public bool Unaccept(int userId)
    {
        lock (_lock)
        {
            if (_state != TradeState.Offering || SideOf(userId) is not { Accepted: true } side)
            {
                return false;
            }

            side.Accepted = false;
            SendToBoth(new TradingAcceptEvent(userId, accepted: false).Compose());
            return true;
        }
    }

    public bool Confirm(int userId)
    {
        lock (_lock)
        {
            if (_state != TradeState.Confirming || SideOf(userId) is not { Accepted: false } side
                || _time.GetElapsedTime(_confirmingSince) < ConfirmCountdown)
            {
                return false;
            }

            side.Accepted = true;
            bool final = OtherThan(side).Accepted;
            if (final)
            {
                _state = TradeState.Completing;
            }

            SendToBoth(new TradingAcceptEvent(userId, accepted: true).Compose());
            return final;
        }
    }

    public bool Cancel(int userId)
    {
        lock (_lock)
        {
            if (_state is TradeState.Completing or TradeState.Done || SideOf(userId) is null)
            {
                return false;
            }

            bool windowsOpen = _state != TradeState.Opening;
            EndLocked();
            if (windowsOpen)
            {
                SendToBoth(new TradingCloseEvent(userId).Compose());
            }

            return true;
        }
    }

    public void Finish(bool completed)
    {
        lock (_lock)
        {
            if (_state != TradeState.Completing)
            {
                return;
            }

            EndLocked();
            if (completed)
            {
                SendToBoth(new TradingCompletedEvent().Compose());
            }
            else
            {
                Opener.Session.Send(new TradingCloseEvent(Opener.UserId).Compose());
                Partner.Session.Send(new TradingCloseEvent(Partner.UserId).Compose());
            }
        }
    }

    public bool IsOffered(int userId, int itemId)
    {
        lock (_lock)
        {
            return SideOf(userId)?.Offered.ContainsKey(itemId) == true;
        }
    }

    public static string KindOf(UserItem item)
    {
        int category = UserItemWireFormat.CategoryOf(item);
        string sprite = item.RefFurnidata.ToString(CultureInfo.InvariantCulture);
        return !UserItemWireFormat.IsGroupable(item, category) ? $"itemid{item.Id}"
            : category == UserItemWireFormat.PosterCategory ? $"{sprite}poster{item.ExtraData}"
            : sprite;
    }

    private static bool FitsTheKinds(TradeSide side, UserItem item)
    {
        HashSet<string> kinds = side.Offered.Values.Select(KindOf).ToHashSet();
        return kinds.Count < MaxStacks || kinds.Contains(KindOf(item));
    }

    private void OffersChangedLocked()
    {
        Opener.Accepted = Partner.Accepted = false;
        SendToBoth(new TradingItemListEvent(Opener.UserId, Opener.Items, Partner.UserId, Partner.Items).Compose());
    }

    private void EndLocked()
    {
        _state = TradeState.Done;
        Opener.Actor.LeaveTrade(this);
        Partner.Actor.LeaveTrade(this);
    }

    private bool IsInTheRoom(TradeSide side) =>
        Room.TryGetActor(side.Actor.SessionId, out RoomActor? actor) && ReferenceEquals(actor, side.Actor);

    private TradeSide? SideOf(int userId) =>
        Opener.UserId == userId ? Opener : Partner.UserId == userId ? Partner : null;

    private TradeSide OtherThan(TradeSide side) => ReferenceEquals(side, Opener) ? Partner : Opener;

    private void SendToBoth(ServerPacket packet)
    {
        Opener.Session.Send(packet);
        Partner.Session.Send(packet);
    }
}
