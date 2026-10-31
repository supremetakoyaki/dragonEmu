using DragonEmu.Game.Rooms;

namespace DragonEmu.Protocol.Messages.Incoming.Idol;

public sealed class StartPerformanceMessageComposer : IMessageComposer
{
    private readonly IdolService _idol;

    private int _songId;

    public StartPerformanceMessageComposer(IdolService idol)
    {
        _idol = idol;
    }

    public void Parse(ClientPacket packet)
    {
        _songId = packet.PopInt32();
    }

    public async ValueTask HandleAsync(IGameSession session, CancellationToken cancellationToken)
    {
        if (session.AuthenticatedUser is { } account)
        {
            await _idol.StartPerformanceAsync(session, account, _songId, cancellationToken);
        }
    }
}
