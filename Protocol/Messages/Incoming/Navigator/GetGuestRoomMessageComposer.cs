using DragonEmu.Domain;
using DragonEmu.Game.Rooms;
using DragonEmu.Persistence;
using DragonEmu.Protocol.Messages.Outgoing.Navigator;

namespace DragonEmu.Protocol.Messages.Incoming.Navigator;

public sealed class GetGuestRoomMessageComposer : IMessageComposer
{
    private readonly IPrivateRoomRepository _privateRoomRepository;
    private readonly IRoomManager _roomManager;

    private int _roomId;
    private bool _enterRoom;
    private bool _roomForward;

    public GetGuestRoomMessageComposer(IPrivateRoomRepository privateRoomRepository, IRoomManager roomManager)
    {
        _privateRoomRepository = privateRoomRepository;
        _roomManager = roomManager;
    }

    public void Parse(ClientPacket packet)
    {
        _roomId = packet.PopInt32();
        _enterRoom = packet.PopInt32() == 1;
        _roomForward = packet.PopInt32() == 1;
    }

    public async ValueTask HandleAsync(IGameSession session, CancellationToken cancellationToken)
    {
        if (session.AuthenticatedUser is null)
        {
            return;
        }

        PrivateRoomSummary? room = await _privateRoomRepository.GetByIdAsync(_roomId, cancellationToken);
        if (room is null)
        {
            return;
        }

        bool teleportingIn = _roomForward && session.PendingTeleport is { } arrival && arrival.IsFor(_roomId, DateTime.UtcNow);

        session.Send(new GetGuestRoomResultEvent(
            _enterRoom, teleportingIn ? room with { AccessType = 0 } : room, _roomManager.GetLiveUserCount(_roomId), _roomForward).Compose());
    }
}
