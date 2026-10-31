using DragonEmu.Game.Rooms;

namespace DragonEmu.Protocol.Messages.Incoming.Idol;

public sealed class VotePerformanceMessageComposer : IMessageComposer
{
    private readonly IdolService _idol;

    private bool _yes;

    public VotePerformanceMessageComposer(IdolService idol)
    {
        _idol = idol;
    }

    public void Parse(ClientPacket packet)
    {
        _yes = packet.PopInt32() != 0;
    }

    public ValueTask HandleAsync(IGameSession session, CancellationToken cancellationToken)
    {
        _idol.Vote(session, _yes);
        return ValueTask.CompletedTask;
    }
}
