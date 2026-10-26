using DragonEmu.Domain;
using DragonEmu.Game.Subscriptions;
using DragonEmu.Persistence;
using DragonEmu.Protocol.Messages.Outgoing.Navigator;
using DragonEmu.Game.Rooms;

namespace DragonEmu.Protocol.Messages.Incoming.Navigator;

public sealed class GetPublicSpaceCastLibsMessageComposer : IMessageComposer
{
    private readonly PublicRoomCache _publicRooms;

    private int _roomId;

    public GetPublicSpaceCastLibsMessageComposer(PublicRoomCache publicRooms)
    {
        _publicRooms = publicRooms;
    }

    public void Parse(ClientPacket packet)
    {
        _roomId = packet.PopInt32();
    }

    public ValueTask HandleAsync(IGameSession session, CancellationToken cancellationToken)
    {
        if (session.AuthenticatedUser is not null)
        {
            string libraryName = _publicRooms.RoomDataOf(_roomId) is { } room ? _publicRooms.LibrariesOf(room.HeightmapLabel) : string.Empty;
            session.Send(new PublicSpaceCastLibsEvent(_roomId, libraryName).Compose());
        }

        return ValueTask.CompletedTask;
    }
}
