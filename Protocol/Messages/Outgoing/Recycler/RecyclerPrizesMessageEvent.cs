using DragonEmu.Domain;

namespace DragonEmu.Protocol.Messages.Outgoing.Recycler;

public sealed class RecyclerPrizesMessageEvent : IMessageEvent
{
    private readonly IReadOnlyList<RecyclerLevel> _levels;

    public RecyclerPrizesMessageEvent(IReadOnlyList<RecyclerLevel> levels)
    {
        _levels = levels;
    }

    public ServerPacket Compose()
    {
        var packet = new ServerPacket(OutgoingOpcodes.Recycler.RecyclerPrizesMessageEvent);
        packet.AppendInt32(_levels.Count);

        foreach (RecyclerLevel level in _levels)
        {
            packet.AppendInt32(level.Level);
            packet.AppendInt32(level.Odds);
            packet.AppendInt32(level.Prizes.Count);

            foreach (RecyclerPrize prize in level.Prizes)
            {
                packet.AppendString(prize.Type == FurnitureType.Floor ? "s" : "i");
                packet.AppendInt32(prize.RefFurnidata);
            }
        }

        return packet;
    }
}
