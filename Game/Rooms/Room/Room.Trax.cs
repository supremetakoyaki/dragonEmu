using DragonEmu.Domain;

namespace DragonEmu.Game.Rooms;

public sealed record TraxPlayback(int MachineItemId, IReadOnlyList<TraxSong> Songs, DateTime StartedAtUtc)
{
    public int PositionSeconds(DateTime utcNow) => Math.Max(0, (int)(utcNow - StartedAtUtc).TotalSeconds);

    public TraxSong? Song(int songId) => Songs.FirstOrDefault(song => song.Id == songId);

    public TraxPlayback WithSongs(IReadOnlyList<TraxSong> songs, DateTime utcNow)
    {
        int total = Songs.Sum(song => song.LengthSeconds);
        if (total == 0)
        {
            return this with { Songs = songs, StartedAtUtc = utcNow };
        }

        int position = PositionSeconds(utcNow) % total;
        int index = 0;
        while (position >= Songs[index].LengthSeconds)
        {
            position -= Songs[index].LengthSeconds;
            index++;
        }

        int playingId = Songs[index].Id;
        int before = 0;
        foreach (TraxSong song in songs)
        {
            if (song.Id == playingId)
            {
                return this with { Songs = songs, StartedAtUtc = utcNow.AddSeconds(-(before + position)) };
            }

            before += song.LengthSeconds;
        }

        return this with { Songs = songs, StartedAtUtc = utcNow };
    }
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

    public TraxPlayback? ChangePlaylist(IReadOnlyList<TraxSong> songs, DateTime utcNow)
    {
        if (Playback is not { } playback)
        {
            return null;
        }

        TraxPlayback changed = playback.WithSongs(songs, utcNow);
        Volatile.Write(ref _playback, changed);
        return changed;
    }

    private void StopPlaybackOf(int itemId)
    {
        if (Playback is { } playback && playback.MachineItemId == itemId)
        {
            StopPlayback();
        }
    }
}
