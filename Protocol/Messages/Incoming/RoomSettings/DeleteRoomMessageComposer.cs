using DragonEmu.Game.Rooms;

namespace DragonEmu.Protocol.Messages.Incoming.RoomSettings;

public sealed class DeleteRoomMessageComposer : IMessageComposer
{
    private readonly RoomDeletionService _roomDeletion;

    private int _roomId;

    public DeleteRoomMessageComposer(RoomDeletionService roomDeletion)
    {
        _roomDeletion = roomDeletion;
    }

    public void Parse(ClientPacket packet)
    {
        _roomId = packet.PopInt32();
    }

    public async ValueTask HandleAsync(IGameSession session, CancellationToken cancellationToken)
    {
        if (session.AuthenticatedUser is { } account)
        {
            await _roomDeletion.DeleteAsync(account, _roomId, cancellationToken);
        }
    }
}
