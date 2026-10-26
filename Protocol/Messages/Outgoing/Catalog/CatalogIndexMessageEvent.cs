using DragonEmu.Domain;

namespace DragonEmu.Protocol.Messages.Outgoing.Catalog;

public sealed class CatalogIndexMessageEvent : IMessageEvent
{
    private const int RootPageId = -1;

    private readonly IReadOnlyList<CatalogPageSummary> _pages;
    private readonly int _accountRank;

    public CatalogIndexMessageEvent(IReadOnlyList<CatalogPageSummary> pages, int accountRank)
    {
        _pages = pages;
        _accountRank = accountRank;
        _childrenByParent = pages.Where(p => p.MinRank <= accountRank).ToLookup(p => p.ParentId);
    }

    public ServerPacket Compose()
    {
        var packet = new ServerPacket(OutgoingOpcodes.Catalog.CatalogIndexMessageEvent);

        packet.AppendInt32(0);
        packet.AppendInt32(0);
        packet.AppendInt32(0);
        packet.AppendInt32(RootPageId);
        packet.AppendString(string.Empty);
        packet.AppendInt32(0);
        packet.AppendInt32(CountChildren(RootPageId));

        foreach (CatalogPageSummary page in ChildrenOf(RootPageId))
        {
            AppendPage(packet, page);
        }

        return packet;
    }

    private void AppendPage(ServerPacket packet, CatalogPageSummary page)
    {
        packet.AppendInt32(1);
        packet.AppendInt32(page.Color);
        packet.AppendInt32(page.Icon);
        packet.AppendInt32(page.Id);
        packet.AppendString(page.DisplayName);
        packet.AppendInt32(page.ComingSoon ? 1 : 0);
        packet.AppendInt32(CountChildren(page.Id));

        foreach (CatalogPageSummary child in ChildrenOf(page.Id))
        {
            AppendPage(packet, child);
        }
    }

    private readonly ILookup<int, CatalogPageSummary> _childrenByParent;

    private IEnumerable<CatalogPageSummary> ChildrenOf(int parentId) => _childrenByParent[parentId];

    private int CountChildren(int parentId) => _childrenByParent[parentId].Count();
}
