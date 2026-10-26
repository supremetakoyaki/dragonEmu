using DragonEmu.Domain;
using DragonEmu.Game.Rooms;
using DragonEmu.Persistence;
using DragonEmu.Protocol.Messages;
using DragonEmu.Protocol.Messages.Outgoing.Moderation;
using DragonEmu.Game.Settings;

namespace DragonEmu.Game.Chat.Commands;

public sealed class RollerSpeedChatCommand : IChatCommand
{
    private readonly IRoomManager _roomManager;
    private readonly IPrivateRoomRepository _privateRoomRepository;
    private readonly RoomRightsService _roomRights;
    private readonly MessageTexts _texts;

    public RollerSpeedChatCommand(IRoomManager roomManager, IPrivateRoomRepository privateRoomRepository, RoomRightsService roomRights, MessageTexts texts)
    {
        _texts = texts;
        _roomManager = roomManager;
        _privateRoomRepository = privateRoomRepository;
        _roomRights = roomRights;
    }

    public string Name => "roller_speed";

    public string? RequiredFuse => null;

    public async ValueTask ExecuteAsync(IGameSession session, string[] args, CancellationToken cancellationToken)
    {
        if (session.AuthenticatedUser is not { } account
            || session.CurrentRoomId is not { } roomId
            || !_roomManager.TryGetLoadedRoom(roomId, out Room? room) || room is null)
        {
            session.Send(new ModMessageEvent(_texts.Get(MessageKeys.NotInRoom)).Compose());
            return;
        }

        if (!await _roomRights.CanToggleFurnitureAsync(account, room.Data, cancellationToken))
        {
            session.Send(new ModMessageEvent(_texts.Get(MessageKeys.RollerSpeedNoRights)).Compose());
            return;
        }

        if (args.Length == 0)
        {
            session.Send(new ModMessageEvent(
                _texts.Format(MessageKeys.RollerSpeedCurrent, ("speed", room.Data.QueuetileSpeed), ("interval", Ticks(room.Data.RollerIntervalTicks)))).Compose());
            return;
        }

        if (!int.TryParse(args[0], out int speed)
            || speed is < PrivateRoomSummary.MinQueuetileSpeed or > PrivateRoomSummary.MaxQueuetileSpeed)
        {
            session.Send(new ModMessageEvent(_texts.Get(MessageKeys.UsageRollerSpeed)).Compose());
            return;
        }

        await _privateRoomRepository.SetQueuetileSpeedAsync(room.Id, speed, cancellationToken);
        room.SetQueuetileSpeed(speed);

        session.Send(new ModMessageEvent(_texts.Format(MessageKeys.RollerSpeedSet, ("speed", speed), ("interval", Ticks(room.Data.RollerIntervalTicks)))).Compose());
    }

    private string Ticks(int ticks) => ticks == 1 ? _texts.Get(MessageKeys.RollerIntervalOne) : _texts.Format(MessageKeys.RollerIntervalMany, ("count", ticks));
}
