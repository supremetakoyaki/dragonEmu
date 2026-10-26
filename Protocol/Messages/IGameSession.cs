using DragonEmu.Domain;

namespace DragonEmu.Protocol.Messages;

public interface IGameSession
{
    long Id { get; }

    string RemoteAddress { get; }

    UserAccount? AuthenticatedUser { get; }

    int? LoadingRoomId { get; }

    int? CurrentRoomId { get; }

    bool TeleportMode { get; set; }

    double? TeleportHeight { get; set; }

    PendingTeleportArrival? PendingTeleport { get; set; }

    PendingArrival? PendingArrival { get; set; }

    DateTime? NoInterstitialUntil { get; set; }

    bool NameChangeDialogOpen { get; set; }

    Game.Polls.PollProgress? ActivePoll { get; set; }

    string? Swimsuit { get; set; }

    IReadOnlySet<int> IgnoredUsers { get; set; }

    int? RingingAt { get; set; }

    int? LetInto { get; set; }

    int AvatarEffectId { get; set; }

    bool UserObjectSent { get; set; }

    void Send(ServerPacket packet);

    void Close();

    DateTime? MutedUntil { get; set; }

    bool IsModerator { get; set; }

    ChatFloodGuard RoomChatFlood { get; }

    ChatFloodGuard MessengerFlood { get; }

    bool TryUseCooldown(string key, TimeSpan cooldown);

    void Authenticate(UserAccount user);

    void SetLoadingRoom(int? roomId);

    void SetCurrentRoom(int? roomId);
}
