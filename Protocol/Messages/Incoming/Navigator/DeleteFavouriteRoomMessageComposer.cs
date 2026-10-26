using DragonEmu.Persistence;
using DragonEmu.Protocol.Messages.Outgoing.Navigator;

namespace DragonEmu.Protocol.Messages.Incoming.Navigator;

public sealed class DeleteFavouriteRoomMessageComposer : IMessageComposer
{
    private readonly IFavouriteRoomRepository _favouriteRoomRepository;

    private int _roomId;

    public DeleteFavouriteRoomMessageComposer(IFavouriteRoomRepository favouriteRoomRepository)
    {
        _favouriteRoomRepository = favouriteRoomRepository;
    }

    public void Parse(ClientPacket packet)
    {
        _roomId = packet.PopInt32();
    }

    public async ValueTask HandleAsync(IGameSession session, CancellationToken cancellationToken)
    {
        if (session.AuthenticatedUser is not { } account)
        {
            return;
        }

        if (await _favouriteRoomRepository.RemoveAsync(account.Id, _roomId, cancellationToken))
        {
            session.Send(new FavouriteChangedEvent(_roomId, added: false).Compose());
        }
    }
}
