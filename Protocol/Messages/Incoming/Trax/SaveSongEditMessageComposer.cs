using DragonEmu.Game.Furniture;

namespace DragonEmu.Protocol.Messages.Incoming.Trax;

public sealed class SaveSongEditMessageComposer : IMessageComposer
{
    private readonly TraxEditorService _editor;

    private int _songId;
    private string _name = "";
    private string _data = "";

    public SaveSongEditMessageComposer(TraxEditorService editor)
    {
        _editor = editor;
    }

    public void Parse(ClientPacket packet)
    {
        _songId = packet.PopInt32();
        _name = packet.PopString();
        _data = packet.PopString();
    }

    public async ValueTask HandleAsync(IGameSession session, CancellationToken cancellationToken) =>
        await _editor.SaveEditAsync(session, _songId, _name, _data, cancellationToken);
}
