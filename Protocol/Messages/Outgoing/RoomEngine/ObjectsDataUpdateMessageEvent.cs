namespace DragonEmu.Protocol.Messages.Outgoing.RoomEngine;

public sealed class ObjectsDataUpdateMessageEvent : IMessageEvent
{
    private readonly IReadOnlyList<(int Id, int State, string Data)> _objects;

    public ObjectsDataUpdateMessageEvent(IReadOnlyList<(int Id, int State, string Data)> objects)
    {
        _objects = objects;
    }

    public ServerPacket Compose()
    {
        var packet = new ServerPacket(OutgoingOpcodes.RoomEngine.ObjectsDataUpdateMessageEvent);
        packet.AppendInt32(_objects.Count);
        foreach ((int id, int state, string data) in _objects)
        {
            packet.AppendInt32(id);
            packet.AppendInt32(state);
            packet.AppendString(data);
        }

        return packet;
    }
}
