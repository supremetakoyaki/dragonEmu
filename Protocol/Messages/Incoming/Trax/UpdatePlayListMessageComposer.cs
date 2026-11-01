using DragonEmu.Game.Furniture;

namespace DragonEmu.Protocol.Messages.Incoming.Trax;

public sealed class UpdatePlayListMessageComposer : IMessageComposer
{
    private readonly TraxEditorService _editor;

    private IReadOnlyList<int> _songIds = [];

    public UpdatePlayListMessageComposer(TraxEditorService editor)
    {
        _editor = editor;
    }

    public void Parse(ClientPacket packet)
    {
        _songIds = packet.PopInt32List(TraxEditorService.PlaylistLimit + 1);
    }

    public async ValueTask HandleAsync(IGameSession session, CancellationToken cancellationToken) =>
        await _editor.SavePlaylistAsync(session, _songIds, cancellationToken);
}
