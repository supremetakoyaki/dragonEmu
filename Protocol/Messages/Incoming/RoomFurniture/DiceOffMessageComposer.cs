using DragonEmu.Game.Furniture;

namespace DragonEmu.Protocol.Messages.Incoming.RoomFurniture;

public sealed class DiceOffMessageComposer : IMessageComposer
{
    private readonly FurnitureRollService _rolls;
    private int _itemId;

    public DiceOffMessageComposer(FurnitureRollService rolls)
    {
        _rolls = rolls;
    }

    public void Parse(ClientPacket packet)
    {
        _itemId = packet.PopInt32();
    }

    public async ValueTask HandleAsync(IGameSession session, CancellationToken cancellationToken) =>
        await _rolls.DiceOffAsync(session, _itemId, cancellationToken);
}
