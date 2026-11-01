using DragonEmu.Game.Furniture;

namespace DragonEmu.Protocol.Messages.Incoming.Trax;

public sealed class JukeboxPlaylistAddMessageComposer : IMessageComposer
{
    private readonly JukeboxService _jukebox;

    private int _songId;

    public JukeboxPlaylistAddMessageComposer(JukeboxService jukebox)
    {
        _jukebox = jukebox;
    }

    public void Parse(ClientPacket packet)
    {
        _songId = packet.PopInt32();
    }

    public async ValueTask HandleAsync(IGameSession session, CancellationToken cancellationToken) =>
        await _jukebox.QueueAsync(session, _songId, cancellationToken);
}
