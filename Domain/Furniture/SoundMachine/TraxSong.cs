namespace DragonEmu.Domain;

public sealed record TraxSong(int Id, string Name, string Author, string Data)
{
    private const int SecondsPerSlot = 2;

    public int LengthSeconds { get; } = LengthOf(Data);

    public static int LengthOf(string data)
    {
        int longest = 0;
        string[] parts = data.Split(':');
        for (int track = 0; track + 1 < parts.Length; track += 2)
        {
            int slots = 0;
            foreach (string entry in parts[track + 1].Split(';', StringSplitOptions.RemoveEmptyEntries))
            {
                string[] sample = entry.Split(',');
                if (sample.Length == 2 && int.TryParse(sample[1], out int length) && length > 0)
                {
                    slots += length;
                }
            }

            longest = Math.Max(longest, slots);
        }

        return Math.Max(1, longest * SecondsPerSlot);
    }
}
