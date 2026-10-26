using DragonEmu.Game.Moderation;

namespace DragonEmu.Protocol.Messages.Incoming.Moderator;

public sealed class ModeratorActionMessageComposer : IMessageComposer
{
    private readonly ModerationToolService _tool;

    private int _type;
    private string _message = string.Empty;

    public ModeratorActionMessageComposer(ModerationToolService tool)
    {
        _tool = tool;
    }

    public void Parse(ClientPacket packet)
    {
        packet.PopInt32();
        _type = packet.PopInt32();
        _message = packet.PopString();
    }

    public async ValueTask HandleAsync(IGameSession session, CancellationToken cancellationToken)
    {
        if (session.IsModerator && session.AuthenticatedUser is { } account)
        {
            await _tool.AlertRoomAsync(session, account, _type, _message, cancellationToken);
        }
    }
}
