using System.Globalization;
using DragonEmu.Game.Fuses;
using DragonEmu.Game.Rooms;
using DragonEmu.Protocol.Messages;
using DragonEmu.Protocol.Messages.Outgoing.Moderation;
using DragonEmu.Game.Settings;

namespace DragonEmu.Game.Chat.Commands;

public sealed class PositionChatCommand : IChatCommand
{
    private readonly IRoomManager _roomManager;
    private readonly FuseService _fuses;
    private readonly MessageTexts _texts;

    public PositionChatCommand(IRoomManager roomManager, FuseService fuses, MessageTexts texts)
    {
        _texts = texts;
        _roomManager = roomManager;
        _fuses = fuses;
    }

    public string Name => "pos";

    public string? RequiredFuse => null;

    public async ValueTask ExecuteAsync(IGameSession session, string[] args, CancellationToken cancellationToken)
    {
        if (session.CurrentRoomId is not { } roomId
            || !_roomManager.TryGetLoadedRoom(roomId, out Room? room)
            || !room!.TryGetActor(session.Id, out RoomActor? actor))
        {
            session.Send(new ModMessageEvent(_texts.Get(MessageKeys.NotInRoom)).Compose());
            return;
        }

        int x, y, rotation;
        double z;
        lock (actor!.Lock)
        {
            x = actor.X;
            y = actor.Y;
            z = actor.Z;
            rotation = actor.RotBody;
        }

        var lines = new List<string>
        {
            $"X: {x}",
            $"Y: {y}",
            $"Z: {z.ToString("0.####", CultureInfo.InvariantCulture)}",
            $"Rotation: {rotation}",
        };

        if (session.AuthenticatedUser is { } account
            && await _fuses.HoldsAsync(account, RoomRightsService.AdministratorAccessFuse, cancellationToken: cancellationToken))
        {
            lines.Add(string.Empty);
            lines.Add($"Room ID: {room.Id}");
            lines.Add($"Room name: {room.Data.Label}");
            lines.Add($"Room model: {room.Data.HeightmapLabel}");
        }

        session.Send(ModMessageEvent.FromLines(lines).Compose());
    }
}
