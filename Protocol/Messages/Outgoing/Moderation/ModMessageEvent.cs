namespace DragonEmu.Protocol.Messages.Outgoing.Moderation;

public sealed class ModMessageEvent : IMessageEvent
{
    public const string LineBreak = "\\r";

    private readonly string _message;
    private readonly string _url;

    public ModMessageEvent(string message, string url = "")
    {
        _message = message;
        _url = url;
    }

    public static ModMessageEvent FromLines(IEnumerable<string> lines) => new(string.Join(LineBreak, lines));

    public ServerPacket Compose()
    {
        var packet = new ServerPacket(OutgoingOpcodes.Moderation.ModMessageEvent);
        packet.AppendString(_message);
        packet.AppendString(_url);
        return packet;
    }
}
