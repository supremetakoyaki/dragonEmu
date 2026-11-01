using DragonEmu.Game.Furniture;

namespace DragonEmu.Protocol.Messages.Incoming.Trax;

public sealed class BurnSongMessageComposer : IMessageComposer
{
    private readonly TraxEditorService _editor;

    private int _songId;

    public BurnSongMessageComposer(TraxEditorService editor)
    {
        _editor = editor;
    }

    public void Parse(ClientPacket packet)
    {
        _songId = packet.PopInt32();
    }

    public async ValueTask HandleAsync(IGameSession session, CancellationToken cancellationToken) =>
        await _editor.BurnAsync(session, _songId, cancellationToken);
}
