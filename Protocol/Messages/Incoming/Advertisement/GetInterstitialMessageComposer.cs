using DragonEmu.Domain;
using DragonEmu.Game.Advertisement;
using DragonEmu.Protocol.Messages.Outgoing.Advertisement;

namespace DragonEmu.Protocol.Messages.Incoming.Advertisement;

public sealed class GetInterstitialMessageComposer : IMessageComposer
{
    private readonly InterstitialAdCache _adCache;

    public GetInterstitialMessageComposer(InterstitialAdCache adCache)
    {
        _adCache = adCache;
    }

    public void Parse(ClientPacket packet)
    {
    }

    public ValueTask HandleAsync(IGameSession session, CancellationToken cancellationToken)
    {
        InterstitialAd? ad = session.NoInterstitialUntil > DateTime.UtcNow
            ? null
            : InterstitialAdSelector.SelectAd(_adCache.Ads, DateTime.UtcNow, Random.Shared);

        session.Send(new InterstitialMessageEvent(ad?.ImageUrl ?? string.Empty, ad?.Link ?? string.Empty).Compose());
        return ValueTask.CompletedTask;
    }
}
