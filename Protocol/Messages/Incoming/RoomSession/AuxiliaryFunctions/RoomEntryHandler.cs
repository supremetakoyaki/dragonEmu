using DragonEmu.Game.Settings;
using DragonEmu.Domain;
using DragonEmu.Game.Achievements;
using DragonEmu.Game.Furniture;
using DragonEmu.Game.Rooms;
using DragonEmu.Game.Subscriptions;
using DragonEmu.Persistence;
using DragonEmu.Protocol.Messages.Outgoing.Handshake;
using DragonEmu.Protocol.Messages.Outgoing.Navigator;
using DragonEmu.Protocol.Messages.Outgoing.RoomAction;
using DragonEmu.Protocol.Messages.Outgoing.RoomEngine;
using DragonEmu.Protocol.Messages.Outgoing.RoomPermissions;
using DragonEmu.Protocol.Messages.Outgoing.RoomPublicRoom;
using DragonEmu.Protocol.Messages.Outgoing.RoomSession;
using DragonEmu.Protocol.Messages.Outgoing.Users;

namespace DragonEmu.Protocol.Messages.Incoming.RoomSession.AuxiliaryFunctions;

internal static class RoomEntryHandler
{
    private const string EnterFullRoomsFuse = "fuse_enter_full_rooms";
    private const string EnterLockedRoomsFuse = "fuse_enter_locked_rooms";

    private const string InfobusHeightmapLabel = "park_a";

    public static async ValueTask EnterAsync(
        IGameSession session, IRoomManager roomManager, IUserItemRepository userItemRepository,
        PublicRoomCache publicRooms, SubscriptionManager subscriptionManager,
        IRoomVoteRepository roomVoteRepository, RoomRightsService roomRights, GameSettingsCache gameSettings,
        RoomBroadcaster roomBroadcaster, FurnitureAliasCache furnitureAliasCache, IRoomVisitRepository roomVisitRepository,
        AchievementService achievementService, RoomTeleportService teleportService, int roomId, string password,
        CancellationToken cancellationToken, bool startsNewSession = false)
    {
        if (session.AuthenticatedUser is not { } account)
        {
            return;
        }

        PendingTeleportArrival? teleportArrival =
            session.PendingTeleport is { } pending && pending.IsFor(roomId, DateTime.UtcNow) ? pending : null;
        session.PendingTeleport = null;
        PendingArrival? tileArrival = session.PendingArrival is { } arrival && arrival.IsFor(roomId, DateTime.UtcNow) ? arrival : null;
        session.PendingArrival = null;

        bool letIn = session.LetInto == roomId;
        session.LetInto = null;
        session.RingingAt = null;

        if (session.CurrentRoomId == roomId && !startsNewSession)
        {
            return;
        }

        session.SetLoadingRoom(roomId);

        Room? room = await roomManager.GetOrLoadRoomAsync(roomId, cancellationToken);
        if (room is null)
        {
            session.SetLoadingRoom(null);
            return;
        }

        if (session.CurrentRoomId is { } previousRoomId
            && roomManager.TryGetLoadedRoom(previousRoomId, out Room? previousRoom)
            && previousRoom!.TryRemoveActor(session.Id, out RoomActor? leftActor))
        {
            roomBroadcaster.Broadcast(previousRoom, new UserRemoveMessageEvent(leftActor!.VirtualId).Compose());
            session.SetCurrentRoom(null);
        }

        (bool infobusOpen, int? infobusRoomRef) = (gameSettings.Current.InfobusOpen, gameSettings.Current.InfobusRoomRef);
        if (!infobusOpen && roomId == infobusRoomRef)
        {
            session.Send(new ParkBusCannotEnterMessageEvent().Compose());
            session.Send(new CloseConnectionMessageEvent().Compose());
            session.SetLoadingRoom(null);
            return;
        }

        bool isOwner = RoomRightsService.IsOwner(account, room.Data);

        RoomFloorItem? arrivalTeleporter = teleportArrival is not null
            ? teleportService.FindArrivalTeleporter(room, teleportArrival, session.Id)
            : null;

        bool isPublic = publicRooms.IsPublic(roomId);
        NavigatorPublicRoom? publicEntry = publicRooms.Find(roomId);

        if (publicEntry is { ClubOnly: true } && !await subscriptionManager.IsClubMemberAsync(account.Id, cancellationToken))
        {
            session.Send(CantConnectMessageEvent.ClubOnly().Compose());
            session.Send(new CloseConnectionMessageEvent().Compose());
            session.SetLoadingRoom(null);
            return;
        }

        if (await roomRights.IsBannedAsync(account.Id, roomId, cancellationToken))
        {
            session.Send(new CantConnectMessageEvent(CantConnectMessageEvent.Banned).Compose());
            session.Send(new CloseConnectionMessageEvent().Compose());
            session.SetLoadingRoom(null);
            return;
        }

        if (!isOwner && room.UserCount >= room.Data.MaxUsers
            && !await roomRights.HoldsFuseAsync(account, EnterFullRoomsFuse, cancellationToken))
        {
            session.Send(new CantConnectMessageEvent(1).Compose());
            session.Send(new CloseConnectionMessageEvent().Compose());
            session.SetLoadingRoom(null);
            return;
        }

        bool throughTheDoor = room.Data.AccessType == RoomAccess.Open || isPublic || isOwner
            || arrivalTeleporter is not null || letIn
            || await roomRights.HoldsFuseAsync(account, EnterLockedRoomsFuse, cancellationToken);

        if (!throughTheDoor && room.Data.AccessType == RoomAccess.Password && !RoomPassword.Matches(password, room.Data.Password))
        {
            session.Send(new GenericErrorEvent(-100002).Compose());
            session.Send(new CloseConnectionMessageEvent().Compose());
            session.SetLoadingRoom(null);
            return;
        }

        if (!throughTheDoor && room.Data.AccessType == RoomAccess.Doorbell)
        {
            if (roomBroadcaster.SendToRightsHolders(room, new DoorbellMessageEvent(account.Username).Compose()) > 0)
            {
                session.RingingAt = roomId;
                session.Send(new DoorbellMessageEvent().Compose());
            }
            else
            {
                session.Send(new FlatAccessDeniedMessageEvent().Compose());
            }

            session.SetLoadingRoom(null);
            return;
        }

        (int spawnX, int spawnY, double spawnZ, int spawnDir) = arrivalTeleporter is { } teleporter
            ? (teleporter.X, teleporter.Y, teleporter.Z, teleporter.Direction)
            : tileArrival is { } tile && isPublic && room.Grid.IsWalkable(tile.X, tile.Y)
                ? (tile.X, tile.Y, room.SurfaceHeightAt(tile.X, tile.Y), tile.Rotation)
                : (room.Data.DoorX, room.Data.DoorY, room.Data.DoorZ, room.Data.DoorDir);

        bool hasGrantedRights = !isPublic && !isOwner && await roomRights.HasGrantedRightsAsync(account.Id, roomId, cancellationToken);

        var actor = new RoomActor(
            SessionId: session.Id,
            VirtualId: room.NextVirtualId(),
            Type: ActorType.Player,
            AccountId: account.Id,
            Username: account.Username,
            Motto: account.Motto,
            Figure: account.Figure,
            Gender: account.Gender,
            X: spawnX,
            Y: spawnY,
            Z: spawnZ)
        {
            RotBody = spawnDir,
            RotHead = spawnDir,
            EffectId = session.AvatarEffectId,
            FavouriteGroup = account.FavouriteGroup,

            Swimsuit = room.IsPool ? session.Swimsuit : null,
            FlatControl = !isPublic && isOwner ? FlatControlStatus.Owner : hasGrantedRights ? FlatControlStatus.RightsHolder : null,
        };

        actor.IsSwimming = room.IsInWater(actor, spawnX, spawnY);
        session.Swimsuit = actor.Swimsuit;
        room.TryAddActor(actor);
        session.SetCurrentRoom(roomId);
        session.SetLoadingRoom(null);

        if (!isPublic)
        {
            bool firstVisit = await roomVisitRepository.RecordVisitAsync(account.Id, roomId, cancellationToken);
            if (firstVisit && !isOwner)
            {
                await achievementService.IncrementAsync(account.Id, "ACH_RoomEntry", cancellationToken: cancellationToken);
            }
        }

        if (!isPublic)
        {
            session.Send(new OpenConnectionMessageEvent().Compose());
        }

        session.Send(new RoomReadyMessageEvent(room.Data.HeightmapLabel, roomId).Compose());

        if (!isPublic)
        {
            if (isOwner || hasGrantedRights)
            {
                session.Send(new YouAreControllerMessageEvent().Compose());
            }

            if (isOwner)
            {
                session.Send(new YouAreOwnerMessageEvent().Compose());
            }

            int rating = -1;
            if (isOwner || await roomVoteRepository.HasVotedAsync(account.Id, roomId, cancellationToken))
            {
                rating = await roomVoteRepository.GetScoreAsync(roomId, cancellationToken);
            }

            session.Send(new RoomRatingEvent(rating).Compose());
            session.Send(new RoomEventEvent(roomId, room.Event).Compose());
        }

        session.Send(new HeightMapMessageEvent(room.StackingMap()).Compose());
        session.Send(new FloorHeightMapMessageEvent(room.Data.Heightmap, room.Data.DoorX, room.Data.DoorY, room.Data.DoorZ).Compose());

        if (!isPublic)
        {
            session.Send(new RoomPropertyMessageEvent("floor", room.Floor).Compose());
            session.Send(new RoomPropertyMessageEvent("wallpaper", room.Wallpaper).Compose());
            if (!string.IsNullOrEmpty(room.Landscape))
            {
                session.Send(new RoomPropertyMessageEvent("landscape", room.Landscape).Compose());
            }
        }

        session.Send(new FurnitureAliasesMessageEvent(furnitureAliasCache.Aliases).Compose());

        session.Send(new PublicRoomObjectsMessageEvent(room.VisibleStaticFurniture).Compose());

        if (!isPublic)
        {
            IReadOnlyList<UserItem> floorItems = await userItemRepository.GetFloorItemsInRoomAsync(roomId, cancellationToken);
            IReadOnlyList<UserItem> wallItems = await userItemRepository.GetWallItemsInRoomAsync(roomId, cancellationToken);
            session.Send(new ObjectsMessageEvent(floorItems).Compose());
            session.Send(new ItemsMessageEvent(wallItems).Compose());
        }

        session.Send(new UsersMessageEvent(room.Actors.ToList()).Compose());

        session.Send((isPublic
            ? RoomEntryInfoMessageEvent.ForPublicRoom(publicRooms.NavCodeOf(room.Data.HeightmapLabel), publicEntry?.NavIndex ?? 0)
            : RoomEntryInfoMessageEvent.ForPrivateRoom(roomId, isOwner)).Compose());

        session.Send(new UserUpdateMessageEvent(room.Actors.ToList()).Compose());

        foreach (RoomActor other in room.Actors)
        {
            int danceId;
            int carryItemId;
            int effectId;
            bool asleep;
            lock (other.Lock)
            {
                (danceId, carryItemId, effectId, asleep) = (other.DanceId, other.CarryItemId, other.EffectId, other.AsleepSince is not null);
            }

            if (asleep)
            {
                session.Send(new SleepMessageEvent(other.VirtualId, asleep: true).Compose());
            }

            if (danceId > 0)
            {
                session.Send(new DanceMessageEvent(other.VirtualId, danceId).Compose());
            }

            if (carryItemId > 0)
            {
                session.Send(new CarryObjectMessageEvent(other.VirtualId, carryItemId).Compose());
            }

            if (effectId > 0)
            {
                session.Send(new AvatarEffectMessageEvent(other.VirtualId, effectId).Compose());
            }
        }

        if (room.Data.HeightmapLabel == InfobusHeightmapLabel)
        {
            session.Send(new ParkBusDoorMessageEvent(infobusOpen).Compose());
        }

        if (actor.FavouriteGroup is { } group)
        {
            roomBroadcaster.Broadcast(room, new HabboGroupBadgesMessageEvent([group]).Compose(), excludeSessionId: session.Id);
        }

        roomBroadcaster.Broadcast(room, new UsersMessageEvent([actor]).Compose(), excludeSessionId: session.Id);
        roomBroadcaster.Broadcast(room, new UserUpdateMessageEvent([actor]).Compose(), excludeSessionId: session.Id);
        if (actor.EffectId > 0)
        {
            roomBroadcaster.Broadcast(room, new AvatarEffectMessageEvent(actor.VirtualId, actor.EffectId).Compose(), excludeSessionId: session.Id);
        }

        if (arrivalTeleporter is not null)
        {
            teleportService.BeginArrival(room, actor, arrivalTeleporter);
        }
    }
}
