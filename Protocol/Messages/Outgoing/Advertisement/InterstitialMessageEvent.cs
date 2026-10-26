namespace DragonEmu.Protocol.Messages.Outgoing.Advertisement;

public sealed class InterstitialMessageEvent : IMessageEvent
{
    private readonly string _imageUrl;
    private readonly string _link;

    public InterstitialMessageEvent(string imageUrl, string link)
    {
        _imageUrl = imageUrl;
        _link = link;
    }

    public ServerPacket Compose()
    {
        var packet = new ServerPacket(OutgoingOpcodes.Advertisement.InterstitialMessageEvent);
        packet.AppendString(_imageUrl);
        packet.AppendString(_link);
        return packet;
    }
}
