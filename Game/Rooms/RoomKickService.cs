using DragonEmu.Domain;
using DragonEmu.Game.Settings;
using DragonEmu.Gateway;
using DragonEmu.Protocol.Messages;
using DragonEmu.Protocol.Messages.Outgoing.Handshake;
using DragonEmu.Protocol.Messages.Outgoing.RoomEngine;

namespace DragonEmu.Game.Rooms;

public enum KickResult
{
    Kicked,
    NotInARoom,
    NotAllowed,
    NotHere,
    CantBeKicked,
}

public sealed class RoomKickService
{
    public const int KickedErrorCode = 4008;

    public const string ModeratorAccessFuse = "fuse_moderator_access";

    private static readonly string[] UnkickableFuses =
        [RoomRightsService.AnyRoomControllerFuse, ModeratorAccessFuse, RoomRightsService.AdministratorAccessFuse];

    private readonly IRoomManager _roomManager;
    private readonly RoomRightsService _roomRights;
    private readonly ClientSessionManager _sessionManager;
    private readonly RoomMovementScheduler _movementScheduler;
    private readonly GameSettingsCache _gameSettings;
    private readonly RoomBroadcaster _broadcaster;

    public RoomKickService(
        IRoomManager roomManager, RoomRightsService roomRights, ClientSessionManager sessionManager, RoomMovementScheduler movementScheduler,
        GameSettingsCache gameSettings, RoomBroadcaster broadcaster)
    {
        _roomManager = roomManager;
        _roomRights = roomRights;
        _sessionManager = sessionManager;
        _movementScheduler = movementScheduler;
        _gameSettings = gameSettings;
        _broadcaster = broadcaster;
    }

    public Task<KickResult> KickAsync(IGameSession session, UserAccount account, int targetUserId, CancellationToken cancellationToken) =>
        PutOutAsync(session, account, actor => actor.AccountId == targetUserId, ban: false, cancellationToken);

    public Task<KickResult> KickAsync(IGameSession session, UserAccount account, string targetUsername, CancellationToken cancellationToken) =>
        PutOutAsync(session, account, actor => string.Equals(actor.Username, targetUsername, StringComparison.OrdinalIgnoreCase), ban: false,
            cancellationToken);

    public Task<KickResult> BanAsync(IGameSession session, UserAccount account, int targetUserId, CancellationToken cancellationToken) =>
        PutOutAsync(session, account, actor => actor.AccountId == targetUserId, ban: true, cancellationToken);

    public async Task<int?> KickEveryoneAsync(IGameSession session, CancellationToken cancellationToken) =>
        RoomOf(session) is { } room ? await KickEveryoneAsync(room, cancellationToken) : null;

    public async Task<int> KickEveryoneAsync(Room room, CancellationToken cancellationToken)
    {
        int kicked = 0;
        foreach (RoomActor actor in room.Actors)
        {
            if (actor.Type == ActorType.Player && PlayerOf(actor) is { } target
                && !await HoldsUnkickableFuseAsync(target.Account, cancellationToken) && PutOut(room, actor, target.Session))
            {
                kicked++;
            }
        }

        return kicked;
    }

    public async Task<bool> CanKickAsync(UserAccount account, Room room, CancellationToken cancellationToken) =>
        RoomRightsService.IsOwner(account, room.Data)
        || await _roomRights.HasGrantedRightsAsync(account.Id, room.Id, cancellationToken)
        || await _roomRights.HoldsAnyRoomControllerFuseAsync(account, cancellationToken)
        || await _roomRights.HoldsFuseAsync(account, ModeratorAccessFuse, cancellationToken);

    public async Task<bool> CanBanAsync(UserAccount account, Room room, CancellationToken cancellationToken) =>
        RoomRightsService.IsOwner(account, room.Data) || await _roomRights.HoldsAnyRoomControllerFuseAsync(account, cancellationToken);

    public async Task<KickResult> SoftKickAsync(IGameSession session, UserAccount account, string targetUsername, CancellationToken cancellationToken)
    {
        (KickResult result, Room? room, RoomActor? actor, _) = await FindTargetAsync(
            session, account, actor => string.Equals(actor.Username, targetUsername, StringComparison.OrdinalIgnoreCase), ban: false, cancellationToken);
        if (result != KickResult.Kicked)
        {
            return result;
        }

        PrivateRoomSummary door = room!.Data;
        room.TeleportActor(actor!, door.DoorX, door.DoorY, door.DoorZ);
        lock (actor!.Lock)
        {
            actor.RotBody = door.DoorDir;
            actor.RotHead = door.DoorDir;
        }

        _broadcaster.Broadcast(room, new UserUpdateMessageEvent([actor]).Compose());
        return KickResult.Kicked;
    }

    private async Task<KickResult> PutOutAsync(
        IGameSession session, UserAccount account, Func<RoomActor, bool> isTarget, bool ban, CancellationToken cancellationToken)
    {
        (KickResult result, Room? room, RoomActor? actor, (IGameSession Session, UserAccount Account)? found) =
            await FindTargetAsync(session, account, isTarget, ban, cancellationToken);
        if (result != KickResult.Kicked)
        {
            return result;
        }

        (IGameSession Session, UserAccount Account) target = found!.Value;

        if (ban)
        {
            await _roomRights.BanAsync(
                target.Account.Id, room!.Id, account.Id, Math.Max(0, _gameSettings.Current.RoomBanDefaultTime), cancellationToken);
        }

        PutOut(room!, actor!, target.Session);
        return KickResult.Kicked;
    }

    private async Task<(KickResult Result, Room? Room, RoomActor? Actor, (IGameSession Session, UserAccount Account)? Target)> FindTargetAsync(
        IGameSession session, UserAccount account, Func<RoomActor, bool> isTarget, bool ban, CancellationToken cancellationToken)
    {
        if (RoomOf(session) is not { } room)
        {
            return (KickResult.NotInARoom, null, null, null);
        }

        if (!await (ban ? CanBanAsync(account, room, cancellationToken) : CanKickAsync(account, room, cancellationToken)))
        {
            return (KickResult.NotAllowed, room, null, null);
        }

        if (room.Actors.FirstOrDefault(actor => actor.Type == ActorType.Player && isTarget(actor)) is not { } actor
            || PlayerOf(actor) is not { } target)
        {
            return (KickResult.NotHere, room, null, null);
        }

        if (target.Account.Id == account.Id
            || (RoomRightsService.IsOwner(target.Account, room.Data) && (ban || !await IsStaffAsync(account, cancellationToken)))
            || await HoldsUnkickableFuseAsync(target.Account, cancellationToken))
        {
            return (KickResult.CantBeKicked, room, actor, target);
        }

        return (KickResult.Kicked, room, actor, target);
    }

    public bool PutOutQuietly(Room room, RoomActor actor) =>
        PlayerOf(actor) is { } player && PutOut(room, actor, player.Session, alert: false);

    public async Task<bool> PutOutIdleAsync(Room room, RoomActor actor, CancellationToken cancellationToken) =>
        PlayerOf(actor) is { } player && !await IsStaffAsync(player.Account, cancellationToken)
        && PutOut(room, actor, player.Session, alert: false);

    private bool PutOut(Room room, RoomActor actor, IGameSession session, bool alert = true)
    {
        lock (actor.Lock)
        {
            if (actor.IsKicked)
            {
                return false;
            }
        }

        if (alert)
        {
            session.Send(new GenericErrorEvent(KickedErrorCode).Compose());
        }

        if (!room.StartKick(actor))
        {
            return false;
        }

        _movementScheduler.MarkActive(room);
        return true;
    }

    private Room? RoomOf(IGameSession session) =>
        session.CurrentRoomId is { } roomId && _roomManager.TryGetLoadedRoom(roomId, out Room? room) ? room : null;

    private (IGameSession Session, UserAccount Account)? PlayerOf(RoomActor actor) =>
        _sessionManager.TryGet(actor.SessionId, out ClientSession? session) && session!.AuthenticatedUser is { } account
            ? (session, account)
            : null;

    private async Task<bool> IsStaffAsync(UserAccount account, CancellationToken cancellationToken) =>
        await _roomRights.HoldsFuseAsync(account, ModeratorAccessFuse, cancellationToken)
        || await _roomRights.HoldsFuseAsync(account, RoomRightsService.AdministratorAccessFuse, cancellationToken);

    private async Task<bool> HoldsUnkickableFuseAsync(UserAccount account, CancellationToken cancellationToken)
    {
        foreach (string fuse in UnkickableFuses)
        {
            if (await _roomRights.HoldsFuseAsync(account, fuse, cancellationToken))
            {
                return true;
            }
        }

        return false;
    }
}
