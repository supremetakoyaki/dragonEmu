using DragonEmu.Game.Moderation;

namespace DragonEmu.Protocol.Messages.Incoming.Moderator;

public sealed class ModerateRoomMessageComposer : IMessageComposer
{
    private readonly ModerationToolService _tool;

    private int _roomId;
    private bool _lockDoor;
    private bool _rename;
    private bool _kickEveryone;

    public ModerateRoomMessageComposer(ModerationToolService tool)
    {
        _tool = tool;
    }

    public void Parse(ClientPacket packet)
    {
        _roomId = packet.PopInt32();
        _lockDoor = packet.PopInt32() == 1;
        _rename = packet.PopInt32() == 1;
        _kickEveryone = packet.PopInt32() == 1;
    }

    public async ValueTask HandleAsync(IGameSession session, CancellationToken cancellationToken)
    {
        if (session.IsModerator && session.AuthenticatedUser is { } account)
        {
            await _tool.ModerateRoomAsync(account, _roomId, _lockDoor, _rename, _kickEveryone, cancellationToken);
        }
    }
}
