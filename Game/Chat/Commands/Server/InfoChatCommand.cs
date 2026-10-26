using DragonEmu.Game.Rooms;
using DragonEmu.Gateway;
using DragonEmu.Protocol.Messages;
using DragonEmu.Protocol.Messages.Outgoing.Moderation;

namespace DragonEmu.Game.Chat.Commands;

public sealed class InfoChatCommand : IChatCommand
{
    private readonly ClientSessionManager _sessionManager;
    private readonly IRoomManager _roomManager;

    public InfoChatCommand(ClientSessionManager sessionManager, IRoomManager roomManager)
    {
        _sessionManager = sessionManager;
        _roomManager = roomManager;
    }

    public string Name => "info";

    public string? RequiredFuse => null;

    public ValueTask ExecuteAsync(IGameSession session, string[] args, CancellationToken cancellationToken)
    {
        session.Send(ModMessageEvent.FromLines(
        [
            ServerInfo.Display,
            $"Users online: {_sessionManager.OnlineUserCount}",
            $"Rooms loaded: {_roomManager.LoadedRoomCount}",
        ]).Compose());

        return ValueTask.CompletedTask;
    }
}
