namespace DragonEmu.Domain;

public enum ModerationActionType
{
    Caution,
    Message,
    Kick,
    Ban,
    RoomCaution,
    RoomMessage,
    RoomLock,
    RoomRename,
    RoomKick,
}

public sealed record ModerationActionRecord(int ModeratorId, int? UserId, int? RoomId, ModerationActionType Action, string Message, string? IssueMessage);

public sealed record RoomVisit(int RoomId, string RoomName, bool IsPublic, DateTime EnteredAt, DateTime LeftAt);

public sealed record ModeratorUserInfo(
    int UserId, string Username, DateTime CreatedAt, DateTime? LastLoginAt, int Reports, int AbusiveReports, int Cautions, int Bans);

public sealed record OffenceCategory(string Name, IReadOnlyList<(string Name, string Message)> Offences);

public sealed record ModerationPresets(IReadOnlyList<string> UserMessages, IReadOnlyList<string> RoomMessages, IReadOnlyList<OffenceCategory> Offences)
{
    public static readonly ModerationPresets Empty = new([], [], []);
}
