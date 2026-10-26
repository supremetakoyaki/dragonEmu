using DragonEmu.Domain;
using DragonEmu.Game.Rooms;
using DragonEmu.Protocol.Messages;

namespace DragonEmu.Game.Trading;

public sealed class TradeSide
{
    public TradeSide(IGameSession session, RoomActor actor, bool canOffer)
    {
        Session = session;
        Actor = actor;
        CanOffer = canOffer;
    }

    public IGameSession Session { get; }

    public RoomActor Actor { get; }

    public int UserId => Actor.AccountId;

    public bool CanOffer { get; }

    public bool Accepted { get; internal set; }

    public IReadOnlyList<UserItem> Items => Offered.Values;

    internal OrderedDictionary<int, UserItem> Offered { get; } = new();
}
