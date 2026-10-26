using DragonEmu.Domain;
using DragonEmu.Persistence;
using DragonEmu.Protocol.Messages.Outgoing.Advertisement;

namespace DragonEmu.Protocol.Messages.Incoming.Advertisement;

public sealed class GetRoomAdMessageComposer : IMessageComposer
{
    private readonly IRoomAdRepository _roomAds;

    public GetRoomAdMessageComposer(IRoomAdRepository roomAds)
    {
        _roomAds = roomAds;
    }

    public void Parse(ClientPacket packet)
    {
    }

    public async ValueTask HandleAsync(IGameSession session, CancellationToken cancellationToken)
    {
        if (session.AuthenticatedUser is null || session.CurrentRoomId is not { } roomId)
        {
            return;
        }

        IReadOnlyList<RoomAd> ads = await _roomAds.GetForRoomAsync(roomId, cancellationToken);
        if (ads.Count == 0)
        {
            return;
        }

        RoomAd ad = ads[Random.Shared.Next(ads.Count)];
        session.Send(new RoomAdMessageEvent(ad.ImageUrl, ad.Link).Compose());
    }
}
