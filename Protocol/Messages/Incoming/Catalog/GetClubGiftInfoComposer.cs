using DragonEmu.Domain;
using DragonEmu.Game.Catalog;
using DragonEmu.Game.Subscriptions;
using DragonEmu.Persistence;
using DragonEmu.Protocol.Messages.Outgoing.Catalog;

namespace DragonEmu.Protocol.Messages.Incoming.Catalog;

public sealed class GetClubGiftInfoComposer : IMessageComposer
{
    private readonly CatalogCache _catalogCache;
    private readonly IUserRepository _userRepository;
    private readonly SubscriptionManager _subscriptions;

    public GetClubGiftInfoComposer(CatalogCache catalogCache, IUserRepository userRepository, SubscriptionManager subscriptions)
    {
        _catalogCache = catalogCache;
        _userRepository = userRepository;
        _subscriptions = subscriptions;
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

        ClubGiftRules.Standing standing = await _userRepository.GetClubGiftStandingAsync(account.Id, cancellationToken);
        ClubMembership membership = await _subscriptions.GetMembershipAsync(account.Id, cancellationToken);

        session.Send(new ClubGiftInfoEvent(
            daysUntilNextGift: 0,
            giftsAvailableNow: ClubGiftRules.PicksShown(standing, membership),
            gifts: _catalogCache.ClubGifts,
            isSelectable: gift => ClubGiftRules.MayPick(gift, standing, membership),
            daysRequired: gift => ClubGiftRules.DaysShown(gift, standing, membership)).Compose());
    }
}
