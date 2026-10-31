using DragonEmu.Domain;

namespace DragonEmu.Game.Rooms;

public sealed record TraxPlayback(int MachineItemId, IReadOnlyList<TraxSong> Songs, DateTime StartedAtUtc)
{
    public int PositionSeconds(DateTime utcNow) => Math.Max(0, (int)(utcNow - StartedAtUtc).TotalSeconds);

    public TraxSong? Song(int songId) => Songs.FirstOrDefault(song => song.Id == songId);
}

public sealed partial class Room
{
    private TraxPlayback? _playback;

    public TraxPlayback? Playback => Volatile.Read(ref _playback);

    public static bool IsSoundFurni(FurnitureLogicClass logicClass) => logicClass is FurnitureLogicClass.SoundMachine or FurnitureLogicClass.Jukebox;

    public RoomFloorItem? SoundFurni => FloorItemsWhere(item => IsSoundFurni(item.LogicClass)).FirstOrDefault();

    public static bool IsMusicFurni(FurnitureLogicClass logicClass) => IsSoundFurni(logicClass) || logicClass == FurnitureLogicClass.Hotspot;

    public RoomFloorItem? MusicFurni => FloorItemsWhere(item => IsMusicFurni(item.LogicClass)).FirstOrDefault();

    public void StartPlayback(int machineItemId, IReadOnlyList<TraxSong> songs, DateTime utcNow) =>
        Volatile.Write(ref _playback, new TraxPlayback(machineItemId, songs, utcNow));

    public void StopPlayback() => Volatile.Write(ref _playback, null);

    private void StopPlaybackOf(int itemId)
    {
        if (Playback is { } playback && playback.MachineItemId == itemId)
        {
            StopPlayback();
        }
    }
}
