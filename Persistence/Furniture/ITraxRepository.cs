using DragonEmu.Domain;

namespace DragonEmu.Persistence;

public interface ITraxRepository
{
    Task<IReadOnlyList<TraxSong>> GetPlaylistAsync(int machineItemId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<TraxSong>> GetJukeboxPlaylistAsync(int jukeboxItemId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<TraxSong>> GetPlaylistOfAsync(int machineItemId, FurnitureLogicClass logicClass, CancellationToken cancellationToken = default) =>
        logicClass == FurnitureLogicClass.Jukebox
            ? GetJukeboxPlaylistAsync(machineItemId, cancellationToken)
            : GetPlaylistAsync(machineItemId, cancellationToken);

    Task<IReadOnlyList<TraxSong>> GetUserSongsAsync(int userId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<TraxSongEntry>> GetSongListAsync(int userId, CancellationToken cancellationToken = default);

    Task<TraxSongEntry?> GetSongAsync(int songId, CancellationToken cancellationToken = default);

    Task<int> CountSongsAsync(int userId, CancellationToken cancellationToken = default);

    Task<int> CreateSongAsync(int userId, string name, string author, string data, CancellationToken cancellationToken = default);

    Task UpdateSongAsync(int songId, string name, string data, CancellationToken cancellationToken = default);

    Task DeleteSongAsync(int songId, CancellationToken cancellationToken = default);

    Task<bool> HasBurntSongNamedAsync(int userId, string name, int exceptSongId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<int>> GetPlaylistSongIdsAsync(int machineItemId, CancellationToken cancellationToken = default);

    Task SetPlaylistAsync(int machineItemId, IReadOnlyList<int> songIds, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<TraxMachineSoundSet>> GetMachineSoundSetsAsync(int machineItemId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<(int ItemId, int SoundSetId)>> GetInventorySoundSetsAsync(int userId, CancellationToken cancellationToken = default);

    Task<int?> InsertSoundSetAsync(int machineItemId, int slot, int userId, int soundSetId, CancellationToken cancellationToken = default);

    Task<TraxMachineSoundSet?> EjectSoundSetAsync(int machineItemId, int slot, CancellationToken cancellationToken = default);

    Task<int> BurnAsync(int userId, int songDiskDefinitionId, string extraData, int songId, CancellationToken cancellationToken = default);

    Task<int> BurnPrebuiltAsync(int userId, int songDiskDefinitionId, string extraData, PrebuiltSong song, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<TraxDiskItem>> GetInventoryDisksAsync(int userId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<JukeboxDisk>> GetJukeboxDisksAsync(int jukeboxItemId, CancellationToken cancellationToken = default);

    Task<bool> LoadDiskAsync(int jukeboxItemId, int slot, int userId, int diskItemId, CancellationToken cancellationToken = default);

    Task<JukeboxDisk?> UnloadDiskAsync(int jukeboxItemId, int slot, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<int>> GetJukeboxPlaylistDiskIdsAsync(int jukeboxItemId, CancellationToken cancellationToken = default);

    Task SetJukeboxPlaylistAsync(int jukeboxItemId, IReadOnlyList<int> diskItemIds, CancellationToken cancellationToken = default);
}
