namespace DragonEmu.Protocol.Messages.Outgoing.Camera;

public sealed class PhotoViewMessageEvent : IMessageEvent
{
    private readonly int _itemId;
    private readonly string _path;
    private readonly string _time;
    private readonly string _caption;

    public PhotoViewMessageEvent(int itemId, string path, string time, string caption)
    {
        _itemId = itemId;
        _path = path;
        _time = time;
        _caption = caption;
    }

    public ServerPacket Compose()
    {
        var packet = new ServerPacket(OutgoingOpcodes.Camera.PhotoViewMessageEvent);
        packet.AppendInt32(_itemId);
        packet.AppendString(_path);
        packet.AppendString(_time);
        packet.AppendString(_caption);
        return packet;
    }
}
