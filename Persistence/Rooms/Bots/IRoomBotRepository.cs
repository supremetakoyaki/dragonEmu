namespace DragonEmu.Persistence;

public sealed record RoomBotRecord(
    int Id, int RoomId, string Name, char Gender, string Figure, string Motto, int X, int Y, double Z, int Rotation,
    string? WalkableAreaJson, bool DiagonalMoveAllowed, IReadOnlyList<RoomBotActionRecord> Actions);

public sealed record RoomBotActionRecord(int Id, string TriggerType, string? TriggerDataJson, string ResponsesJson, int TileDistanceRequired);

public interface IRoomBotRepository
{
    Task<IReadOnlyList<RoomBotRecord>> GetForRoomAsync(int roomId, CancellationToken cancellationToken = default);
}
