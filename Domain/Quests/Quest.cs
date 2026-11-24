namespace DragonEmu.Domain;

public sealed record QuestInfo(
    int Id,
    int Status,
    string Type,
    string RoomItemTypeName,
    int RewardCurrencyAmount,
    string EndDateTimeStamp,
    string LocalizationCode);

public static class QuestStatus
{
    public const int Available = 0;
    public const int Completed = 1;
    public const int Expired = 2;
    public const int Accepted = 3;

    public const int Preview = 4;
}

public sealed record QuestDefinition(
    int Id,
    string Campaign,
    string LocalizationCode,
    string Type,
    string FurniClassname,
    int RewardActivityPoints,
    int RewardActivityPointType,
    DateTime? StartsAt,
    DateTime? EndsAt,
    bool Accepted,
    bool Completed)
{
    public int StatusAt(DateTime nowUtc) =>
        Completed ? QuestStatus.Completed
        : Accepted ? QuestStatus.Accepted
        : StartsAt is { } start && nowUtc < start ? QuestStatus.Preview
        : EndsAt is { } end && nowUtc >= end ? QuestStatus.Expired
        : QuestStatus.Available;

    public QuestInfo ToInfo(DateTime nowUtc) => new(
        Id,
        StatusAt(nowUtc),
        Type,
        FurniClassname,
        RewardActivityPoints,
        EndsAt is { } end ? new DateTimeOffset(DateTime.SpecifyKind(end, DateTimeKind.Utc)).ToUnixTimeMilliseconds().ToString() : "",
        LocalizationCode);
}
