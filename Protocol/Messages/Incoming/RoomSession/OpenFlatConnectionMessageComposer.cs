using DragonEmu.Game.Settings;
using DragonEmu.Game.Achievements;
using DragonEmu.Game.Furniture;
using DragonEmu.Game.Rooms;
using DragonEmu.Game.Subscriptions;
using DragonEmu.Persistence;
using DragonEmu.Protocol.Messages.Incoming.RoomSession.AuxiliaryFunctions;

namespace DragonEmu.Protocol.Messages.Incoming.RoomSession;

public sealed class OpenFlatConnectionMessageComposer : IMessageComposer
{
    private readonly IRoomManager _roomManager;
    private readonly IUserItemRepository _userItemRepository;
    private readonly PublicRoomCache _publicRooms;
    private readonly SubscriptionManager _subscriptionManager;
    private readonly IRoomVoteRepository _roomVoteRepository;
    private readonly RoomRightsService _roomRights;
    private readonly GameSettingsCache _gameSettings;
    private readonly RoomBroadcaster _roomBroadcaster;
    private readonly FurnitureAliasCache _furnitureAliasCache;
    private readonly IRoomVisitRepository _roomVisitRepository;
    private readonly AchievementService _achievementService;
    private readonly RoomTeleportService _teleportService;

    private int _roomId;
    private string _password = string.Empty;

    public OpenFlatConnectionMessageComposer(
        IRoomManager roomManager, IUserItemRepository userItemRepository,
        PublicRoomCache publicRooms, SubscriptionManager subscriptionManager, IRoomVoteRepository roomVoteRepository,
        RoomRightsService roomRights, GameSettingsCache gameSettings, RoomBroadcaster roomBroadcaster, FurnitureAliasCache furnitureAliasCache,
        IRoomVisitRepository roomVisitRepository, AchievementService achievementService, RoomTeleportService teleportService)
    {
        _roomManager = roomManager;
        _userItemRepository = userItemRepository;
        _publicRooms = publicRooms;
        _subscriptionManager = subscriptionManager;
        _roomVoteRepository = roomVoteRepository;
        _roomRights = roomRights;
        _gameSettings = gameSettings;
        _roomBroadcaster = roomBroadcaster;
        _furnitureAliasCache = furnitureAliasCache;
        _roomVisitRepository = roomVisitRepository;
        _achievementService = achievementService;
        _teleportService = teleportService;
    }

    public void Parse(ClientPacket packet)
    {
        _roomId = packet.PopInt32();
        _password = packet.PopString();
        packet.PopInt32();
    }

    public ValueTask HandleAsync(IGameSession session, CancellationToken cancellationToken) =>
        RoomEntryHandler.EnterAsync(
            session, _roomManager, _userItemRepository, _publicRooms, _subscriptionManager, _roomVoteRepository,
            _roomRights, _gameSettings, _roomBroadcaster, _furnitureAliasCache, _roomVisitRepository, _achievementService,
            _teleportService, _roomId, _password, cancellationToken, startsNewSession: true);
}
