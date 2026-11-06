using System.Globalization;
using System.Security.Cryptography;
using DragonEmu.Domain;
using DragonEmu.Game.Achievements;
using DragonEmu.Gateway;
using DragonEmu.Game.Chat;
using DragonEmu.Game.Furniture;
using DragonEmu.Game.Rooms;
using DragonEmu.Game.Settings;
using DragonEmu.Persistence;
using DragonEmu.Protocol.Messages;
using DragonEmu.Protocol.Messages.Outgoing.Camera;
using DragonEmu.Protocol.Messages.Outgoing.InventoryFurni;
using DragonEmu.Protocol.Messages.Outgoing.RoomAction;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace DragonEmu.Game.Camera;

public sealed class CameraService : IHostedService
{
    public const int HandItem = 20;
    public const int MaxCaptionLength = 105;
    public const string PhotoAchievement = "ACH_CameraPhotoCount";

    public const string TimeFormat = "dd/MM/yy HH:mm";

    private static readonly TimeSpan SaveInterval = TimeSpan.FromSeconds(3);

    private readonly IRoomManager _roomManager;
    private readonly ClientSessionManager _sessions;
    private readonly RoomBroadcaster _broadcaster;
    private readonly FurnitureDefinitionCache _definitions;
    private readonly IUserItemRepository _userItems;
    private readonly ICurrencyRepository _currency;
    private readonly IPhotoRepository _photos;
    private readonly ProfanityFilterCache _profanity;
    private readonly HotelClock _clock;
    private readonly AchievementService _achievements;
    private readonly ILogger<CameraService> _logger;

    public CameraService(
        IRoomManager roomManager, ClientSessionManager sessions, RoomBroadcaster broadcaster, FurnitureDefinitionCache definitions,
        IUserItemRepository userItems, ICurrencyRepository currency, IPhotoRepository photos, ProfanityFilterCache profanity, HotelClock clock,
        AchievementService achievements, ILogger<CameraService> logger)
    {
        _roomManager = roomManager;
        _sessions = sessions;
        _broadcaster = broadcaster;
        _definitions = definitions;
        _userItems = userItems;
        _currency = currency;
        _photos = photos;
        _profanity = profanity;
        _clock = clock;
        _achievements = achievements;
        _logger = logger;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _roomManager.RoomLoaded += Watch;
        foreach (Room room in _roomManager.LoadedRooms)
        {
            Watch(room);
        }

        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _roomManager.RoomLoaded -= Watch;
        return Task.CompletedTask;
    }

    public void WindowsLoaded(IGameSession session)
    {
        session.Send(new CameraFurniMessageEvent([.. _definitions.WithLogicClass(FurnitureLogicClass.Camera).Select(camera => camera.RefFurnidata)]).Compose());
    }

    public async Task CarryAsync(IGameSession session, UserAccount account, int item, CancellationToken cancellationToken)
    {
        if (!TryGetActor(session, out Room? room, out RoomActor? actor))
        {
            return;
        }

        if (item == 0)
        {
            PutAway(session, room, actor);
            return;
        }

        if (item != HandItem)
        {
            return;
        }

        int[] cameras = [.. _definitions.WithLogicClass(FurnitureLogicClass.Camera).Select(camera => camera.Id)];
        long film = await _currency.GetBalanceAsync(account.Id, CurrencyType.Film, cancellationToken);
        if (!await _userItems.HasInInventoryAsync(account.Id, cameras, cancellationToken))
        {
            session.Send(new FilmMessageEvent((int)film, string.Empty).Compose());
            return;
        }

        room.GiveHandItem(actor, HandItem, held: true);
        _broadcaster.Broadcast(room, new CarryObjectMessageEvent(actor.VirtualId, HandItem).Compose());
        session.Send(new FilmMessageEvent((int)film, session.Camera.NewToken()).Compose());
    }

    public void Use(IGameSession session, int item)
    {
        if ((item == 0 || item == HandItem) && TryGetActor(session, out Room? room, out RoomActor? actor) && HoldsCamera(actor))
        {
            _broadcaster.Broadcast(room, new UseObjectMessageEvent(actor.VirtualId, item).Compose());
        }
    }

    public async Task SaveAsync(
        IGameSession session, UserAccount account, uint checksum, string caption, string picture, CancellationToken cancellationToken)
    {
        string? token = session.Camera.TakeToken();
        if (token is null || !TryGetActor(session, out Room? room, out RoomActor? actor))
        {
            return;
        }

        bool saved = false;
        if (!session.Camera.TrySave(DateTime.UtcNow, SaveInterval))
        {
            _logger.LogInformation("Camera: {User}'s photo came too soon after the last.", account.Username);
        }
        else if (!CameraPicture.TryRead(picture, out byte[] pixels) || CameraPicture.Checksum(token, pixels) != checksum)
        {
            _logger.LogWarning("Camera: {User} sent a photo that isn't the camera's (picture or checksum).", account.Username);
        }
        else if (_definitions.WithLogicClass(FurnitureLogicClass.Photo).FirstOrDefault() is not { } photoDefinition)
        {
            _logger.LogWarning("Camera: no photo furni (logic_class 'photo') to make the photo with.");
        }
        else if (await _currency.TrySpendAsync(account.Id, CurrencyType.Film, 1, cancellationToken) is not null)
        {
            string text = _profanity.Filter(RoomTextSanitizer.Sanitize(caption.Replace('\r', ' ').Replace('\n', ' ').Replace('\t', ' '), MaxCaptionLength).Trim());
            int itemId = await _photos.CreateAsync(
                account.Id, photoDefinition.Id, room.Id, text, RandomNumberGenerator.GetHexString(16, lowercase: true),
                CameraPicture.ToPng(pixels), DateTime.UtcNow.ToWholeSecond(), cancellationToken);
            if (await _userItems.GetByIdAsync(itemId, cancellationToken) is { } item)
            {
                session.Send(new FurniListInsertEvent(item).Compose());
            }

            saved = true;
        }

        long film = await _currency.GetBalanceAsync(account.Id, CurrencyType.Film, cancellationToken);
        session.Send(new FilmMessageEvent((int)film, HoldsCamera(actor) ? session.Camera.NewToken() : string.Empty).Compose());
        session.Send(new PhotoSavedMessageEvent(saved).Compose());
        if (saved)
        {
            await _achievements.IncrementAsync(account.Id, PhotoAchievement, cancellationToken: cancellationToken);
        }
    }

    public async Task ViewAsync(IGameSession session, int itemId, CancellationToken cancellationToken)
    {
        Photo? photo = await _photos.GetByItemAsync(itemId, cancellationToken);
        string time = photo is null ? string.Empty : _clock.ToHotelTime(photo.TakenAt).ToString(TimeFormat, CultureInfo.InvariantCulture);
        session.Send(new PhotoViewMessageEvent(itemId, photo?.Path ?? string.Empty, time, photo?.Caption ?? string.Empty).Compose());
    }

    public void FilmChanged(IGameSession session, long film) =>
        session.Send(new FilmMessageEvent((int)film, session.Camera.IsOut ? session.Camera.NewToken() : string.Empty).Compose());

    private void PutAway(IGameSession session, Room room, RoomActor actor)
    {
        session.Camera.PutAway();
        if (!HoldsCamera(actor))
        {
            return;
        }

        room.GiveHandItem(actor, 0);
        _broadcaster.Broadcast(room, new UseObjectMessageEvent(actor.VirtualId, 0).Compose());
        _broadcaster.Broadcast(room, new CarryObjectMessageEvent(actor.VirtualId, 0).Compose());
    }

    private void Watch(Room room)
    {
        room.HeldHandItemLost -= OnHeldHandItemLost;
        room.HeldHandItemLost += OnHeldHandItemLost;
    }

    private void OnHeldHandItemLost(Room room, RoomActor actor)
    {
        if (_sessions.TryGet(actor.SessionId, out ClientSession? session) && session is not null && session.Camera.IsOut)
        {
            session.Camera.PutAway();
            _ = TellPutAwayAsync(session, actor.AccountId);
        }
    }

    private async Task TellPutAwayAsync(IGameSession session, int userId)
    {
        try
        {
            long film = await _currency.GetBalanceAsync(userId, CurrencyType.Film, CancellationToken.None);
            session.Send(new FilmMessageEvent((int)film, string.Empty).Compose());
        }
        catch (Exception exception)
        {
            _logger.LogWarning(exception, "Camera: couldn't tell session {SessionId} its camera was put away.", session.Id);
        }
    }

    private static bool HoldsCamera(RoomActor actor)
    {
        lock (actor.Lock)
        {
            return actor.CarryItemId == HandItem;
        }
    }

    private bool TryGetActor(IGameSession session, [System.Diagnostics.CodeAnalysis.NotNullWhen(true)] out Room? room,
        [System.Diagnostics.CodeAnalysis.NotNullWhen(true)] out RoomActor? actor)
    {
        actor = null;
        room = null;
        return session.CurrentRoomId is { } roomId && _roomManager.TryGetLoadedRoom(roomId, out room) && room is not null
            && room.TryGetActor(session.Id, out actor) && actor is not null;
    }
}
