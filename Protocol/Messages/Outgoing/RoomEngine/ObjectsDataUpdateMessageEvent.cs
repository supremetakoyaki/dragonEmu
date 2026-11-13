namespace DragonEmu.Protocol.Messages.Outgoing.RoomEngine;

public sealed class ObjectsDataUpdateMessageEvent : IMessageEvent
{
    private readonly IReadOnlyList<(int Id, string Data)> _objects;

    public ObjectsDataUpdateMessageEvent(IReadOnlyList<(int Id, string Data)> objects)
    {
        _objects = objects;
    }

    public static IMessageEvent? ForChanges(IReadOnlyList<(int Id, string Data)> changes) => changes.Count switch
    {
        0 => null,
        1 => new ObjectDataUpdateMessageEvent(changes[0].Id, changes[0].Data),
        _ => new ObjectsDataUpdateMessageEvent(changes),
    };

    public ServerPacket Compose()
    {
        var packet = new ServerPacket(OutgoingOpcodes.RoomEngine.ObjectsDataUpdateMessageEvent);
        packet.AppendInt32(_objects.Count);
        foreach ((int id, string data) in _objects)
        {
            packet.AppendInt32(id);
            packet.AppendString(data);
        }

        return packet;
    }
}
