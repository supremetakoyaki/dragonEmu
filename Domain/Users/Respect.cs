namespace DragonEmu.Domain;

public sealed record RespectStanding(int Received, DateOnly? LastGivenDay, int GivenThatDay, DateOnly? PetLastGivenDay = null, int PetGivenThatDay = 0)
{
    public static RespectStanding None { get; } = new(0, null, 0);

    public int LeftOn(DateOnly today, int dailyGrant) =>
        Math.Max(0, dailyGrant - (LastGivenDay == today ? GivenThatDay : 0));

    public int PetLeftOn(DateOnly today, int dailyGrant) =>
        Math.Max(0, dailyGrant - (PetLastGivenDay == today ? PetGivenThatDay : 0));
}

public sealed record RespectGiven(int GiverTotalGiven, int TargetTotalReceived);
