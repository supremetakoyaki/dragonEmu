using DragonEmu.Domain;
using DragonEmu.Game.Chat;
using DragonEmu.Game.Fuses;
using DragonEmu.Game.Rooms;
using DragonEmu.Persistence;
using DragonEmu.Protocol.Messages.Outgoing.Navigator;

namespace DragonEmu.Protocol.Messages.Incoming.Navigator;

public sealed class CreateFlatMessageComposer : IMessageComposer
{
    private const int DefaultCategoryId = 1;
    private const int MaxRoomNameLength = 100;
    private const string ClubModelsFuse = "fuse_use_special_room_layouts";

    private static readonly HashSet<string> ClientModels =
        [.. Enumerable.Range('a', 'r' - 'a' + 1).Select(letter => "model_" + (char)letter)];

    private readonly IPrivateRoomRepository _privateRoomRepository;
    private readonly IRoomHeightmapRepository _roomHeightmapRepository;
    private readonly ProfanityFilterCache _profanityFilterCache;
    private readonly FuseService _fuses;
    private readonly RoomCreationLimit _limit;

    private string _roomName = string.Empty;
    private string _heightmapLabel = string.Empty;

    public CreateFlatMessageComposer(
        IPrivateRoomRepository privateRoomRepository, IRoomHeightmapRepository roomHeightmapRepository, ProfanityFilterCache profanityFilterCache,
        FuseService fuses, RoomCreationLimit limit)
    {
        _privateRoomRepository = privateRoomRepository;
        _roomHeightmapRepository = roomHeightmapRepository;
        _profanityFilterCache = profanityFilterCache;
        _fuses = fuses;
        _limit = limit;
    }

    public void Parse(ClientPacket packet)
    {
        _roomName = RoomTextSanitizer.Sanitize(packet.PopString(), MaxRoomNameLength);
        _heightmapLabel = packet.PopString();
        packet.PopString();
    }

    public async ValueTask HandleAsync(IGameSession session, CancellationToken cancellationToken)
    {
        if (session.AuthenticatedUser is not { } account)
        {
            return;
        }

        if (!ClientModels.Contains(_heightmapLabel)
            || await _roomHeightmapRepository.GetByLabelAsync(_heightmapLabel, cancellationToken) is not { } model
            || (model.Club && !await _fuses.HoldsAsync(account, ClubModelsFuse, cancellationToken: cancellationToken))
            || !await _limit.MayCreateAsync(account, cancellationToken))
        {
            return;
        }

        PrivateRoomSummary room = await _privateRoomRepository.CreateAsync(
            account.Id, _profanityFilterCache.Filter(_roomName), description: string.Empty, DefaultCategoryId, model.Id, cancellationToken);

        session.Send(new FlatCreatedEvent(room.Id, room.Label).Compose());
    }
}
