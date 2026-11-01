using DragonEmu.Game.Furniture;

namespace DragonEmu.Protocol.Messages.Incoming.Trax;

public sealed class AddJukeboxDiscMessageComposer : IMessageComposer
{
    private readonly JukeboxService _jukebox;

    private int _diskItemId;
    private int _slot;

    public AddJukeboxDiscMessageComposer(JukeboxService jukebox)
    {
        _jukebox = jukebox;
    }

    public void Parse(ClientPacket packet)
    {
        _diskItemId = packet.PopInt32();
        _slot = packet.PopInt32();
    }

    public async ValueTask HandleAsync(IGameSession session, CancellationToken cancellationToken) =>
        await _jukebox.LoadDiskAsync(session, _diskItemId, _slot, cancellationToken);
}
