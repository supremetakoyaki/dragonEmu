using DragonEmu.Domain;
using DragonEmu.Game.Catalog;
using DragonEmu.Game.Subscriptions;
using DragonEmu.Persistence;
using DragonEmu.Protocol.Messages.Outgoing.Catalog;

namespace DragonEmu.Protocol.Messages.Incoming.Catalog;

public sealed class GetClubOffersMessageComposer : IMessageComposer
{
    private readonly CatalogCache _catalog;
    private readonly SubscriptionManager _subscriptions;
    private readonly IUserRepository _users;

    public GetClubOffersMessageComposer(CatalogCache catalog, SubscriptionManager subscriptions, IUserRepository users)
    {
        _catalog = catalog;
        _subscriptions = subscriptions;
        _users = users;
    }

    public void Parse(ClientPacket packet)
    {
    }

    public async ValueTask HandleAsync(IGameSession session, CancellationToken cancellationToken)
    {
        if (session.AuthenticatedUser is not { } account)
        {
            return;
        }

        ClubMembership membership = await _subscriptions.GetMembershipAsync(account.Id, cancellationToken);
        (DateTime? expiresAt, _, _) = await _users.GetClubStatusAsync(account.Id, cancellationToken);

        session.Send(new HabboClubOffersMessageEvent([.. ClubOfferRules.Listing(_catalog.ClubOffers, membership, expiresAt, DateTime.UtcNow).Select(o =>
            new HabboClubOffersMessageEvent.Offer(
                o.OfferId, o.Code, o.Price, o.Upgrade, o.Vip, o.Months, o.DaysLeftAfterPurchase, o.EndsAt.Year, o.EndsAt.Month, o.EndsAt.Day))]).Compose());
    }
}
