using DragonEmu.Game.Furniture;

namespace DragonEmu.Protocol.Messages.Incoming.Trax;

public sealed class EditSongMessageComposer : IMessageComposer
{
    private readonly TraxEditorService _editor;

    private int _songId;

    public EditSongMessageComposer(TraxEditorService editor)
    {
        _editor = editor;
    }

    public void Parse(ClientPacket packet)
    {
        _songId = packet.PopInt32();
    }

    public async ValueTask HandleAsync(IGameSession session, CancellationToken cancellationToken) =>
        await _editor.EditSongAsync(session, _songId, cancellationToken);
}
