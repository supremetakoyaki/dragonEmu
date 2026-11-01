using DragonEmu.Game.Furniture;

namespace DragonEmu.Protocol.Messages.Incoming.Trax;

public sealed class GetUserSongDiscsMessageComposer : IMessageComposer
{
    private readonly JukeboxService _jukebox;

    public GetUserSongDiscsMessageComposer(JukeboxService jukebox)
    {
        _jukebox = jukebox;
    }

    public void Parse(ClientPacket packet)
    {
    }

    public async ValueTask HandleAsync(IGameSession session, CancellationToken cancellationToken) =>
        await _jukebox.SendUserDisksAsync(session, cancellationToken);
}
