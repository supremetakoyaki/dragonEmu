namespace DragonEmu.Persistence;

public sealed record RoomModel(int Id, string Label, int ClubLevel);

public interface IRoomHeightmapRepository
{
    Task<RoomModel?> GetByLabelAsync(string label, CancellationToken cancellationToken = default);

    Task<int?> GetAnyRoomIdByHeightmapLabelAsync(string label, CancellationToken cancellationToken = default);
}
