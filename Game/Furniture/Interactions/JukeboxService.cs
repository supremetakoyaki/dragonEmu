using DragonEmu.Domain;
using DragonEmu.Game.Rooms;
using DragonEmu.Gateway;
using DragonEmu.Persistence;
using DragonEmu.Protocol;
using DragonEmu.Protocol.Messages;
using DragonEmu.Protocol.Messages.Outgoing.InventoryFurni;
using DragonEmu.Protocol.Messages.Outgoing.Trax;

namespace DragonEmu.Game.Furniture;

public sealed class JukeboxService
{
    public const int DiskSlots = 10;
    public const int PlaylistLimit = 9;

    private readonly IRoomManager _roomManager;
    private readonly RoomRightsService _rights;
    private readonly ITraxRepository _trax;
    private readonly TraxService _playback;
    private readonly IUserItemRepository _userItems;
    private readonly RoomBroadcaster _broadcaster;
    private readonly ClientSessionManager _sessions;

    public JukeboxService(
        IRoomManager roomManager, RoomRightsService rights, ITraxRepository trax, TraxService playback, IUserItemRepository userItems,
        RoomBroadcaster broadcaster, ClientSessionManager sessions)
    {
        _roomManager = roomManager;
        _rights = rights;
        _trax = trax;
        _playback = playback;
        _userItems = userItems;
        _broadcaster = broadcaster;
        _sessions = sessions;
    }

    public async Task SelectAsync(IGameSession session, UserAccount account, Room room, RoomFloorItem jukebox, CancellationToken cancellationToken) =>
        session.Send(new JukeboxSelectedMessageEvent(jukebox.Id, await _rights.CanUseFurnitureAsync(account, room.Data, cancellationToken)).Compose());

    public async Task SendDisksAsync(IGameSession session, CancellationToken cancellationToken)
    {
        if (await ResolveAsync(session, manage: false, cancellationToken) is { } use)
        {
            session.Send(await DisksMessageAsync(use.Room, use.Jukebox, cancellationToken));
        }
    }

    public async Task SendUserDisksAsync(IGameSession session, CancellationToken cancellationToken)
    {
        if (await ResolveAsync(session, manage: true, cancellationToken) is { } use)
        {
            session.Send(new UserSongDisksMessageEvent(await _trax.GetInventoryDisksAsync(use.Account.Id, cancellationToken)).Compose());
        }
    }

    public async Task LoadDiskAsync(IGameSession session, int diskItemId, int slot, CancellationToken cancellationToken)
    {
        if (await ResolveAsync(session, manage: true, cancellationToken) is not { } use || slot is < 1 or > DiskSlots)
        {
            return;
        }

        if (await _trax.LoadDiskAsync(use.Jukebox.Id, slot, use.Account.Id, diskItemId, cancellationToken))
        {
            session.Send(new FurniListRemoveEvent(diskItemId).Compose());
            await BroadcastDisksAsync(use, cancellationToken);
        }

        session.Send(new UserSongDisksMessageEvent(await _trax.GetInventoryDisksAsync(use.Account.Id, cancellationToken)).Compose());
    }

    public async Task EjectDiskAsync(IGameSession session, int slot, CancellationToken cancellationToken)
    {
        if (await ResolveAsync(session, manage: true, cancellationToken) is not { } use
            || await _trax.UnloadDiskAsync(use.Jukebox.Id, slot, cancellationToken) is not { } disk)
        {
            return;
        }

        if (_sessions.TryGetByUserId(disk.OwnerId, out ClientSession? owner) && await _userItems.GetByIdAsync(disk.ItemId, cancellationToken) is { } item)
        {
            owner!.EnqueueSend(new FurniListInsertEvent(item).Compose());
        }

        await _playback.PlaylistChangedAsync(use.Room, use.Jukebox, cancellationToken);
        await BroadcastDisksAsync(use, cancellationToken);
        if (disk.OwnerId == use.Account.Id)
        {
            session.Send(new UserSongDisksMessageEvent(await _trax.GetInventoryDisksAsync(use.Account.Id, cancellationToken)).Compose());
        }
    }

    public async Task QueueAsync(IGameSession session, int songId, CancellationToken cancellationToken)
    {
        if (await ResolveAsync(session, manage: false, cancellationToken) is not { } use
            || (await _trax.GetJukeboxDisksAsync(use.Jukebox.Id, cancellationToken)).FirstOrDefault(loaded => loaded.Song.Id == songId) is not { } disk)
        {
            return;
        }

        List<int> playlist = [.. (await _trax.GetJukeboxPlaylistDiskIdsAsync(use.Jukebox.Id, cancellationToken)).Where(id => id != disk.ItemId)];
        if (playlist.Count >= PlaylistLimit)
        {
            session.Send(new JukeboxPlaylistFullMessageEvent(PlaylistLimit).Compose());
            return;
        }

        playlist.Add(disk.ItemId);
        await _trax.SetJukeboxPlaylistAsync(use.Jukebox.Id, playlist, cancellationToken);
        if (use.Room.Playback?.MachineItemId == use.Jukebox.Id)
        {
            await _playback.PlaylistChangedAsync(use.Room, use.Jukebox, cancellationToken);
        }
        else
        {
            await _playback.SwitchAsync(use.Room, use.Jukebox, on: true, cancellationToken);
        }

        await BroadcastDisksAsync(use, cancellationToken);
    }

    public async Task ResetAsync(IGameSession session, CancellationToken cancellationToken)
    {
        if (await ResolveAsync(session, manage: true, cancellationToken) is { } use)
        {
            await _trax.SetJukeboxPlaylistAsync(use.Jukebox.Id, [], cancellationToken);
            await _playback.SwitchAsync(use.Room, use.Jukebox, on: false, cancellationToken);
            await BroadcastDisksAsync(use, cancellationToken);
        }
    }

    private async Task<ServerPacket> DisksMessageAsync(Room room, RoomFloorItem jukebox, CancellationToken cancellationToken)
    {
        IReadOnlyList<JukeboxDisk> disks = await _trax.GetJukeboxDisksAsync(jukebox.Id, cancellationToken);
        Dictionary<int, int> songOfDisk = disks.ToDictionary(disk => disk.ItemId, disk => disk.Song.Id);
        List<int> playlist = [.. (await _trax.GetJukeboxPlaylistDiskIdsAsync(jukebox.Id, cancellationToken))
            .Where(songOfDisk.ContainsKey)
            .Select(diskId => songOfDisk[diskId])];
        int position = room.Playback is { } playback && playback.MachineItemId == jukebox.Id ? playback.PositionSeconds(DateTime.UtcNow) : -1;
        return new JukeboxDisksMessageEvent(DiskSlots, disks, position, playlist).Compose();
    }

    private async Task BroadcastDisksAsync(Using use, CancellationToken cancellationToken) =>
        _broadcaster.Broadcast(use.Room, await DisksMessageAsync(use.Room, use.Jukebox, cancellationToken));

    private async Task<Using?> ResolveAsync(IGameSession session, bool manage, CancellationToken cancellationToken)
    {
        if (session.AuthenticatedUser is not { } account || session.CurrentRoomId is not { } roomId
            || !_roomManager.TryGetLoadedRoom(roomId, out Room? room)
            || room!.SoundFurni is not { LogicClass: FurnitureLogicClass.Jukebox } jukebox
            || (manage && !await _rights.CanUseFurnitureAsync(account, room.Data, cancellationToken)))
        {
            return null;
        }

        return new Using(account, room, jukebox);
    }

    private sealed record Using(UserAccount Account, Room Room, RoomFloorItem Jukebox);
}
