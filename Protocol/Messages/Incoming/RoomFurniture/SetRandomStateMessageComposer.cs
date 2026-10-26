using DragonEmu.Game.Furniture;

namespace DragonEmu.Protocol.Messages.Incoming.RoomFurniture;

public sealed class SetRandomStateMessageComposer : IMessageComposer
{
    private readonly FurnitureRollService _rolls;
    private int _itemId;

    public SetRandomStateMessageComposer(FurnitureRollService rolls)
    {
        _rolls = rolls;
    }

    public void Parse(ClientPacket packet)
    {
        _itemId = packet.PopInt32();
        packet.PopInt32();
    }

    public async ValueTask HandleAsync(IGameSession session, CancellationToken cancellationToken) =>
        await _rolls.SetRandomStateAsync(session, _itemId, cancellationToken);
}
