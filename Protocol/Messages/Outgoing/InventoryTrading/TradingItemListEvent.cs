using DragonEmu.Domain;
using DragonEmu.Protocol.Messages.Outgoing.InventoryFurni;

namespace DragonEmu.Protocol.Messages.Outgoing.InventoryTrading;

public sealed class TradingItemListEvent : IMessageEvent
{
    private readonly int _userId;
    private readonly IReadOnlyList<UserItem> _items;
    private readonly int _otherUserId;
    private readonly IReadOnlyList<UserItem> _otherItems;

    public TradingItemListEvent(int userId, IReadOnlyList<UserItem> items, int otherUserId, IReadOnlyList<UserItem> otherItems)
    {
        _userId = userId;
        _items = items;
        _otherUserId = otherUserId;
        _otherItems = otherItems;
    }

    public ServerPacket Compose()
    {
        var packet = new ServerPacket(OutgoingOpcodes.InventoryTrading.TradingItemListEvent);
        AppendSide(packet, _userId, _items);
        AppendSide(packet, _otherUserId, _otherItems);
        return packet;
    }

    private static void AppendSide(ServerPacket packet, int userId, IReadOnlyList<UserItem> items)
    {
        packet.AppendInt32(userId);
        packet.AppendInt32(items.Count);
        foreach (UserItem item in items)
        {
            AppendItem(packet, item);
        }
    }

    private static void AppendItem(ServerPacket packet, UserItem item)
    {
        bool isFloor = item.Type == FurnitureType.Floor;
        int category = UserItemWireFormat.CategoryOf(item);

        packet.AppendInt32(item.Id);
        packet.AppendString(isFloor ? "S" : "I");
        packet.AppendInt32(item.Id);
        packet.AppendInt32(item.RefFurnidata);
        packet.AppendInt32(category);
        packet.AppendBoolean(UserItemWireFormat.IsGroupable(item, category));
        packet.AppendString(item.ExtraData ?? string.Empty);
        packet.AppendInt32(0);
        packet.AppendInt32(0);
        packet.AppendInt32(0);

        if (isFloor)
        {
            packet.AppendInt32(item.Extras ?? -1);
        }
    }
}
