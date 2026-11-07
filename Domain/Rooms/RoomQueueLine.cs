namespace DragonEmu.Domain;

public enum RoomQueueLine
{
    Spectators = 1,
    Visitors = 2,
}

public sealed record RoomQueuePlace(RoomQueueLine Line, int Ahead, int? ClubAhead);

public sealed record RoomQueueStatus(RoomQueuePlace Place, RoomQueuePlace OtherLine);

public sealed record PendingRoomAdmission(int RoomId, RoomQueueLine Line, DateTime ExpiresAtUtc)
{
    public bool IsFor(int roomId, DateTime utcNow) => RoomId == roomId && utcNow < ExpiresAtUtc;
}
