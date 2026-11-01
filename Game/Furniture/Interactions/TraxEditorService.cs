using DragonEmu.Domain;
using DragonEmu.Game.Rooms;
using DragonEmu.Gateway;
using DragonEmu.Persistence;
using DragonEmu.Protocol.Messages;
using DragonEmu.Protocol.Messages.Outgoing.Catalog;
using DragonEmu.Protocol.Messages.Outgoing.InventoryFurni;
using DragonEmu.Protocol.Messages.Outgoing.InventoryPurse;
using DragonEmu.Protocol.Messages.Outgoing.Trax;

namespace DragonEmu.Game.Furniture;

public sealed class TraxEditorService
{
    public const int SoundSetSlots = 4;
    public const int PlaylistLimit = 5;
    public const int MaxSongs = 50;
    public const int BurnCost = 1;
    public const string SongDiskClassname = "song_disk";

    private readonly IRoomManager _roomManager;
    private readonly RoomRightsService _rights;
    private readonly ITraxRepository _trax;
    private readonly TraxDefinitionCache _definitions;
    private readonly FurnitureDefinitionCache _furniture;
    private readonly TraxService _playback;
    private readonly IUserItemRepository _userItems;
    private readonly ICurrencyRepository _currency;
    private readonly ClientSessionManager _sessions;

    public TraxEditorService(
        IRoomManager roomManager, RoomRightsService rights, ITraxRepository trax, TraxDefinitionCache definitions,
        FurnitureDefinitionCache furniture, TraxService playback, IUserItemRepository userItems, ICurrencyRepository currency,
        ClientSessionManager sessions)
    {
        _roomManager = roomManager;
        _rights = rights;
        _trax = trax;
        _definitions = definitions;
        _furniture = furniture;
        _playback = playback;
        _userItems = userItems;
        _currency = currency;
        _sessions = sessions;
    }

    public async Task<bool> MayEditAsync(UserAccount account, Room room, CancellationToken cancellationToken) =>
        RoomRightsService.IsOwner(account, room.Data) || await _rights.HoldsAnyRoomControllerFuseAsync(account, cancellationToken);

    public async Task InventoryChangedAsync(IGameSession session, int furniDefinitionId, CancellationToken cancellationToken)
    {
        if (session.TraxClient && session.AuthenticatedUser is { } account && _definitions.GetSoundSetByFurni(furniDefinitionId) is not null)
        {
            await SendUserSoundSetsAsync(session, account.Id, cancellationToken);
        }
    }

    public async Task<IReadOnlySet<int>> HoldersAsync(int machineItemId, CancellationToken cancellationToken)
    {
        IReadOnlyList<TraxMachineSoundSet> sets = await _trax.GetMachineSoundSetsAsync(machineItemId, cancellationToken);
        IReadOnlyList<JukeboxDisk> disks = await _trax.GetJukeboxDisksAsync(machineItemId, cancellationToken);
        return sets.Select(set => set.OwnerId).Concat(disks.Select(disk => disk.OwnerId)).ToHashSet();
    }

    public void TellReturned(IReadOnlySet<int> holders)
    {
        foreach (int holder in holders)
        {
            if (_sessions.TryGetByUserId(holder, out ClientSession? owner))
            {
                owner!.EnqueueSend(new FurniListUpdateEvent().Compose());
            }
        }
    }

    public Task<int> BurnPrebuiltAsync(int userId, int songDiskDefinitionId, PrebuiltSong song, CancellationToken cancellationToken) =>
        _trax.BurnPrebuiltAsync(userId, songDiskDefinitionId, $"{song.Name}\t{song.Author}", song, cancellationToken);

    public async Task SelectAsync(IGameSession session, RoomFloorItem machine, CancellationToken cancellationToken) =>
        session.Send(new SoundMachineSelectedMessageEvent(machine.Id, await _playback.IsOnAsync(machine, cancellationToken)).Compose());

    public async Task SetStateAsync(IGameSession session, int itemId, bool on, CancellationToken cancellationToken)
    {
        if (await ResolveAsync(session, cancellationToken) is { } editing && editing.Machine.Id == itemId)
        {
            await _playback.SwitchAsync(editing.Room, editing.Machine, on, cancellationToken);
        }
    }

    public async Task NewSongAsync(IGameSession session, CancellationToken cancellationToken)
    {
        if (await ResolveAsync(session, cancellationToken) is { } editing)
        {
            await SendSoundSetsAsync(session, editing, cancellationToken);
        }
    }

    public async Task EditSongAsync(IGameSession session, int songId, CancellationToken cancellationToken)
    {
        if (await ResolveAsync(session, cancellationToken) is not { } editing
            || await _trax.GetSongAsync(songId, cancellationToken) is not { } song || song.UserId != editing.Account.Id)
        {
            return;
        }

        IReadOnlyList<TraxMachineSoundSet> inserted = await _trax.GetMachineSoundSetsAsync(editing.Machine.Id, cancellationToken);
        HashSet<int> insertedSets = [.. inserted.Select(set => set.SoundSetId)];
        List<TraxSoundSet> missing = [.. TraxSongRules.SamplesIn(song.Data)
            .Select(sampleId => _definitions.GetSample(sampleId)?.SoundSetId ?? 0)
            .Where(setId => setId != 0 && !insertedSets.Contains(setId))
            .Distinct()
            .Order()
            .Select(setId => _definitions.GetSoundSet(setId)!)];
        if (missing.Count > 0)
        {
            session.Send(new SongMissingPackagesMessageEvent(missing).Compose());
            return;
        }

        if (song.Burnt)
        {
            session.Send(SongLockedMessageEvent.Instance.Compose());
        }

        session.Send(new EditorSongMessageEvent(song.ToSong()).Compose());
        await SendSoundSetsAsync(session, editing, cancellationToken);
    }

    public async Task InsertSoundSetAsync(IGameSession session, int soundSetId, int slot, CancellationToken cancellationToken)
    {
        if (await ResolveAsync(session, cancellationToken) is not { } editing || _definitions.GetSoundSet(soundSetId) is null)
        {
            return;
        }

        IReadOnlyList<TraxMachineSoundSet> inserted = await _trax.GetMachineSoundSetsAsync(editing.Machine.Id, cancellationToken);
        if (slot is < 1 or > SoundSetSlots || inserted.Any(set => set.Slot == slot))
        {
            slot = Enumerable.Range(1, SoundSetSlots).FirstOrDefault(free => inserted.All(set => set.Slot != free));
        }

        if (slot != 0 && await _trax.InsertSoundSetAsync(editing.Machine.Id, slot, editing.Account.Id, soundSetId, cancellationToken) is { } itemId)
        {
            session.Send(new FurniListRemoveEvent(itemId).Compose());
        }

        await SendSoundSetsAsync(session, editing, cancellationToken);
    }

    public async Task EjectSoundSetAsync(IGameSession session, int slot, CancellationToken cancellationToken)
    {
        if (await ResolveAsync(session, cancellationToken) is not { } editing)
        {
            return;
        }

        if (await _trax.EjectSoundSetAsync(editing.Machine.Id, slot, cancellationToken) is { } ejected)
        {
            await ReturnToOwnerAsync(ejected.ItemId, ejected.OwnerId, cancellationToken);
        }

        await SendSoundSetsAsync(session, editing, cancellationToken);
    }

    public async Task SendSongListAsync(IGameSession session, CancellationToken cancellationToken)
    {
        if (await ResolveAsync(session, cancellationToken) is { } editing)
        {
            await SendSongListAsync(session, editing, cancellationToken);
        }
    }

    public async Task SaveNewAsync(IGameSession session, string name, string data, CancellationToken cancellationToken)
    {
        if (await ResolveAsync(session, cancellationToken) is not { } editing
            || TraxSongRules.CleanName(name) is not { } cleanName
            || !await IsValidAsync(editing, data, cancellationToken))
        {
            return;
        }

        if (await _trax.CountSongsAsync(editing.Account.Id, cancellationToken) >= MaxSongs)
        {
            session.Send(new SongListFullMessageEvent(MaxSongs).Compose());
            return;
        }

        int songId = await _trax.CreateSongAsync(editing.Account.Id, cleanName, editing.Account.Username, data, cancellationToken);
        session.Send(new NewSongMessageEvent(songId, cleanName).Compose());
        await SendSongListAsync(session, editing, cancellationToken);
    }

    public async Task SaveEditAsync(IGameSession session, int songId, string name, string data, CancellationToken cancellationToken)
    {
        if (await ResolveAsync(session, cancellationToken) is not { } editing
            || await _trax.GetSongAsync(songId, cancellationToken) is not { } song || song.UserId != editing.Account.Id
            || TraxSongRules.CleanName(name) is not { } cleanName
            || !await IsValidAsync(editing, data, cancellationToken))
        {
            return;
        }

        if (song.Burnt)
        {
            if (cleanName == song.Name)
            {
                session.Send(SongLockedMessageEvent.Instance.Compose());
                return;
            }

            if (await _trax.CountSongsAsync(editing.Account.Id, cancellationToken) >= MaxSongs)
            {
                session.Send(new SongListFullMessageEvent(MaxSongs).Compose());
                return;
            }

            songId = await _trax.CreateSongAsync(editing.Account.Id, cleanName, editing.Account.Username, data, cancellationToken);
        }
        else
        {
            await _trax.UpdateSongAsync(songId, cleanName, data, cancellationToken);
            await _playback.PlaylistChangedAsync(editing.Room, editing.Machine, cancellationToken);
        }

        session.Send(new SongSavedMessageEvent(songId, cleanName).Compose());
        await SendSongListAsync(session, editing, cancellationToken);
    }

    public async Task SavePlaylistAsync(IGameSession session, IReadOnlyList<int> songIds, CancellationToken cancellationToken)
    {
        if (await ResolveAsync(session, cancellationToken) is not { } editing)
        {
            return;
        }

        if (songIds.Count > PlaylistLimit)
        {
            session.Send(new PlayListInvalidMessageEvent(PlaylistLimit).Compose());
            return;
        }

        HashSet<int> own = [.. (await _trax.GetSongListAsync(editing.Account.Id, cancellationToken)).Select(song => song.Id)];
        await _trax.SetPlaylistAsync(editing.Machine.Id, [.. songIds.Where(own.Contains).Distinct()], cancellationToken);
        await _playback.PlaylistChangedAsync(editing.Room, editing.Machine, cancellationToken);
        await SendSongListAsync(session, editing, cancellationToken);
    }

    public async Task DeleteSongAsync(IGameSession session, int songId, CancellationToken cancellationToken)
    {
        if (await ResolveAsync(session, cancellationToken) is not { } editing
            || await _trax.GetSongAsync(songId, cancellationToken) is not { } song || song.UserId != editing.Account.Id)
        {
            return;
        }

        await _trax.DeleteSongAsync(songId, cancellationToken);
        await _playback.PlaylistChangedAsync(editing.Room, editing.Machine, cancellationToken);
        await SendSongListAsync(session, editing, cancellationToken);
    }

    public async Task BurnAsync(IGameSession session, int songId, CancellationToken cancellationToken)
    {
        if (await ResolveAsync(session, cancellationToken) is not { } editing
            || await _trax.GetSongAsync(songId, cancellationToken) is not { } song || song.UserId != editing.Account.Id
            || _furniture.GetByClassname(SongDiskClassname) is not { } songDisk)
        {
            return;
        }

        if (TraxSongRules.SamplesIn(song.Data).Count == 0)
        {
            session.Send(InvalidSongLengthMessageEvent.Instance.Compose());
            return;
        }

        if (await _trax.HasBurntSongNamedAsync(editing.Account.Id, song.Name, song.Id, cancellationToken))
        {
            session.Send(InvalidSongNameMessageEvent.Instance.Compose());
            return;
        }

        if (await _currency.GetBalanceAsync(editing.Account.Id, CurrencyType.Credits, cancellationToken) < BurnCost)
        {
            session.Send(new NotEnoughBalanceMessageEvent(notEnoughCredits: true, notEnoughPixels: false).Compose());
            return;
        }

        long balance = await _currency.AddBalanceAsync(editing.Account.Id, CurrencyType.Credits, -BurnCost, cancellationToken);
        session.Send(new CreditBalanceEvent(balance).Compose());

        int diskId = await _trax.BurnAsync(editing.Account.Id, songDisk.Id, $"{song.Name}\t{song.Author}", song.Id, cancellationToken);
        if (await _userItems.GetByIdAsync(diskId, cancellationToken) is { } disk)
        {
            session.Send(new FurniListInsertEvent(disk).Compose());
        }

        await SendSongListAsync(session, editing, cancellationToken);
    }

    private async Task<bool> IsValidAsync(Editing editing, string data, CancellationToken cancellationToken)
    {
        IReadOnlyList<TraxMachineSoundSet> inserted = await _trax.GetMachineSoundSetsAsync(editing.Machine.Id, cancellationToken);
        return TraxSongRules.IsValid(data, _definitions.SamplesOf(inserted.Select(set => set.SoundSetId)), out _);
    }

    private async Task SendSoundSetsAsync(IGameSession session, Editing editing, CancellationToken cancellationToken)
    {
        IReadOnlyList<TraxMachineSoundSet> inserted = await _trax.GetMachineSoundSetsAsync(editing.Machine.Id, cancellationToken);
        List<(int, TraxSoundSet)> sets = [.. inserted
            .Select(held => (held.Slot, Set: _definitions.GetSoundSet(held.SoundSetId)))
            .Where(held => held.Set is not null)
            .Select(held => (held.Slot, held.Set!))];
        session.Send(new SoundPackagesMessageEvent(SoundSetSlots, sets, _definitions.SamplesOf(inserted.Select(held => held.SoundSetId))).Compose());

        await SendUserSoundSetsAsync(session, editing.Account.Id, cancellationToken);
    }

    private async Task SendUserSoundSetsAsync(IGameSession session, int userId, CancellationToken cancellationToken)
    {
        IReadOnlyList<(int ItemId, int SoundSetId)> owned = await _trax.GetInventorySoundSetsAsync(userId, cancellationToken);
        session.Send(new UserSoundPackagesMessageEvent([.. owned.Select(set => _definitions.GetSoundSet(set.SoundSetId)).OfType<TraxSoundSet>()]).Compose());
    }

    private async Task SendSongListAsync(IGameSession session, Editing editing, CancellationToken cancellationToken)
    {
        IReadOnlyList<TraxSongEntry> songs = await _trax.GetSongListAsync(editing.Account.Id, cancellationToken);
        IReadOnlyList<int> playlist = await _trax.GetPlaylistSongIdsAsync(editing.Machine.Id, cancellationToken);
        session.Send(new SongListMessageEvent(songs, playlist).Compose());
    }

    private async Task ReturnToOwnerAsync(int itemId, int ownerId, CancellationToken cancellationToken)
    {
        if (_sessions.TryGetByUserId(ownerId, out ClientSession? owner) && await _userItems.GetByIdAsync(itemId, cancellationToken) is { } item)
        {
            owner!.EnqueueSend(new FurniListInsertEvent(item).Compose());
        }
    }

    private async Task<Editing?> ResolveAsync(IGameSession session, CancellationToken cancellationToken)
    {
        if (session.AuthenticatedUser is not { } account || session.CurrentRoomId is not { } roomId
            || !_roomManager.TryGetLoadedRoom(roomId, out Room? room)
            || room!.SoundFurni is not { LogicClass: FurnitureLogicClass.SoundMachine } machine
            || !await MayEditAsync(account, room, cancellationToken))
        {
            return null;
        }

        return new Editing(account, room, machine);
    }

    private sealed record Editing(UserAccount Account, Room Room, RoomFloorItem Machine);
}
