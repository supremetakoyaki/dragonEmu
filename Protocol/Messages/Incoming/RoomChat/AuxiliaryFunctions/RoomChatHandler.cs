using DragonEmu.Game.Chat;
using DragonEmu.Domain;
using DragonEmu.Game.Chat.Commands;
using DragonEmu.Game.History;
using DragonEmu.Game.Moderation;
using DragonEmu.Game.Rooms;
using DragonEmu.Protocol.Messages.Outgoing.RoomChat;

namespace DragonEmu.Protocol.Messages.Incoming.RoomChat.AuxiliaryFunctions;

internal static class RoomChatHandler
{
    public static async ValueTask SendAsync(
        IGameSession session,
        IRoomManager roomManager,
        RoomBroadcaster broadcaster,
        IChatCommandDispatcher commandDispatcher,
        ProfanityFilterCache profanityFilterCache,
        HistoryLog history,
        ModerationService moderation,
        string message,
        bool shout,
        CancellationToken cancellationToken)
    {
        if (session.AuthenticatedUser is not { } account)
        {
            return;
        }

        if (!PassesFloodControl(session, roomManager))
        {
            return;
        }

        string sanitized = Sanitize(message);

        if (sanitized.StartsWith(':') && await commandDispatcher.TryHandleAsync(session, sanitized, cancellationToken))
        {
            return;
        }

        if (session.CurrentRoomId is not { } roomId)
        {
            return;
        }

        Room? room = await roomManager.GetOrLoadRoomAsync(roomId, cancellationToken);
        if (room is null || !room.TryGetActor(session.Id, out RoomActor? actor))
        {
            return;
        }

        (bool blocked, string? alert) = await moderation.WhyCantChatAsync(session, account, room, cancellationToken);
        if (blocked)
        {
            if (alert is not null)
            {
                session.Send(new WhisperMessageEvent(actor!.VirtualId, alert).Compose());
            }

            return;
        }

        string filtered = profanityFilterCache.Filter(sanitized);

        ServerPacket packet = shout
            ? new ShoutMessageEvent(actor!.VirtualId, filtered).Compose()
            : new ChatMessageEvent(actor!.VirtualId, filtered).Compose();

        broadcaster.BroadcastFrom(room, packet, account.Id);
        history.Record(new RoomChatRecord(
            DateTime.UtcNow, account.Id, roomId, RoomTextSanitizer.Sanitize(sanitized, MaxRecordedLength),
            shout ? RoomChatType.Shout : RoomChatType.Chat, WhisperedUserId: null, session.RemoteAddress));

        room.HearChat(actor, filtered, shout);
    }

    public static bool PassesFloodControl(IGameSession session, IRoomManager roomManager)
    {
        if (session.RoomChatFlood.TryPass(Environment.TickCount64, out int secondsLeft, out bool tell))
        {
            return true;
        }

        if (tell && session.CurrentRoomId is { } roomId && roomManager.TryGetLoadedRoom(roomId, out Room? room)
            && room!.TryGetActor(session.Id, out RoomActor? actor))
        {
            session.Send(new WhisperMessageEvent(actor!.VirtualId, FloodText(secondsLeft)).Compose());
        }

        return false;
    }

    public static string FloodText(int secondsLeft) =>
        $"You're typing too fast, don't flood the room! - {secondsLeft} seconds remaining";

    public const int MaxRecordedLength = 255;

    private static string Sanitize(string message) =>
        new(message.Where(c => c is not (char)0x01 and not (char)0x02).ToArray());
}
