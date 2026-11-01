using DragonEmu.Game.Furniture;

namespace DragonEmu.Protocol.Messages.Incoming.Trax;

public sealed class EjectSoundPackageMessageComposer : IMessageComposer
{
    private readonly TraxEditorService _editor;

    private int _slot;

    public EjectSoundPackageMessageComposer(TraxEditorService editor)
    {
        _editor = editor;
    }

    public void Parse(ClientPacket packet)
    {
        _slot = packet.PopInt32();
    }

    public async ValueTask HandleAsync(IGameSession session, CancellationToken cancellationToken) =>
        await _editor.EjectSoundSetAsync(session, _slot, cancellationToken);
}
