using DragonEmu.Game.Furniture;

namespace DragonEmu.Protocol.Messages.Incoming.RoomFurniture;

public sealed class RoomDimmerGetPresetsMessageComposer : IMessageComposer
{
    private readonly RoomDimmerService _dimmers;

    public RoomDimmerGetPresetsMessageComposer(RoomDimmerService dimmers)
    {
        _dimmers = dimmers;
    }

    public void Parse(ClientPacket packet)
    {
    }

    public async ValueTask HandleAsync(IGameSession session, CancellationToken cancellationToken) =>
        await _dimmers.SendPresetsAsync(session, cancellationToken);
}
