using DragonEmu.Game.Settings;
using DragonEmu.Game.Rooms;
using DragonEmu.Persistence;
using DragonEmu.Protocol.Messages;

namespace DragonEmu.Game.Chat.Commands;

public sealed class CloseInfobusChatCommand : IChatCommand
{
    private readonly GameSettingsCache _gameSettings;
    private readonly IRoomManager _roomManager;
    private readonly RoomBroadcaster _roomBroadcaster;
    private readonly PublicRoomTransfers _transfers;

    public CloseInfobusChatCommand(GameSettingsCache gameSettings, IRoomManager roomManager, RoomBroadcaster roomBroadcaster, PublicRoomTransfers transfers)
    {
        _transfers = transfers;
        _gameSettings = gameSettings;
        _roomManager = roomManager;
        _roomBroadcaster = roomBroadcaster;
    }

    public string Name => "close_infobus";

    public string RequiredFuse => "fuse_administrator_access";

    public ValueTask ExecuteAsync(IGameSession session, string[] args, CancellationToken cancellationToken) =>
        InfobusChatCommandHandler.SetStatusAsync(_gameSettings, _roomManager, _roomBroadcaster, _transfers, open: false, cancellationToken);
}
