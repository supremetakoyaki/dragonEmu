namespace DragonEmu.Domain;

public sealed record ChatlogLine(DateTime SaidAt, int UserId, string Username, string Message);

public sealed record ChatlogRoom(int RoomId, string RoomName, bool IsPublic, IReadOnlyList<ChatlogLine> Lines);
