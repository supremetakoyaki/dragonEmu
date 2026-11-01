using DragonEmu.Game.Furniture;

namespace DragonEmu.Protocol.Messages.Incoming.Trax;

public sealed class GetSongListMessageComposer : IMessageComposer
{
    private readonly TraxEditorService _editor;

    public GetSongListMessageComposer(TraxEditorService editor)
    {
        _editor = editor;
    }

    public void Parse(ClientPacket packet)
    {
    }

    public async ValueTask HandleAsync(IGameSession session, CancellationToken cancellationToken) =>
        await _editor.SendSongListAsync(session, cancellationToken);
}
