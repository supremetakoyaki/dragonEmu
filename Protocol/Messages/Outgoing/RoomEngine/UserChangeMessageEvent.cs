namespace DragonEmu.Protocol.Messages.Outgoing.RoomEngine;

public sealed class UserChangeMessageEvent : IMessageEvent
{
    private readonly int _virtualId;
    private readonly string _figure;
    private readonly char _gender;
    private readonly string _motto;
    private readonly int _achievementScore;

    public UserChangeMessageEvent(int virtualId, string figure, char gender, string motto, int achievementScore = 0)
    {
        _achievementScore = achievementScore;
        _virtualId = virtualId;
        _figure = figure;
        _gender = gender;
        _motto = motto;
    }

    public ServerPacket Compose()
    {
        var packet = new ServerPacket(OutgoingOpcodes.RoomEngine.UserChangeMessageEvent);
        packet.AppendInt32(_virtualId);
        packet.AppendString(_figure);
        packet.AppendString(char.ToLowerInvariant(_gender).ToString());
        packet.AppendString(_motto);
        packet.AppendInt32(_achievementScore);
        return packet;
    }
}
