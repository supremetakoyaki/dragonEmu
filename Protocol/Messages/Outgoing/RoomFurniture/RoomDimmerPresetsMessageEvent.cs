using DragonEmu.Domain;

namespace DragonEmu.Protocol.Messages.Outgoing.RoomFurniture;

public sealed class RoomDimmerPresetsMessageEvent : IMessageEvent
{
    private readonly int _selectedPresetId;
    private readonly IReadOnlyList<DimmerPreset> _presets;

    public RoomDimmerPresetsMessageEvent(int selectedPresetId, IReadOnlyList<DimmerPreset> presets)
    {
        _selectedPresetId = selectedPresetId;
        _presets = presets;
    }

    public ServerPacket Compose()
    {
        var packet = new ServerPacket(OutgoingOpcodes.RoomFurniture.RoomDimmerPresetsMessageEvent);
        packet.AppendInt32(_presets.Count);
        packet.AppendInt32(_selectedPresetId);

        foreach (DimmerPreset preset in _presets)
        {
            packet.AppendInt32(preset.Id);
            packet.AppendInt32(preset.Type);
            packet.AppendString(preset.Color);
            packet.AppendInt32(preset.Intensity);
        }

        return packet;
    }
}
