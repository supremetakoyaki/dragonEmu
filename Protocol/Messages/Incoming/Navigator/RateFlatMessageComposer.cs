using DragonEmu.Game.Rooms;
using DragonEmu.Persistence;
using DragonEmu.Protocol.Messages.Outgoing.Navigator;

namespace DragonEmu.Protocol.Messages.Incoming.Navigator;

public sealed class RateFlatMessageComposer : IMessageComposer
{
    private readonly IRoomManager _roomManager;
    private readonly IRoomVoteRepository _roomVoteRepository;

    private int _vote;

    public RateFlatMessageComposer(IRoomManager roomManager, IRoomVoteRepository roomVoteRepository)
    {
        _roomManager = roomManager;
        _roomVoteRepository = roomVoteRepository;
    }

    public void Parse(ClientPacket packet)
    {
        _vote = packet.PopInt32();
    }

    public async ValueTask HandleAsync(IGameSession session, CancellationToken cancellationToken)
    {
        if (session.AuthenticatedUser is not { } account || session.CurrentRoomId is not { } roomId)
        {
            return;
        }

        if (_vote is not (1 or -1)
            || !_roomManager.TryGetLoadedRoom(roomId, out Room? room)
            || RoomRightsService.IsOwner(account, room!.Data))
        {
            return;
        }

        int? score = await _roomVoteRepository.VoteAsync(account.Id, roomId, _vote, cancellationToken);
        if (score is null)
        {
            return;
        }

        session.Send(new RoomRatingEvent(score.Value).Compose());
    }
}
