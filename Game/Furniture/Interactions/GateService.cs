using System.Globalization;
using DragonEmu.Game.Rooms;
using DragonEmu.Persistence;
using DragonEmu.Protocol.Messages.Outgoing.RoomEngine;

namespace DragonEmu.Game.Furniture;

public sealed class GateService
{
    public const string ClosedState = "0";

    private readonly IUserItemRepository _userItemRepository;
    private readonly RoomBroadcaster _roomBroadcaster;
    private readonly PerRoomLock _roomLock = new();

    public GateService(IUserItemRepository userItemRepository, RoomBroadcaster roomBroadcaster)
    {
        _userItemRepository = userItemRepository;
        _roomBroadcaster = roomBroadcaster;
    }

    public Task ToggleAsync(Room room, RoomFloorItem gate, CancellationToken cancellationToken) =>
        _roomLock.RunAsync(room, async () =>
        {
            if (room.TryToggleGate(gate.Id) is not { } open)
            {
                return;
            }

            string state = (open ? 1 : 0).ToString(CultureInfo.InvariantCulture);
            await _userItemRepository.UpdateExtraDataAsync(gate.Id, state, cancellationToken);
            _roomBroadcaster.Broadcast(room, new ObjectDataUpdateMessageEvent(gate.Id, state).Compose());
        }, cancellationToken);
}
