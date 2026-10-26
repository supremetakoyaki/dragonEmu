using DragonEmu.Domain;
using DragonEmu.Game.Rooms;
using DragonEmu.Gateway;
using DragonEmu.Persistence;
using DragonEmu.Protocol.Messages;
using DragonEmu.Protocol.Messages.Outgoing.InventoryBadges;
using DragonEmu.Protocol.Messages.Outgoing.Moderation;
using DragonEmu.Game.Settings;

namespace DragonEmu.Game.Chat.Commands;

public sealed class GiveBadgeChatCommand : IChatCommand
{
    private const int MaxBadgeCodeLength = 6;

    private readonly IUserRepository _userRepository;
    private readonly IUserBadgeRepository _userBadgeRepository;
    private readonly ClientSessionManager _sessionManager;
    private readonly MessageTexts _texts;

    public GiveBadgeChatCommand(IUserRepository userRepository, IUserBadgeRepository userBadgeRepository, ClientSessionManager sessionManager, MessageTexts texts)
    {
        _texts = texts;
        _userRepository = userRepository;
        _userBadgeRepository = userBadgeRepository;
        _sessionManager = sessionManager;
    }

    public string Name => "givebadge";

    public string RequiredFuse => RoomRightsService.AdministratorAccessFuse;

    public async ValueTask ExecuteAsync(IGameSession session, string[] args, CancellationToken cancellationToken)
    {
        if (args.Length != 2 || args[1].Length > MaxBadgeCodeLength)
        {
            session.Send(new ModMessageEvent(_texts.Format(MessageKeys.UsageGivebadge, ("max", MaxBadgeCodeLength))).Compose());
            return;
        }

        string badgeCode = args[1];
        if (await _userRepository.GetByUsernameAsync(args[0], cancellationToken) is not { } receiver)
        {
            session.Send(new ModMessageEvent(_texts.Format(MessageKeys.UserNotFound, ("username", args[0]))).Compose());
            return;
        }

        if (!await _userBadgeRepository.AwardAsync(receiver.Id, badgeCode, cancellationToken))
        {
            session.Send(new ModMessageEvent(_texts.Format(MessageKeys.BadgeAlreadyOwned, ("username", receiver.Username), ("badge", badgeCode))).Compose());
            return;
        }

        if (_sessionManager.TryGetByUserId(receiver.Id, out ClientSession? online))
        {
            IReadOnlyList<UserBadge> badges = await _userBadgeRepository.GetAllAsync(receiver.Id, cancellationToken);
            online!.EnqueueSend(BadgesEvent.For(badges).Compose());
        }

        session.Send(new ModMessageEvent(_texts.Format(MessageKeys.BadgeGiven, ("username", receiver.Username), ("badge", badgeCode))).Compose());
    }
}
