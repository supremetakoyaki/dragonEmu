using DragonEmu.Game.Furniture;

namespace DragonEmu.Protocol.Messages.Incoming.Trax;

public sealed class ResetJukeboxMessageComposer : IMessageComposer
{
    private readonly JukeboxService _jukebox;

    public ResetJukeboxMessageComposer(JukeboxService jukebox)
    {
        _jukebox = jukebox;
    }

    public void Parse(ClientPacket packet)
    {
    }

    public async ValueTask HandleAsync(IGameSession session, CancellationToken cancellationToken) =>
        await _jukebox.ResetAsync(session, cancellationToken);
}
