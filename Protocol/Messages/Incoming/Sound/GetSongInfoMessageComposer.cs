using DragonEmu.Game.Furniture;
using DragonEmu.Game.Rooms;

namespace DragonEmu.Protocol.Messages.Incoming.Sound;

public sealed class GetSongInfoMessageComposer : IMessageComposer
{
    private readonly IRoomManager _roomManager;
    private readonly TraxService _trax;

    private int _songId;

    public GetSongInfoMessageComposer(IRoomManager roomManager, TraxService trax)
    {
        _roomManager = roomManager;
        _trax = trax;
    }

    public void Parse(ClientPacket packet)
    {
        _songId = packet.PopInt32();
    }

    public ValueTask HandleAsync(IGameSession session, CancellationToken cancellationToken)
    {
        if (session.AuthenticatedUser is not null && session.CurrentRoomId is { } roomId && _roomManager.TryGetLoadedRoom(roomId, out Room? room))
        {
            _trax.SendSong(session, room!, _songId);
        }

        return ValueTask.CompletedTask;
    }
}
