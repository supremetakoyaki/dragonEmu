using DragonEmu.Game.Rooms;
using DragonEmu.Game.Settings;
using DragonEmu.Gateway;
using DragonEmu.Protocol.Messages;
using DragonEmu.Protocol.Messages.Outgoing.Moderation;

namespace DragonEmu.Game.Chat.Commands;

public sealed class InfoChatCommand : IChatCommand
{
    private readonly ClientSessionManager _sessionManager;
    private readonly IRoomManager _roomManager;
    private readonly MessageTexts _texts;

    public InfoChatCommand(ClientSessionManager sessionManager, IRoomManager roomManager, MessageTexts texts)
    {
        _sessionManager = sessionManager;
        _roomManager = roomManager;
        _texts = texts;
    }

    public string Name => "info";

    public string? RequiredFuse => null;

    public ValueTask ExecuteAsync(IGameSession session, string[] args, CancellationToken cancellationToken)
    {
        session.Send(ModMessageEvent.FromLines(
        [
            ServerInfo.Display,
            _texts.Format(MessageKeys.InfoUsersOnline, ("count", _sessionManager.OnlineUserCount)),
            _texts.Format(MessageKeys.InfoRoomsLoaded, ("count", _roomManager.LoadedRoomCount)),
        ]).Compose());

        return ValueTask.CompletedTask;
    }
}
