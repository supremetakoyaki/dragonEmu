using DragonEmu.Domain;
using DragonEmu.Game.Achievements;
using DragonEmu.Game.Avatars;
using DragonEmu.Game.Messenger;
using DragonEmu.Game.Rooms;
using DragonEmu.Game.Fuses;
using DragonEmu.Persistence;
using DragonEmu.Protocol.Messages.Outgoing.RoomEngine;
using DragonEmu.Protocol.Messages.Outgoing.Notifications;
using DragonEmu.Game.Settings;

namespace DragonEmu.Protocol.Messages.Incoming.Register;

public sealed class UpdateFigureDataMessageComposer : IMessageComposer
{
    private const string ClubOutfitsFuse = "fuse_use_club_outfits";

    private readonly IUserRepository _userRepository;
    private readonly IRoomManager _roomManager;
    private readonly RoomBroadcaster _broadcaster;
    private readonly FigureDataCache _figureDataCache;
    private readonly FuseService _fuses;
    private readonly AchievementService _achievementService;
    private readonly MessengerService _messenger;
    private readonly MessageTexts _texts;

    private char _gender;
    private string _figure = string.Empty;

    public UpdateFigureDataMessageComposer(
        IUserRepository userRepository, IRoomManager roomManager, RoomBroadcaster broadcaster,
        FigureDataCache figureDataCache, FuseService fuses, AchievementService achievementService,
        MessengerService messenger, MessageTexts texts)
    {
        _texts = texts;
        _messenger = messenger;
        _userRepository = userRepository;
        _roomManager = roomManager;
        _broadcaster = broadcaster;
        _figureDataCache = figureDataCache;
        _fuses = fuses;
        _achievementService = achievementService;
    }

    public void Parse(ClientPacket packet)
    {
        string genderField = Sanitize(packet.PopString());
        _gender = genderField.Length > 0 ? char.ToUpperInvariant(genderField[0]) : '\0';
        _figure = Sanitize(packet.PopString());
    }

    public async ValueTask HandleAsync(IGameSession session, CancellationToken cancellationToken)
    {
        if (session.AuthenticatedUser is not { } account || _figure.Length == 0 || _gender is not ('M' or 'F'))
        {
            return;
        }

        bool isClubMember = await _fuses.HoldsAsync(account, ClubOutfitsFuse, cancellationToken: cancellationToken);
        string? sanitizedFigure = FigureSanitizer.Sanitize(_figureDataCache, _figure, _gender, isClubMember);
        if (sanitizedFigure is null)
        {
            return;
        }

        if (!session.TryUseCooldown(ActionCooldowns.Figure, ActionCooldowns.Default))
        {
            session.Send(new HabboBroadcastMessageEvent(_texts.Get(MessageKeys.ActionTooFast)).Compose());
            return;
        }

        await _userRepository.UpdateFigureAsync(account.Id, sanitizedFigure, _gender, cancellationToken);
        await _achievementService.IncrementAsync(account.Id, "ACH_AvatarLooks", 1, cancellationToken);

        UserAccount updated = account with { Figure = sanitizedFigure, Gender = _gender };
        session.Authenticate(updated);

        session.Send(new UserChangeMessageEvent(-1, updated.Figure, updated.Gender, updated.Motto).Compose());
        await _messenger.AnnounceAsync(account.Id, cancellationToken);

        if (session.CurrentRoomId is not { } roomId)
        {
            return;
        }

        Room? room = await _roomManager.GetOrLoadRoomAsync(roomId, cancellationToken);
        if (room is null || !room.TryGetActor(session.Id, out RoomActor? actor))
        {
            return;
        }

        lock (actor!.Lock)
        {
            actor.Figure = updated.Figure;
            actor.Gender = updated.Gender;
        }

        _broadcaster.Broadcast(room, new UserChangeMessageEvent(actor.VirtualId, updated.Figure, updated.Gender, updated.Motto).Compose());

        if (actor.HasSwimwear)
        {
            _broadcaster.Broadcast(room, new UsersMessageEvent([actor]).Compose());
        }
    }

    private static string Sanitize(string value) =>
        new(value.Where(c => c is not (char)0x01 and not (char)0x02).ToArray());
}
