namespace DragonEmu.Domain;

public enum IssueState
{
    Open = 1,
    Picked = 2,
    Closed = 3,
}

public enum IssueResolution
{
    Useless = 1,
    Abusive = 2,
    Resolved = 3,
}

public sealed record ModerationIssue(
    int Id, IssueState State, int Category, int ReporterId, string ReporterName, int? ReportedId, string ReportedName,
    int? RoomId, string RoomName, bool RoomIsPublic, string Message, int? PickerId, string PickerName, DateTime CreatedAt);
