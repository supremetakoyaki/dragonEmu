namespace DragonEmu.Domain;

public static class TraxSongRules
{
    public const int TrackCount = 4;
    public const int SlotCount = 150;
    public const int SilentSample = 0;
    public const int MaxNameLength = 100;

    public static bool IsValid(string data, IReadOnlyDictionary<int, int> samples, out bool hasSound)
    {
        hasSound = false;
        if (string.IsNullOrEmpty(data))
        {
            return false;
        }

        string[] parts = data.Split(':');
        if (parts.Length % 2 != 0 || parts.Length / 2 > TrackCount)
        {
            return false;
        }

        var tracks = new HashSet<int>();
        for (int i = 0; i < parts.Length; i += 2)
        {
            if (!int.TryParse(parts[i], out int track) || track is < 1 or > TrackCount || !tracks.Add(track))
            {
                return false;
            }

            int slots = 0;
            foreach (string entry in parts[i + 1].Split(';', StringSplitOptions.RemoveEmptyEntries))
            {
                string[] sample = entry.Split(',');
                if (sample.Length != 2 || !int.TryParse(sample[0], out int id) || !int.TryParse(sample[1], out int length) || length < 1)
                {
                    return false;
                }

                if (id != SilentSample)
                {
                    if (!samples.TryGetValue(id, out int bars) || bars != length)
                    {
                        return false;
                    }

                    hasSound = true;
                }

                slots += length;
                if (slots > SlotCount)
                {
                    return false;
                }
            }
        }

        return true;
    }

    public static IReadOnlySet<int> SamplesIn(string data)
    {
        var ids = new HashSet<int>();
        string[] parts = data.Split(':');
        for (int i = 1; i < parts.Length; i += 2)
        {
            foreach (string entry in parts[i].Split(';', StringSplitOptions.RemoveEmptyEntries))
            {
                string[] sample = entry.Split(',');
                if (sample.Length == 2 && int.TryParse(sample[0], out int id) && id != SilentSample)
                {
                    ids.Add(id);
                }
            }
        }

        return ids;
    }

    public static string? CleanName(string name)
    {
        string trimmed = new string(name.Where(c => !char.IsControl(c)).ToArray()).Trim();
        if (trimmed.Length == 0)
        {
            return null;
        }

        return trimmed.Length > MaxNameLength ? trimmed[..MaxNameLength] : trimmed;
    }
}
