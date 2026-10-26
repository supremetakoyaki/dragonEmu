namespace DragonEmu.Domain;

public sealed record MessengerProfile(
    int Id, string Name, char Gender, string Figure, string Motto, DateTime? LastLoginAt, bool AllowsFollowing);

public sealed record FriendRequest(int FromUserId, string FromUsername);

public sealed record FriendSearchHit(MessengerProfile User, bool IsFriend);
