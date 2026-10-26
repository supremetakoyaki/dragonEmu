namespace DragonEmu.Domain;

public sealed record RoomEvent(
    int HostUserId, string HostName, long HostSessionId, int Category, string Name, string Description, string StartTime, IReadOnlyList<string> Tags);
