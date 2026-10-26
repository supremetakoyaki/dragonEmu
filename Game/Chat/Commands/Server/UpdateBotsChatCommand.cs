using DragonEmu.Game.Rooms;
using DragonEmu.Protocol.Messages;
using DragonEmu.Protocol.Messages.Outgoing.Moderation;
using DragonEmu.Protocol.Messages.Outgoing.RoomEngine;
using DragonEmu.Game.Settings;

namespace DragonEmu.Game.Chat.Commands;

public sealed class UpdateBotsChatCommand : IChatCommand
{
    private readonly IRoomManager _roomManager;
    private readonly RoomBotService _bots;
    private readonly RoomBroadcaster _broadcaster;
    private readonly RoomMovementScheduler _scheduler;
    private readonly GuideBotService _guides;
    private readonly MessageTexts _texts;

    public UpdateBotsChatCommand(
        IRoomManager roomManager, RoomBotService bots, RoomBroadcaster broadcaster, RoomMovementScheduler scheduler, GuideBotService guides, MessageTexts texts)
    {
        _texts = texts;
        _guides = guides;
        _roomManager = roomManager;
        _bots = bots;
        _broadcaster = broadcaster;
        _scheduler = scheduler;
    }

    public string Name => "update_bots";

    public string RequiredFuse => RoomRightsService.AdministratorAccessFuse;

    public async ValueTask ExecuteAsync(IGameSession session, string[] args, CancellationToken cancellationToken)
    {
        int bots = 0;
        await _guides.ReloadAsync(cancellationToken);
        IReadOnlyList<Room> rooms = _roomManager.LoadedRooms;
        foreach (Room room in rooms)
        {
            IReadOnlyList<BotDefinition> definitions = await _bots.LoadAsync(room.Id, cancellationToken);
            foreach (RoomActor gone in room.RemoveBots())
            {
                _broadcaster.Broadcast(room, new UserRemoveMessageEvent(gone.VirtualId).Compose());
            }

            List<RoomActor> added = room.AddBots(definitions);
            if (added.Count > 0)
            {
                _broadcaster.Broadcast(room, new UsersMessageEvent(added).Compose());
                _broadcaster.Broadcast(room, new UserUpdateMessageEvent(added).Compose());
                _scheduler.MarkActive(room);
            }

            bots += added.Count;
        }

        session.Send(new ModMessageEvent(_texts.Format(MessageKeys.BotsReloaded, ("bots", bots), ("rooms", rooms.Count))).Compose());
    }
}
