using DragonEmu.Game.Furniture;

namespace DragonEmu.Protocol.Messages.Incoming.Trax;

public sealed class InsertSoundPackageMessageComposer : IMessageComposer
{
    private readonly TraxEditorService _editor;

    private int _soundSetId;
    private int _slot;

    public InsertSoundPackageMessageComposer(TraxEditorService editor)
    {
        _editor = editor;
    }

    public void Parse(ClientPacket packet)
    {
        _soundSetId = packet.PopInt32();
        _slot = packet.PopInt32();
    }

    public async ValueTask HandleAsync(IGameSession session, CancellationToken cancellationToken) =>
        await _editor.InsertSoundSetAsync(session, _soundSetId, _slot, cancellationToken);
}
