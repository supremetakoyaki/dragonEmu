using DragonEmu.Game.Furniture;
using DragonEmu.Game.Rooms;

namespace DragonEmu.Protocol.Messages.Incoming.Sound;

public sealed class GetSoundMachinePlayListMessageComposer : IMessageComposer
{
    private readonly IRoomManager _roomManager;
    private readonly TraxService _trax;

    public GetSoundMachinePlayListMessageComposer(IRoomManager roomManager, TraxService trax)
    {
        _roomManager = roomManager;
        _trax = trax;
    }

    public void Parse(ClientPacket packet)
    {
    }

    public ValueTask HandleAsync(IGameSession session, CancellationToken cancellationToken)
    {
        if (session.AuthenticatedUser is not null && session.CurrentRoomId is { } roomId && _roomManager.TryGetLoadedRoom(roomId, out Room? room))
        {
            _trax.SendPlaylist(session, room!);
        }

        return ValueTask.CompletedTask;
    }
}
