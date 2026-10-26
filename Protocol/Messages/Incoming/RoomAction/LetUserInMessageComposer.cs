using DragonEmu.Game.Rooms;
using DragonEmu.Gateway;
using DragonEmu.Protocol.Messages.Outgoing.Navigator;
using DragonEmu.Protocol.Messages.Outgoing.RoomSession;

namespace DragonEmu.Protocol.Messages.Incoming.RoomAction;

public sealed class LetUserInMessageComposer : IMessageComposer
{
    private readonly IRoomManager _roomManager;
    private readonly RoomRightsService _roomRights;
    private readonly RoomBroadcaster _roomBroadcaster;
    private readonly ClientSessionManager _sessionManager;

    private string _userName = string.Empty;
    private bool _letIn;

    public LetUserInMessageComposer(
        IRoomManager roomManager, RoomRightsService roomRights, RoomBroadcaster roomBroadcaster, ClientSessionManager sessionManager)
    {
        _roomManager = roomManager;
        _roomRights = roomRights;
        _roomBroadcaster = roomBroadcaster;
        _sessionManager = sessionManager;
    }

    public void Parse(ClientPacket packet)
    {
        _userName = packet.PopString();
        _letIn = packet.PopBoolean();
    }

    public async ValueTask HandleAsync(IGameSession session, CancellationToken cancellationToken)
    {
        if (session.AuthenticatedUser is not { } account || session.CurrentRoomId is not { } roomId
            || !_roomManager.TryGetLoadedRoom(roomId, out Room? room)
            || !await _roomRights.CanUseFurnitureAsync(account, room!.Data, cancellationToken))
        {
            return;
        }

        ClientSession? visitor = _sessionManager.GetAuthenticatedSessions()
            .FirstOrDefault(candidate => candidate.RingingAt == roomId && candidate.AuthenticatedUser!.Username == _userName);
        if (visitor is null)
        {
            return;
        }

        visitor.RingingAt = null;
        if (_letIn)
        {
            visitor.LetInto = roomId;
            visitor.EnqueueSend(new FlatAccessibleMessageEvent().Compose());
            _roomBroadcaster.SendToRightsHolders(room, new FlatAccessibleMessageEvent(_userName).Compose());
        }
        else
        {
            visitor.EnqueueSend(new FlatAccessDeniedMessageEvent().Compose());
            _roomBroadcaster.SendToRightsHolders(room, new FlatAccessDeniedMessageEvent(_userName).Compose());
        }
    }
}
