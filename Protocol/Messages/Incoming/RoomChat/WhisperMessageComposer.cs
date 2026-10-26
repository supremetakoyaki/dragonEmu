using DragonEmu.Game.Moderation;
using DragonEmu.Domain;
using DragonEmu.Game.Fuses;
using DragonEmu.Game.History;
using DragonEmu.Game.Chat;
using DragonEmu.Game.Rooms;
using DragonEmu.Gateway;
using DragonEmu.Persistence;
using DragonEmu.Protocol.Messages.Incoming.RoomChat.AuxiliaryFunctions;
using DragonEmu.Protocol.Messages.Outgoing.RoomChat;

namespace DragonEmu.Protocol.Messages.Incoming.RoomChat;

public sealed class WhisperMessageComposer : IMessageComposer
{
    private const string ModeratorFuse = "fuse_moderator_access";

    private readonly IRoomManager _roomManager;
    private readonly ClientSessionManager _sessionManager;
    private readonly FuseService _fuses;
    private readonly ProfanityFilterCache _profanityFilterCache;
    private readonly HistoryLog _history;

    private string _targetUsername = string.Empty;
    private string _message = string.Empty;

    private readonly ModerationService _moderation;

    public WhisperMessageComposer(
        IRoomManager roomManager, ClientSessionManager sessionManager, FuseService fuses, ProfanityFilterCache profanityFilterCache,
        HistoryLog history, ModerationService moderation)
    {
        _roomManager = roomManager;
        _sessionManager = sessionManager;
        _fuses = fuses;
        _profanityFilterCache = profanityFilterCache;
        _history = history;
        _moderation = moderation;
    }

    public void Parse(ClientPacket packet)
    {
        string raw = Sanitize(packet.PopString());
        int spaceIndex = raw.IndexOf(' ');
        if (spaceIndex < 0)
        {
            _targetUsername = raw;
            return;
        }

        _targetUsername = raw[..spaceIndex];
        _message = raw[(spaceIndex + 1)..];
    }

    public async ValueTask HandleAsync(IGameSession session, CancellationToken cancellationToken)
    {
        if (session.AuthenticatedUser is not { } account || session.CurrentRoomId is not { } roomId
            || _targetUsername.Length == 0 || _message.Length == 0)
        {
            return;
        }

        if (!RoomChatHandler.PassesFloodControl(session, _roomManager))
        {
            return;
        }

        Room? room = await _roomManager.GetOrLoadRoomAsync(roomId, cancellationToken);
        if (room is null || !room.TryGetActor(session.Id, out RoomActor? sender))
        {
            return;
        }

        (bool blocked, string? alert) = await _moderation.WhyCantChatAsync(session, account, room, cancellationToken);
        if (blocked)
        {
            if (alert is not null)
            {
                session.Send(new WhisperMessageEvent(sender!.VirtualId, alert).Compose());
            }

            return;
        }

        RoomActor? target = room.Actors.FirstOrDefault(
            a => a.Type == ActorType.Player && string.Equals(a.Username, _targetUsername, StringComparison.OrdinalIgnoreCase));
        if (target is null)
        {
            return;
        }

        string filtered = _profanityFilterCache.Filter(_message);

        session.Send(new WhisperMessageEvent(sender!.VirtualId, filtered).Compose());

        if (target.SessionId != sender.SessionId && _sessionManager.TryGet(target.SessionId, out ClientSession? targetSession)
            && !targetSession!.IgnoredUsers.Contains(account.Id))
        {
            targetSession!.EnqueueSend(new WhisperMessageEvent(sender.VirtualId, filtered).Compose());
        }

        _history.Record(new RoomChatRecord(
            DateTime.UtcNow, account.Id, roomId, RoomTextSanitizer.Sanitize(_message, RoomChatHandler.MaxRecordedLength),
            RoomChatType.Whisper, target.AccountId, session.RemoteAddress));

        await NotifyModeratorsAsync(room, sender, target, filtered, cancellationToken);
    }

    private async ValueTask NotifyModeratorsAsync(Room room, RoomActor sender, RoomActor target, string message, CancellationToken cancellationToken)
    {
        string eavesdropMessage = $"[whispering to {target.Username}] {message}";

        foreach (RoomActor actor in room.Actors)
        {
            if (actor.Type != ActorType.Player || actor.SessionId == sender.SessionId || actor.SessionId == target.SessionId)
            {
                continue;
            }

            if (!_sessionManager.TryGet(actor.SessionId, out ClientSession? moderatorSession) || moderatorSession!.AuthenticatedUser is not { } moderatorAccount)
            {
                continue;
            }

            if (await _fuses.HoldsAsync(moderatorAccount, ModeratorFuse, cancellationToken: cancellationToken))
            {
                moderatorSession.EnqueueSend(new WhisperMessageEvent(sender.VirtualId, eavesdropMessage).Compose());
            }
        }
    }

    private static string Sanitize(string value) =>
        new(value.Where(c => c is not (char)0x01 and not (char)0x02).ToArray());
}
