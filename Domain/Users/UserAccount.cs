namespace DragonEmu.Domain;

public sealed record UserAccount(
    int Id,
    string Username,
    string Email,
    string Motto,
    int Rank,
    char Gender,
    string Figure,
    DateTime CreatedAt,
    GroupBadge? FavouriteGroup = null);
