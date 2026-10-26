using DragonEmu.Game.Rooms;

namespace DragonEmu.Protocol.Messages.Incoming.RoomPublicRoom;

public sealed class ChangeRoomMessageComposer : IMessageComposer
{
    private readonly IRoomManager _roomManager;
    private readonly PublicRoomTransfers _transfers;

    public ChangeRoomMessageComposer(IRoomManager roomManager, PublicRoomTransfers transfers)
    {
        _roomManager = roomManager;
        _transfers = transfers;
    }

    public void Parse(ClientPacket packet)
    {
        packet.PopInt32();
    }

    public ValueTask HandleAsync(IGameSession session, CancellationToken cancellationToken)
    {
        if (session.AuthenticatedUser is not null && session.CurrentRoomId is { } roomId
            && _roomManager.TryGetLoadedRoom(roomId, out Room? room) && _transfers.IsInfobus(room!))
        {
            _transfers.SendToPark(session);
        }

        return ValueTask.CompletedTask;
    }
}
