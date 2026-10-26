using DragonEmu.Domain;
using DragonEmu.Game.Chat;
using DragonEmu.Game.Pets;
using DragonEmu.Game.Rooms;
using DragonEmu.Persistence;
using DragonEmu.Protocol.Messages.Outgoing.Navigator;
using DragonEmu.Protocol.Messages.Outgoing.RoomSettings;
using DragonEmu.Protocol.Messages.Outgoing.Notifications;
using DragonEmu.Protocol.Messages.Incoming.Register;
using DragonEmu.Game.Settings;

namespace DragonEmu.Protocol.Messages.Incoming.RoomSettings;

public sealed class SaveRoomSettingsMessageComposer : IMessageComposer
{
    private const int DefaultCategoryId = 1;
    private const int MaxNameLength = 100;
    private const int MaxDescriptionLength = 255;
    private const int MaxPasswordLength = 255;
    private const int MaxTagLength = 50;
    private const int MaxTagCount = 2;
    private static readonly int[] ValidMaxUsers = [10, 15, 20, 25];

    private readonly IRoomManager _roomManager;
    private readonly PublicRoomCache _publicRooms;
    private readonly IRoomCategoryRepository _roomCategoryRepository;
    private readonly IPrivateRoomRepository _privateRoomRepository;
    private readonly RoomRightsService _roomRights;
    private readonly ProfanityFilterCache _profanityFilterCache;
    private readonly RoomBroadcaster _roomBroadcaster;
    private readonly PetService _pets;

    private int _roomId;
    private string _name = string.Empty;
    private string _description = string.Empty;
    private int _accessType;
    private string _password = string.Empty;
    private int _maxUsers;
    private int _categoryId;
    private readonly List<string> _tags = [];
    private readonly MessageTexts _texts;
    private bool _tooManyTags;
    private bool _allowPets;
    private bool _allowPetsEat;
    private bool _allowWalkthrough;

    public SaveRoomSettingsMessageComposer(
        IRoomManager roomManager, PublicRoomCache publicRooms,
        IRoomCategoryRepository roomCategoryRepository, IPrivateRoomRepository privateRoomRepository, RoomRightsService roomRights,
        ProfanityFilterCache profanityFilterCache, RoomBroadcaster roomBroadcaster, PetService pets, MessageTexts texts)
    {
        _texts = texts;
        _pets = pets;
        _roomBroadcaster = roomBroadcaster;
        _roomManager = roomManager;
        _publicRooms = publicRooms;
        _roomCategoryRepository = roomCategoryRepository;
        _privateRoomRepository = privateRoomRepository;
        _roomRights = roomRights;
        _profanityFilterCache = profanityFilterCache;
    }

    public void Parse(ClientPacket packet)
    {
        _roomId = packet.PopInt32();
        _name = RoomTextSanitizer.Sanitize(packet.PopString(), MaxNameLength);
        _description = RoomTextSanitizer.Sanitize(packet.PopString(), MaxDescriptionLength);
        _accessType = packet.PopInt32();
        _password = RoomTextSanitizer.Sanitize(packet.PopString(), MaxPasswordLength);
        _maxUsers = packet.PopInt32();
        _categoryId = packet.PopInt32();

        int tagCount = packet.PopInt32();
        for (int i = 0; i < tagCount && packet.RemainingLength >= 2; i++)
        {
            string tag = RoomTextSanitizer.Sanitize(packet.PopString(), MaxTagLength).ToLowerInvariant();
            if (tag.Length > 0)
            {
                _tags.Add(tag);
            }
        }

        _tooManyTags = tagCount > MaxTagCount;

        _allowPets = packet.PopBoolean();
        _allowPetsEat = packet.PopBoolean();
        _allowWalkthrough = packet.PopBoolean();
    }

    public async ValueTask HandleAsync(IGameSession session, CancellationToken cancellationToken)
    {
        if (session.AuthenticatedUser is not { } account)
        {
            return;
        }

        if (session.CurrentRoomId != _roomId || !_roomManager.TryGetLoadedRoom(_roomId, out Room? room))
        {
            return;
        }

        if (_publicRooms.IsPublic(_roomId))
        {
            return;
        }

        if (!await _roomRights.CanEditAsync(account, room!.Data, cancellationToken))
        {
            session.Send(new RoomSettingsSaveErrorEvent(_roomId, RoomSettingsSaveErrorEvent.Forbidden).Compose());
            return;
        }

        if (string.IsNullOrWhiteSpace(_name))
        {
            session.Send(new RoomSettingsSaveErrorEvent(_roomId, RoomSettingsSaveErrorEvent.NameMandatory).Compose());
            return;
        }

        if (_accessType is < 0 or > 2)
        {
            return;
        }

        if (!ValidMaxUsers.Contains(_maxUsers))
        {
            return;
        }

        if (_tooManyTags)
        {
            return;
        }

        string? passwordHash = null;
        if (_accessType == RoomAccess.Password)
        {
            if (_password.Length > 0)
            {
                if (_profanityFilterCache.IsUnacceptable(_password))
                {
                    session.Send(new RoomSettingsSaveErrorEvent(_roomId, RoomSettingsSaveErrorEvent.UnacceptableWordsInName).Compose());
                    return;
                }

                passwordHash = RoomPassword.Hash(_password);
            }
            else if (room.Data.AccessType == RoomAccess.Password && RoomPassword.IsHash(room.Data.Password))
            {
                passwordHash = room.Data.Password;
            }
            else
            {
                session.Send(new RoomSettingsSaveErrorEvent(_roomId, RoomSettingsSaveErrorEvent.PasswordMandatory).Compose());
                return;
            }
        }

        IReadOnlyList<RoomCategory> categories = await _roomCategoryRepository.GetAllAsync(cancellationToken);
        RoomCategory? category = categories.FirstOrDefault(c => c.Id == _categoryId);
        int resolvedCategoryId = category is not null && category.RequiredRights <= account.Rank ? category.Id : DefaultCategoryId;

        if (!session.TryUseCooldown(ActionCooldowns.RoomSettings, ActionCooldowns.Default))
        {
            session.Send(new HabboBroadcastMessageEvent(_texts.Get(MessageKeys.ActionTooFast)).Compose());
            return;
        }

        string filteredName = _profanityFilterCache.Filter(_name);
        string filteredDescription = _profanityFilterCache.Filter(_description);
        List<string> filteredTags = _tags.Select(_profanityFilterCache.Filter).ToList();

        PrivateRoomSummary updated = await _privateRoomRepository.UpdateSettingsAsync(
            _roomId, filteredName, filteredDescription, resolvedCategoryId, _accessType, passwordHash, _maxUsers,
            filteredTags, _allowPets, _allowPetsEat, _allowWalkthrough, cancellationToken);

        room.ApplySettingsUpdate(updated);

        if (!updated.AllowPets)
        {
            _pets.SendGuestPetsHome(room);
        }

        if (updated.AccessType != RoomAccess.Open)
        {
            room.EndEvent();
        }

        session.Send(new RoomSettingsSavedEvent(_roomId).Compose());

        session.Send(new RoomInfoUpdatedEvent(_roomId).Compose());
        _roomBroadcaster.Broadcast(room, new RoomInfoUpdatedEvent(_roomId).Compose(), excludeSessionId: session.Id);
    }
}
