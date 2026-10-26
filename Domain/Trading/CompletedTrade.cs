namespace DragonEmu.Domain;

public sealed record CompletedTrade(int RoomId, CompletedTradeSide User1, CompletedTradeSide User2);

public sealed record CompletedTradeSide(int UserId, string IpAddress, IReadOnlyList<int> ItemIds);
