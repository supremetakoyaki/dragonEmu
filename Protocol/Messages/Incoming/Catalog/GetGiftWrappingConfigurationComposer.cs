using DragonEmu.Domain;
using DragonEmu.Game.Catalog;
using DragonEmu.Protocol.Messages.Outgoing.Catalog;

namespace DragonEmu.Protocol.Messages.Incoming.Catalog;

public sealed class GetGiftWrappingConfigurationComposer : IMessageComposer
{
    private readonly CatalogCache _catalogCache;

    public GetGiftWrappingConfigurationComposer(CatalogCache catalogCache)
    {
        _catalogCache = catalogCache;
    }

    public void Parse(ClientPacket packet)
    {
    }

    public ValueTask HandleAsync(IGameSession session, CancellationToken cancellationToken)
    {
        if (session.AuthenticatedUser is null)
        {
            return ValueTask.CompletedTask;
        }

        GiftWrapConfig? config = _catalogCache.GiftWrapConfig;
        if (config is null || config.Boxes.Count == 0)
        {
            session.Send(new GiftWrappingConfigurationEvent(false, 0, [], [], []).Compose());
            return ValueTask.CompletedTask;
        }

        int[] boxIds = config.Boxes.Select(b => b.FurnidataRef).ToArray();

        session.Send(new GiftWrappingConfigurationEvent(
            true,
            config.WrapPriceCredits,
            boxIds,
            config.ColourValues,
            config.RibbonIds).Compose());
        return ValueTask.CompletedTask;
    }
}
