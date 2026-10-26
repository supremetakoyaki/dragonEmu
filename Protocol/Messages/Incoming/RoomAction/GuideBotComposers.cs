using DragonEmu.Game.Rooms;

namespace DragonEmu.Protocol.Messages.Incoming.RoomAction;

public sealed class CallGuideBotMessageComposer : IMessageComposer
{
    private readonly GuideBotService _guides;

    public CallGuideBotMessageComposer(GuideBotService guides)
    {
        _guides = guides;
    }

    public void Parse(ClientPacket packet)
    {
    }

    public ValueTask HandleAsync(IGameSession session, CancellationToken cancellationToken)
    {
        if (session.AuthenticatedUser is { } account)
        {
            _guides.Call(session, account);
        }

        return ValueTask.CompletedTask;
    }
}

public sealed class KickBotMessageComposer : IMessageComposer
{
    private readonly GuideBotService _guides;

    private int _virtualId;

    public KickBotMessageComposer(GuideBotService guides)
    {
        _guides = guides;
    }

    public void Parse(ClientPacket packet)
    {
        _virtualId = packet.PopInt32();
    }

    public async ValueTask HandleAsync(IGameSession session, CancellationToken cancellationToken)
    {
        if (session.AuthenticatedUser is { } account)
        {
            await _guides.KickAsync(session, account, _virtualId, cancellationToken);
        }
    }
}
