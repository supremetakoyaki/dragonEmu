using DragonEmu.Game.Moderation;

namespace DragonEmu.Protocol.Messages.Incoming.Moderator;

public sealed class GetModeratorRoomInfoMessageComposer : IMessageComposer
{
    private readonly ModerationToolService _tool;

    private int _roomId;

    public GetModeratorRoomInfoMessageComposer(ModerationToolService tool)
    {
        _tool = tool;
    }

    public void Parse(ClientPacket packet)
    {
        _roomId = packet.PopInt32();
    }

    public async ValueTask HandleAsync(IGameSession session, CancellationToken cancellationToken)
    {
        if (session.IsModerator && session.AuthenticatedUser is { } account)
        {
            await _tool.SendRoomInfoAsync(session, account, _roomId, cancellationToken);
        }
    }
}
