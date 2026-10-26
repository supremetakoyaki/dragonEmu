using DragonEmu.Game.Rooms;
using DragonEmu.Game.Fuses;
using DragonEmu.Protocol.Messages.Outgoing.RoomAction;

namespace DragonEmu.Protocol.Messages.Incoming.RoomAvatar;

public sealed class DanceMessageComposer : IMessageComposer
{
    private const int MaxFreeDanceId = 1;
    private const int MaxClubDanceId = 4;

    private readonly IRoomManager _roomManager;
    private const string ClubDanceFuse = "fuse_use_club_dance";

    private readonly FuseService _fuses;
    private readonly RoomBroadcaster _broadcaster;

    private int _danceId;

    public DanceMessageComposer(IRoomManager roomManager, FuseService fuses, RoomBroadcaster broadcaster)
    {
        _roomManager = roomManager;
        _fuses = fuses;
        _broadcaster = broadcaster;
    }

    public void Parse(ClientPacket packet)
    {
        _danceId = packet.PopInt32();
    }

    public async ValueTask HandleAsync(IGameSession session, CancellationToken cancellationToken)
    {
        if (session.AuthenticatedUser is not { } account || session.CurrentRoomId is not { } roomId)
        {
            return;
        }

        Room? room = await _roomManager.GetOrLoadRoomAsync(roomId, cancellationToken);
        if (room is null || !room.TryGetActor(session.Id, out RoomActor? actor))
        {
            return;
        }

        int danceId = _danceId;
        if (danceId is < 0 or > MaxClubDanceId)
        {
            danceId = 0;
        }
        else if (danceId > MaxFreeDanceId && !await _fuses.HoldsAsync(account, ClubDanceFuse, cancellationToken: cancellationToken))
        {
            danceId = 0;
        }

        bool putsDownHandItem;
        lock (actor!.Lock)
        {
            actor.DanceId = danceId;

            putsDownHandItem = danceId > 0 && actor.CarryItemId > 0;
            if (putsDownHandItem)
            {
                actor.CarryItemId = 0;
            }
        }

        if (putsDownHandItem)
        {
            _broadcaster.Broadcast(room, new CarryObjectMessageEvent(actor.VirtualId, 0).Compose());
        }

        _broadcaster.Broadcast(room, new DanceMessageEvent(actor.VirtualId, danceId).Compose());
    }
}
