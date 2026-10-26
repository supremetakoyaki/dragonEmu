namespace DragonEmu.Domain;

public sealed record PublicRoomWarpTile(string HeightmapLabel, int X, int Y, int TargetRoomId, int TargetX, int TargetY);
