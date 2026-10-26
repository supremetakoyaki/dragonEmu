using DragonEmu.Game.Moderation;
using DragonEmu.Game.Chat.Commands;
using DragonEmu.Game.Chat;
using DragonEmu.Game.History;
using DragonEmu.Game.Rooms;
using DragonEmu.Protocol.Messages.Incoming.RoomChat.AuxiliaryFunctions;

namespace DragonEmu.Protocol.Messages.Incoming.RoomChat;

public sealed class ChatMessageComposer : IMessageComposer
{
    private readonly IRoomManager _roomManager;
    private readonly RoomBroadcaster _broadcaster;
    private readonly IChatCommandDispatcher _commandDispatcher;
    private readonly ProfanityFilterCache _profanityFilterCache;
    private readonly HistoryLog _history;
    private readonly ModerationService _moderation;

    private string _message = string.Empty;

    public ChatMessageComposer(
        IRoomManager roomManager, RoomBroadcaster broadcaster, IChatCommandDispatcher commandDispatcher, ProfanityFilterCache profanityFilterCache,
        HistoryLog history, ModerationService moderation)
    {
        _roomManager = roomManager;
        _broadcaster = broadcaster;
        _commandDispatcher = commandDispatcher;
        _profanityFilterCache = profanityFilterCache;
        _history = history;
        _moderation = moderation;
    }

    public void Parse(ClientPacket packet)
    {
        _message = packet.PopString();
    }

    public ValueTask HandleAsync(IGameSession session, CancellationToken cancellationToken) =>
        RoomChatHandler.SendAsync(session, _roomManager, _broadcaster, _commandDispatcher, _profanityFilterCache, _history, _moderation, _message, shout: false, cancellationToken);
}
