using DragonEmu.Game.Rooms;
using DragonEmu.Game.Settings;
using DragonEmu.Protocol.Messages.Outgoing.RoomPublicRoom;

namespace DragonEmu.Protocol.Messages.Incoming.RoomPublicRoom;

public sealed class TryBusMessageComposer : IMessageComposer
{
    private readonly IRoomManager _roomManager;
    private readonly GameSettingsCache _gameSettings;
    private readonly PublicRoomTransfers _transfers;

    public TryBusMessageComposer(IRoomManager roomManager, GameSettingsCache gameSettings, PublicRoomTransfers transfers)
    {
        _roomManager = roomManager;
        _gameSettings = gameSettings;
        _transfers = transfers;
    }

    public void Parse(ClientPacket packet)
    {
    }

    public ValueTask HandleAsync(IGameSession session, CancellationToken cancellationToken)
    {
        if (session.AuthenticatedUser is null || session.CurrentRoomId is not { } roomId
            || !_roomManager.TryGetLoadedRoom(roomId, out Room? room) || !room!.TryGetActor(session.Id, out RoomActor? actor)
            || !_transfers.CanBoardBus(room, actor!))
        {
            return ValueTask.CompletedTask;
        }

        _transfers.TryBoard(session);
        return ValueTask.CompletedTask;
    }
}
