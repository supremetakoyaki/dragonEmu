using DragonEmu.Domain;
using DragonEmu.Game.Rooms;
using DragonEmu.Persistence;
using DragonEmu.Protocol.Messages.Incoming.Quest;
using DragonEmu.Protocol.Messages.Outgoing.Quest;

namespace DragonEmu.Protocol.Messages.Incoming.RoomFurniture;

public sealed class QuestVendingWallItemMessageComposer : IMessageComposer
{
    private static readonly HashSet<string> Calendars = ["qt_calendar0", "qt_calendar1", "qt_calendar2"];

    private readonly IRoomManager _roomManager;
    private readonly IQuestRepository _quests;
    private int _itemId;

    public QuestVendingWallItemMessageComposer(IRoomManager roomManager, IQuestRepository quests)
    {
        _roomManager = roomManager;
        _quests = quests;
    }

    public void Parse(ClientPacket packet)
    {
        _itemId = packet.PopInt32();
    }

    public async ValueTask HandleAsync(IGameSession session, CancellationToken cancellationToken)
    {
        if (session.AuthenticatedUser is not { } account
            || session.CurrentRoomId is not { } roomId
            || !_roomManager.TryGetLoadedRoom(roomId, out Room? room)
            || room!.GetWallItem(_itemId) is not { } item
            || !Calendars.Contains(item.Classname))
        {
            return;
        }

        string campaign = OpenQuestListMessageComposer.DefaultCampaign;
        IReadOnlyList<QuestDefinition> quests = await _quests.GetCampaignAsync(campaign, account.Id, cancellationToken);
        DateTime now = DateTime.UtcNow;
        session.Send(new QuestListMessageEvent(campaign, quests.Select(quest => quest.ToInfo(now)).ToList()).Compose());
    }
}
