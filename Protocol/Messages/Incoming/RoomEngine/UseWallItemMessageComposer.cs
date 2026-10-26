using DragonEmu.Domain;
using DragonEmu.Game.Furniture;
using DragonEmu.Game.Rooms;
using DragonEmu.Persistence;

namespace DragonEmu.Protocol.Messages.Incoming.RoomEngine;

public sealed class UseWallItemMessageComposer : IMessageComposer
{
    private readonly IRoomManager _roomManager;
    private readonly FurnitureDefinitionCache _furnitureDefinitions;
    private readonly FurnitureStateService _furnitureState;
    private readonly RoomRightsService _roomRights;

    private int _itemId;

    public UseWallItemMessageComposer(
        IRoomManager roomManager, FurnitureDefinitionCache furnitureDefinitions, FurnitureStateService furnitureState,
        RoomRightsService roomRights)
    {
        _roomManager = roomManager;
        _furnitureDefinitions = furnitureDefinitions;
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
        if (session.AuthenticatedUser is not { } account
            || session.CurrentRoomId is not { } roomId
            || !_roomManager.TryGetLoadedRoom(roomId, out Room? room) || room is null
            || room.GetWallItem(_itemId) is not { } item)
        {
            return;
        }

        FurnitureDefinition? definition = _furnitureDefinitions.GetById(item.FurniDefId);
        if (definition is not { InteractionLogic: FurnitureInteractionLogic.Toggle, LogicClass: FurnitureLogicClass.Multistate }
            || !await _roomRights.CanToggleFurnitureAsync(account, room.Data, cancellationToken))
        {
            return;
        }

        await _furnitureState.ToggleNextWallStateAsync(room, _itemId, definition.States, cancellationToken);
    }
}
