namespace DragonEmu.Domain;

public sealed record PendingTeleportArrival(int RoomId, int TeleporterItemId, DateTime ExpiresAtUtc)
{
    public bool IsFor(int roomId, DateTime utcNow) => RoomId == roomId && utcNow < ExpiresAtUtc;
}
