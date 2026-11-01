using DragonEmu.Game.Furniture;

namespace DragonEmu.Protocol.Messages.Incoming.Trax;

public sealed class SaveSongNewMessageComposer : IMessageComposer
{
    private readonly TraxEditorService _editor;

    private string _name = "";
    private string _data = "";

    public SaveSongNewMessageComposer(TraxEditorService editor)
    {
        _editor = editor;
    }

    public void Parse(ClientPacket packet)
    {
        _name = packet.PopString();
        _data = packet.PopString();
    }

    public async ValueTask HandleAsync(IGameSession session, CancellationToken cancellationToken) =>
        await _editor.SaveNewAsync(session, _name, _data, cancellationToken);
}
