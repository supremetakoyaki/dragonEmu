namespace DragonEmu.Persistence;

public sealed record RoomModel(int Id, string Label, bool Club);

public interface IRoomHeightmapRepository
{
    Task<RoomModel?> GetByLabelAsync(string label, CancellationToken cancellationToken = default);

    Task<int?> GetAnyRoomIdByHeightmapLabelAsync(string label, CancellationToken cancellationToken = default);
}
