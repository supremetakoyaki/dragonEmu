using DragonEmu.Game.Furniture;

namespace DragonEmu.Protocol.Messages.Incoming.Trax;

public sealed class SetSoundMachineStateMessageComposer : IMessageComposer
{
    private readonly TraxEditorService _editor;

    private int _itemId;
    private bool _on;

    public SetSoundMachineStateMessageComposer(TraxEditorService editor)
    {
        _editor = editor;
    }

    public void Parse(ClientPacket packet)
    {
        _itemId = packet.PopInt32();
        _on = packet.PopBoolean();
    }

    public async ValueTask HandleAsync(IGameSession session, CancellationToken cancellationToken) =>
        await _editor.SetStateAsync(session, _itemId, _on, cancellationToken);
}
