using DragonEmu.Domain;
using DragonEmu.Game.Fuses;
using DragonEmu.Persistence;

namespace DragonEmu.Game.Rooms;

public sealed class RoomRightsService
{
    public const string AnyRoomControllerFuse = "fuse_any_room_controller";

    public const string AdministratorAccessFuse = "fuse_administrator_access";

    public const string PickUpAnyFurniFuse = "fuse_pick_up_any_furni";

    public const string RemoveStickiesFuse = "fuse_remove_stickies";

    private readonly FuseService _fuses;
    private readonly IRoomRightsRepository _roomRightsRepository;

    public RoomRightsService(FuseService fuses, IRoomRightsRepository roomRightsRepository)
    {
        _fuses = fuses;
        _roomRightsRepository = roomRightsRepository;
    }

    public static bool IsOwner(UserAccount account, PrivateRoomSummary room) => room.OwnerId == account.Id;

    public async Task<bool> IsControllerOfOwnerlessRoomAsync(
        UserAccount account, PrivateRoomSummary room, CancellationToken cancellationToken = default) =>
        room.OwnerId is null && await HoldsAnyRoomControllerFuseAsync(account, cancellationToken);

    public Task<bool> HoldsAnyRoomControllerFuseAsync(UserAccount account, CancellationToken cancellationToken = default) =>
        HoldsFuseAsync(account, AnyRoomControllerFuse, cancellationToken);

    public Task<bool> HoldsFuseAsync(UserAccount account, string fuse, CancellationToken cancellationToken = default) =>
        _fuses.HoldsAsync(account, fuse, room: null, cancellationToken);

    public Task BanAsync(int userId, int roomId, int bannedById, int seconds, CancellationToken cancellationToken = default) =>
        _roomRightsRepository.BanAsync(userId, roomId, bannedById, seconds, cancellationToken);

    public Task<bool> IsBannedAsync(int userId, int roomId, CancellationToken cancellationToken = default) =>
        _roomRightsRepository.IsBannedAsync(userId, roomId, cancellationToken);

    public Task<bool> HasGrantedRightsAsync(int userId, int roomId, CancellationToken cancellationToken = default) =>
        _roomRightsRepository.HasRightsAsync(userId, roomId, cancellationToken);

    public async Task<bool> CanEditAsync(
        UserAccount account, PrivateRoomSummary room, CancellationToken cancellationToken = default) =>
        IsOwner(account, room) || await IsControllerOfOwnerlessRoomAsync(account, room, cancellationToken);

    public async Task<bool> CanUseFurnitureAsync(
        UserAccount account, PrivateRoomSummary room, CancellationToken cancellationToken = default) =>
        await CanEditAsync(account, room, cancellationToken) || await HasGrantedRightsAsync(account.Id, room.Id, cancellationToken);

    public async Task<bool> CanToggleFurnitureAsync(
        UserAccount account, PrivateRoomSummary room, CancellationToken cancellationToken = default) =>
        IsOwner(account, room)
        || await HasGrantedRightsAsync(account.Id, room.Id, cancellationToken)
        || await HoldsAnyRoomControllerFuseAsync(account, cancellationToken);

    public async Task<bool> CanPickUpAsync(
        UserAccount account, PrivateRoomSummary room, bool isWallItem, CancellationToken cancellationToken = default) =>
        IsOwner(account, room)
        || (isWallItem && await HasGrantedRightsAsync(account.Id, room.Id, cancellationToken))
        || await _fuses.HoldsAsync(account, PickUpAnyFurniFuse, room, cancellationToken);

    public Task<bool> CanRemoveStickiesAsync(UserAccount account, PrivateRoomSummary room, CancellationToken cancellationToken = default) =>
        _fuses.HoldsAsync(account, RemoveStickiesFuse, room, cancellationToken);

    public async Task<bool> CanManageStickiesAsync(
        UserAccount account, PrivateRoomSummary room, CancellationToken cancellationToken = default) =>
        IsOwner(account, room) || await HoldsAnyRoomControllerFuseAsync(account, cancellationToken);

    public async Task<bool> CanWriteOnStickiesAsync(
        UserAccount account, PrivateRoomSummary room, CancellationToken cancellationToken = default) =>
        await CanManageStickiesAsync(account, room, cancellationToken) || await HasGrantedRightsAsync(account.Id, room.Id, cancellationToken);

    public Task<bool> CanExceedRoomLimitsAsync(UserAccount account, CancellationToken cancellationToken = default) =>
        HoldsFuseAsync(account, AdministratorAccessFuse, cancellationToken);
}
