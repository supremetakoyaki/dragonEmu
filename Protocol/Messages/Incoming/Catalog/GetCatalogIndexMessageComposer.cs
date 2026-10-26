using DragonEmu.Domain;
using DragonEmu.Game.Catalog;
using DragonEmu.Game.Furniture;
using DragonEmu.Persistence;
using DragonEmu.Protocol.Messages.Outgoing.Catalog;
using DragonEmu.Protocol.Messages.Outgoing.Notifications;
using DragonEmu.Protocol.Messages.Outgoing.RoomEngine;

namespace DragonEmu.Protocol.Messages.Incoming.Catalog;

public sealed class GetCatalogIndexMessageComposer : IMessageComposer
{
    private readonly CatalogCache _catalogCache;
    private readonly FurnitureAliasCache _furnitureAliasCache;
    private readonly IUserRepository _userRepository;

    public GetCatalogIndexMessageComposer(CatalogCache catalogCache, FurnitureAliasCache furnitureAliasCache, IUserRepository userRepository)
    {
        _catalogCache = catalogCache;
        _furnitureAliasCache = furnitureAliasCache;
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

        IReadOnlyList<CatalogPageSummary> pages = _catalogCache.Pages;
        session.Send(new CatalogIndexMessageEvent(pages, account.Rank).Compose());
        session.Send(new FurnitureAliasesMessageEvent(_furnitureAliasCache.Aliases).Compose());

        (_, int clubMonthsAccumulated, int clubGiftsClaimed) = await _userRepository.GetClubStatusAsync(account.Id, cancellationToken);
        int giftsAvailableNow = Math.Max(0, clubMonthsAccumulated - clubGiftsClaimed);
        if (giftsAvailableNow > 0)
        {
            session.Send(new ClubGiftNotificationEvent(giftsAvailableNow).Compose());
        }
    }
}
