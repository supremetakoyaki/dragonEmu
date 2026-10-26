using DragonEmu.Domain;
using DragonEmu.Game.Achievements;
using DragonEmu.Game.Chat;
using DragonEmu.Game.Messenger;
using DragonEmu.Game.Rooms;
using DragonEmu.Persistence;
using DragonEmu.Protocol.Messages;
using DragonEmu.Protocol.Messages.Outgoing.RoomEngine;

namespace DragonEmu.Game.Avatars;

public sealed class MottoService
{
    public const int MaxMottoLength = 255;

    private readonly IUserRepository _userRepository;
    private readonly ProfanityFilterCache _profanityFilterCache;
    private readonly IRoomManager _roomManager;
    private readonly RoomBroadcaster _broadcaster;
    private readonly AchievementService _achievementService;
    private readonly MessengerService _messenger;

    public MottoService(
        IUserRepository userRepository, ProfanityFilterCache profanityFilterCache, IRoomManager roomManager, RoomBroadcaster broadcaster,
        AchievementService achievementService, MessengerService messenger)
    {
        _messenger = messenger;
        _userRepository = userRepository;
        _profanityFilterCache = profanityFilterCache;
        _roomManager = roomManager;
        _broadcaster = broadcaster;
        _achievementService = achievementService;
    }

    public async Task<string> ChangeAsync(IGameSession session, UserAccount account, string text, CancellationToken cancellationToken)
    {
        string motto = _profanityFilterCache.Filter(RoomTextSanitizer.Sanitize(text, MaxMottoLength));

        await _userRepository.UpdateMottoAsync(account.Id, motto, cancellationToken);
        await _achievementService.IncrementAsync(account.Id, "ACH_Motto", 1, cancellationToken);

        UserAccount updated = account with { Motto = motto };
        session.Authenticate(updated);
        session.Send(new UserChangeMessageEvent(-1, updated.Figure, updated.Gender, motto).Compose());
        await _messenger.AnnounceAsync(account.Id, cancellationToken);

        if (session.CurrentRoomId is { } roomId
            && _roomManager.TryGetLoadedRoom(roomId, out Room? room)
            && room!.TryGetActor(session.Id, out RoomActor? actor))
        {
            lock (actor!.Lock)
            {
                actor.Motto = motto;
            }

            _broadcaster.Broadcast(room, new UsersMessageEvent([actor]).Compose());
        }

        return motto;
    }
}
