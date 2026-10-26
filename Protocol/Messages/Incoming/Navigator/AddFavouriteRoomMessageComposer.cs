using DragonEmu.Game.Settings;
using DragonEmu.Persistence;
using DragonEmu.Protocol.Messages.Outgoing.Navigator;
using DragonEmu.Protocol.Messages.Outgoing.Notifications;

namespace DragonEmu.Protocol.Messages.Incoming.Navigator;

public sealed class AddFavouriteRoomMessageComposer : IMessageComposer
{
    private const string FavouritesFullText = "${navigator.favouritesfull.body}";

    private readonly IFavouriteRoomRepository _favouriteRoomRepository;
    private readonly GameSettingsCache _gameSettings;

    private int _roomId;

    public AddFavouriteRoomMessageComposer(IFavouriteRoomRepository favouriteRoomRepository, GameSettingsCache gameSettings)
    {
        _favouriteRoomRepository = favouriteRoomRepository;
        _gameSettings = gameSettings;
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

        int limit = _gameSettings.Current.MaxFavouriteRooms;
        switch (await _favouriteRoomRepository.TryAddAsync(account.Id, _roomId, limit, cancellationToken))
        {
            case FavouriteAddResult.Added:
                session.Send(new FavouriteChangedEvent(_roomId, added: true).Compose());
                break;
            case FavouriteAddResult.ListFull:
                session.Send(new HabboBroadcastMessageEvent(FavouritesFullText).Compose());
                break;
        }
    }
}
