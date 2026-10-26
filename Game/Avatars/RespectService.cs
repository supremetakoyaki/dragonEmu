using DragonEmu.Domain;
using DragonEmu.Game.Achievements;
using DragonEmu.Game.Rooms;
using DragonEmu.Game.Settings;
using DragonEmu.Persistence;
using DragonEmu.Protocol.Messages;
using DragonEmu.Protocol.Messages.Outgoing.Users;

namespace DragonEmu.Game.Avatars;

public sealed class RespectService
{
    private const string RespectGivenAchievement = "ACH_RespectGiven";
    private const string RespectEarnedAchievement = "ACH_RespectEarned";

    private readonly IRespectRepository _respectRepository;
    private readonly HotelClock _clock;
    private readonly GameSettingsCache _gameSettings;
    private readonly IRoomManager _roomManager;
    private readonly RoomBroadcaster _broadcaster;
    private readonly AchievementService _achievements;

    public RespectService(
        IRespectRepository respectRepository, HotelClock clock, GameSettingsCache gameSettings, IRoomManager roomManager,
        RoomBroadcaster broadcaster, AchievementService achievements)
    {
        _respectRepository = respectRepository;
        _clock = clock;
        _gameSettings = gameSettings;
        _roomManager = roomManager;
        _broadcaster = broadcaster;
        _achievements = achievements;
    }

    public async Task<(int Received, int Left, int PetLeft)> GetAsync(int userId, CancellationToken cancellationToken)
    {
        RespectStanding standing = await _respectRepository.GetAsync(userId, cancellationToken);
        GameSettings settings = _gameSettings.Current;
        return (standing.Received, standing.LeftOn(_clock.Today, settings.DailyRespectGrant),
            settings.Pets.Enabled ? standing.PetLeftOn(_clock.Today, settings.DailyPetRespectGrant) : 0);
    }

    public async Task GiveAsync(IGameSession session, UserAccount giver, int targetUserId, CancellationToken cancellationToken)
    {
        if (targetUserId == giver.Id
            || session.CurrentRoomId is not { } roomId
            || !_roomManager.TryGetLoadedRoom(roomId, out Room? room)
            || !room!.Actors.Any(actor => actor.Type == ActorType.Player && actor.AccountId == targetUserId))
        {
            return;
        }

        if (await _respectRepository.TryGiveAsync(
                giver.Id, targetUserId, _clock.Today, _gameSettings.Current.DailyRespectGrant, cancellationToken) is not { } given)
        {
            return;
        }

        _broadcaster.Broadcast(room, new RespectNotificationMessageEvent(targetUserId, given.TargetTotalReceived).Compose());

        await _achievements.SetAsync(giver.Id, RespectGivenAchievement, given.GiverTotalGiven, cancellationToken);
        await _achievements.SetAsync(targetUserId, RespectEarnedAchievement, given.TargetTotalReceived, cancellationToken);
    }
}
