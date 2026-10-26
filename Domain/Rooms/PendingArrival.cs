namespace DragonEmu.Domain;

public sealed record PendingArrival(int RoomId, int X, int Y, int Rotation, DateTime ExpiresAtUtc)
{
    public bool IsFor(int roomId, DateTime utcNow) => RoomId == roomId && utcNow < ExpiresAtUtc;
}
