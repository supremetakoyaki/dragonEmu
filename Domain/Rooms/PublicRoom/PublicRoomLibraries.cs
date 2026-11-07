namespace DragonEmu.Domain;

public sealed record PublicRoomLibraries(string HeightmapLabel, string LibraryName, string? NavCode, int? RoomId = null);
