namespace DragonEmu.Domain;

public sealed class ActionCooldowns
{
    public static readonly TimeSpan Default = TimeSpan.FromSeconds(2);

    public const string Figure = "figure";
    public const string Motto = "motto";
    public const string RoomSettings = "room_settings";
    public const string RoomThumbnail = "room_thumbnail";
    public const string HomeRoom = "home_room";
    public const string RoomInvite = "room_invite";
    public const string RoomEvent = "room_event";

    public static string WardrobeSlot(int slot) => $"wardrobe:{slot}";

    private const int PruneAbove = 32;

    private readonly Dictionary<string, long> _until = new(StringComparer.Ordinal);

    public bool TryUse(string key, TimeSpan cooldown, long nowMs)
    {
        lock (_until)
        {
            if (_until.TryGetValue(key, out long until) && nowMs < until)
            {
                return false;
            }

            if (_until.Count >= PruneAbove)
            {
                foreach (string expired in _until.Where(entry => entry.Value <= nowMs).Select(entry => entry.Key).ToList())
                {
                    _until.Remove(expired);
                }
            }

            _until[key] = nowMs + (long)cooldown.TotalMilliseconds;
            return true;
        }
    }
}
