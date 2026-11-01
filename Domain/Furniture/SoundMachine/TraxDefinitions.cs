namespace DragonEmu.Domain;

public sealed record TraxSoundSet(int Id, int FurniDefinitionId, string Name, IReadOnlyList<int> SampleIds);

public sealed record TraxSample(int Id, int SoundSetId, int Position, int Bars);

public sealed record TraxSongEntry(int Id, int UserId, string Name, string Author, string Data, bool Burnt)
{
    public int LengthSeconds { get; } = TraxSong.LengthOf(Data);

    public TraxSong ToSong() => new(Id, Name, Author, Data);
}

public sealed record TraxMachineSoundSet(int Slot, int SoundSetId, int ItemId, int OwnerId);

public sealed record JukeboxDisk(int Slot, int ItemId, int OwnerId, TraxSong Song);

public sealed record TraxDiskItem(int ItemId, TraxSong Song);
