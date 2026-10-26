using DragonEmu.Game.Furniture;

namespace DragonEmu.Protocol.Messages.Incoming.RoomFurniture;

public sealed class RoomDimmerChangeStateMessageComposer : IMessageComposer
{
    private readonly RoomDimmerService _dimmers;

    public RoomDimmerChangeStateMessageComposer(RoomDimmerService dimmers)
    {
        _dimmers = dimmers;
    }

    public void Parse(ClientPacket packet)
    {
    }

    public async ValueTask HandleAsync(IGameSession session, CancellationToken cancellationToken) =>
        await _dimmers.ToggleAsync(session, cancellationToken);
}
