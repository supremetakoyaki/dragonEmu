namespace DragonEmu.Domain;

public sealed record Photo(int Id, int ItemId, int UserId, string Caption, string Token, DateTime TakenAt, bool Removed)
{
    public string Path => Removed ? string.Empty : $"/photos/{Id}-{Token}.png";
}
