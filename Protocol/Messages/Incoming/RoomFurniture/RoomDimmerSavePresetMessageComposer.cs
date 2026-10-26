using DragonEmu.Game.Furniture;

namespace DragonEmu.Protocol.Messages.Incoming.RoomFurniture;

public sealed class RoomDimmerSavePresetMessageComposer : IMessageComposer
{
    private readonly RoomDimmerService _dimmers;

    private int _presetId;
    private int _type;
    private string _color = string.Empty;
    private int _intensity;
    private bool _apply;

    public RoomDimmerSavePresetMessageComposer(RoomDimmerService dimmers)
    {
        _dimmers = dimmers;
    }

    public void Parse(ClientPacket packet)
    {
        _presetId = packet.PopInt32();
        _type = packet.PopInt32();
        _color = packet.PopString();
        _intensity = packet.PopInt32();
        _apply = packet.PopInt32() != 0;
    }

    public async ValueTask HandleAsync(IGameSession session, CancellationToken cancellationToken) =>
        await _dimmers.SavePresetAsync(session, _presetId, _type, _color, _intensity, _apply, cancellationToken);
}
