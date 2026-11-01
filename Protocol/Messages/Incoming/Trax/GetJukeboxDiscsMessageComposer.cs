using DragonEmu.Game.Furniture;

namespace DragonEmu.Protocol.Messages.Incoming.Trax;

public sealed class GetJukeboxDiscsMessageComposer : IMessageComposer
{
    private readonly JukeboxService _jukebox;

    public GetJukeboxDiscsMessageComposer(JukeboxService jukebox)
    {
        _jukebox = jukebox;
    }

    public void Parse(ClientPacket packet)
    {
    }

    public async ValueTask HandleAsync(IGameSession session, CancellationToken cancellationToken) =>
        await _jukebox.SendDisksAsync(session, cancellationToken);
}
