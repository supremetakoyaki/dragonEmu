using DragonEmu.Game.Furniture;

namespace DragonEmu.Protocol.Messages.Incoming.Trax;

public sealed class RemoveJukeboxDiscMessageComposer : IMessageComposer
{
    private readonly JukeboxService _jukebox;

    private int _slot;

    public RemoveJukeboxDiscMessageComposer(JukeboxService jukebox)
    {
        _jukebox = jukebox;
    }

    public void Parse(ClientPacket packet)
    {
        _slot = packet.PopInt32();
    }

    public async ValueTask HandleAsync(IGameSession session, CancellationToken cancellationToken) =>
        await _jukebox.EjectDiskAsync(session, _slot, cancellationToken);
}
