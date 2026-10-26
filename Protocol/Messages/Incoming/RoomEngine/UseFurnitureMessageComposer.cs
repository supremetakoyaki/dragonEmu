using DragonEmu.Domain;
using DragonEmu.Game.Furniture;
using DragonEmu.Game.Pets;
using DragonEmu.Game.Rooms;

namespace DragonEmu.Protocol.Messages.Incoming.RoomEngine;

public sealed class UseFurnitureMessageComposer : IMessageComposer
{
    private readonly IRoomManager _roomManager;
    private readonly RoomTeleportService _teleports;
    private readonly FurnitureStateService _furnitureState;
    private readonly RoomRightsService _roomRights;
    private readonly VendingService _vending;
    private readonly TotemService _totems;
    private readonly GateService _gates;
    private readonly HockeyLightService _hockeyLights;
    private readonly PetService _pets;
    private readonly TraxService _trax;

    private int _itemId;

    public UseFurnitureMessageComposer(
        IRoomManager roomManager, RoomTeleportService teleports, FurnitureStateService furnitureState, RoomRightsService roomRights,
        VendingService vending, TotemService totems, GateService gates, HockeyLightService hockeyLights, PetService pets, TraxService trax)
    {
        _trax = trax;
        _pets = pets;
        _hockeyLights = hockeyLights;
        _vending = vending;
        _totems = totems;
        _gates = gates;
        _roomManager = roomManager;
        _teleports = teleports;
        _furnitureState = furnitureState;
        _roomRights = roomRights;
    }

    public void Parse(ClientPacket packet)
    {
        _itemId = packet.PopInt32();
        packet.PopInt32();
    }

    public async ValueTask HandleAsync(IGameSession session, CancellationToken cancellationToken)
    {
        if (session.AuthenticatedUser is not { } account || session.CurrentRoomId is not { } roomId)
        {
            return;
        }

        if (!_roomManager.TryGetLoadedRoom(roomId, out Room? room) || !room!.TryGetActor(session.Id, out RoomActor? actor))
        {
            return;
        }

        RoomFloorItem? item = room.GetFloorItem(_itemId);
        if (item is null)
        {
            return;
        }

        switch (item.InteractionLogic)
        {
            case FurnitureInteractionLogic.Teleport:
                await _teleports.TryUseAsync(room, actor!, item, cancellationToken);
                break;

            case FurnitureInteractionLogic.Vending:
                _vending.Use(room, actor!, item);
                break;

            case FurnitureInteractionLogic.Toggle when Room.IsSoundFurni(item.LogicClass):
                if (await _roomRights.CanToggleFurnitureAsync(account, room.Data, cancellationToken))
                {
                    await _trax.ToggleAsync(room, item, cancellationToken);
                }

                break;

            case FurnitureInteractionLogic.Toggle when item.LogicClass == FurnitureLogicClass.Multistate:
                if (await _roomRights.CanToggleFurnitureAsync(account, room.Data, cancellationToken))
                {
                    await _furnitureState.ToggleNextStateAsync(room, item, cancellationToken);
                }

                break;

            case FurnitureInteractionLogic.Gate:
                if (await _roomRights.CanToggleFurnitureAsync(account, room.Data, cancellationToken))
                {
                    await _gates.ToggleAsync(room, item, cancellationToken);
                }

                break;

            case FurnitureInteractionLogic.HockeyLight:
                if (await _roomRights.CanToggleFurnitureAsync(account, room.Data, cancellationToken))
                {
                    _hockeyLights.Use(room, actor!, item);
                }

                break;

            case FurnitureInteractionLogic.PetWater:
                if (await _roomRights.CanToggleFurnitureAsync(account, room.Data, cancellationToken))
                {
                    await _pets.RefillWaterAsync(room, item, cancellationToken);
                }

                break;

            case FurnitureInteractionLogic.PetNest when item.LogicClass == FurnitureLogicClass.Multistate:
                if (await _roomRights.CanToggleFurnitureAsync(account, room.Data, cancellationToken))
                {
                    await _furnitureState.ToggleNextStateAsync(room, item, cancellationToken);
                }

                break;

            case FurnitureInteractionLogic.TotemLeg or FurnitureInteractionLogic.TotemHead or FurnitureInteractionLogic.TotemPlanet:
                bool mayChange = await _roomRights.CanToggleFurnitureAsync(account, room.Data, cancellationToken);
                await _totems.UseAsync(session, account.Id, room, item, mayChange, cancellationToken);
                break;
        }
    }
}
