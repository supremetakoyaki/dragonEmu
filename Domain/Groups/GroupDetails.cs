namespace DragonEmu.Domain;

public sealed record GroupDetails(int Id, string Name, string Description, int? RoomId, string RoomName);
