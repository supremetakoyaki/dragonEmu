namespace DragonEmu.Protocol.Messages.Outgoing.Camera;

public sealed class FilmMessageEvent : IMessageEvent
{
    private readonly int _film;
    private readonly string _token;

    public FilmMessageEvent(int film, string token)
    {
        _film = film;
        _token = token;
    }

    public ServerPacket Compose()
    {
        var packet = new ServerPacket(OutgoingOpcodes.Camera.FilmMessageEvent);
        packet.AppendInt32(_film);
        packet.AppendString(_token);
        return packet;
    }
}
