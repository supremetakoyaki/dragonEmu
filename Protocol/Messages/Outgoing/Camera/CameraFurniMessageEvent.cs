namespace DragonEmu.Protocol.Messages.Outgoing.Camera;

public sealed class CameraFurniMessageEvent : IMessageEvent
{
    private readonly IReadOnlyList<int> _types;

    public CameraFurniMessageEvent(IReadOnlyList<int> types)
    {
        _types = types;
    }

    public ServerPacket Compose()
    {
        var packet = new ServerPacket(OutgoingOpcodes.Camera.CameraFurniMessageEvent);
        packet.AppendInt32(_types.Count);
        foreach (int type in _types)
        {
            packet.AppendInt32(type);
        }

        return packet;
    }
}
