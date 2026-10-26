using DragonEmu.Game.Catalog;
using DragonEmu.Persistence;
using DragonEmu.Protocol.Messages.Outgoing.Catalog;

namespace DragonEmu.Protocol.Messages.Incoming.Catalog;

public sealed class GetClubGiftInfoComposer : IMessageComposer
{
    private readonly CatalogCache _catalogCache;
    private readonly IUserRepository _userRepository;

    public GetClubGiftInfoComposer(CatalogCache catalogCache, IUserRepository userRepository)
    {
        _catalogCache = catalogCache;
        _userRepository = userRepository;
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

        (_, int accumulatedMonths, int giftsClaimed) = await _userRepository.GetClubStatusAsync(account.Id, cancellationToken);

        int giftsAvailableNow = Math.Max(0, accumulatedMonths - giftsClaimed);

        session.Send(new ClubGiftInfoEvent(
            daysUntilNextGift: 0,
            giftsAvailableNow: giftsAvailableNow,
            gifts: _catalogCache.ClubGifts,
            accumulatedMonths: accumulatedMonths).Compose());
    }
}
