using DragonEmu.Domain;
using DragonEmu.Game.Settings;
using DragonEmu.Gateway;
using DragonEmu.Protocol.Messages;
using DragonEmu.Protocol.Messages.Outgoing.RoomPublicRoom;
using DragonEmu.Protocol.Messages.Outgoing.RoomSession;

namespace DragonEmu.Game.Rooms;

public sealed class PublicRoomTransfers
{
    public static readonly (int X, int Y) BusEntranceTile = (28, 5);

    public static readonly (int X, int Y, int Rotation) ParkArrivalTile = (14, 6, 4);

    private static readonly TimeSpan ArrivalWindow = TimeSpan.FromSeconds(30);

    private readonly PublicRoomCache _publicRooms;
    private readonly GameSettingsCache _gameSettings;
    private readonly ClientSessionManager _sessions;
    private readonly IRoomManager _roomManager;

    public PublicRoomTransfers(PublicRoomCache publicRooms, GameSettingsCache gameSettings, ClientSessionManager sessions, IRoomManager roomManager)
    {
        _publicRooms = publicRooms;
        _gameSettings = gameSettings;
        _sessions = sessions;
        _roomManager = roomManager;
    }

    public bool IsInfobus(Room room) => _gameSettings.Current.InfobusRoomRef == room.Id;

    public bool CanBoardBus(Room room, RoomActor actor)
    {
        if (room.Data.HeightmapLabel != PublicRoomCache.ParkHeightmapLabel)
        {
            return false;
        }

        lock (actor.Lock)
        {
            return (actor.X, actor.Y) == BusEntranceTile && !actor.IsWalking && actor.PendingStep is null;
        }
    }

    public void TryBoard(IGameSession session)
    {
        if (!_gameSettings.Current.InfobusOpen || _gameSettings.Current.InfobusRoomRef is null)
        {
            session.Send(new ParkBusCannotEnterMessageEvent().Compose());
            return;
        }

        Board(session);
    }

    public void Board(IGameSession session)
    {
        if (_gameSettings.Current.InfobusRoomRef is { } busRoomId)
        {
            session.NoInterstitialUntil = DateTime.UtcNow + ArrivalWindow;
            session.Send(new RoomForwardMessageEvent(isPublic: _publicRooms.IsPublic(busRoomId), busRoomId).Compose());
        }
    }

    public void Warp(IGameSession session, PublicRoomWarpTile warp, int rotation) =>
        SendTo(session, warp.TargetRoomId, warp.TargetX, warp.TargetY, rotation);

    public void SendToPark(IGameSession session)
    {
        if (_publicRooms.ParkRoomId is { } parkRoomId)
        {
            SendTo(session, parkRoomId, ParkArrivalTile.X, ParkArrivalTile.Y, ParkArrivalTile.Rotation);
        }
    }

    public void EmptyInfobus()
    {
        if (_gameSettings.Current.InfobusRoomRef is not { } busRoomId || !_roomManager.TryGetLoadedRoom(busRoomId, out Room? bus))
        {
            return;
        }

        foreach (RoomActor actor in bus!.Actors)
        {
            if (actor.Type == ActorType.Player && _sessions.TryGet(actor.SessionId, out ClientSession? session) && session!.CurrentRoomId == busRoomId)
            {
                SendToPark(session);
            }
        }
    }

    private void SendTo(IGameSession session, int roomId, int x, int y, int rotation)
    {
        session.PendingArrival = new PendingArrival(roomId, x, y, rotation, DateTime.UtcNow + ArrivalWindow);
        session.NoInterstitialUntil = DateTime.UtcNow + ArrivalWindow;
        session.Send(new RoomForwardMessageEvent(isPublic: _publicRooms.IsPublic(roomId), roomId).Compose());
    }
}
