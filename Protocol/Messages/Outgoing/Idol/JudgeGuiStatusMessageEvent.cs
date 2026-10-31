namespace DragonEmu.Protocol.Messages.Outgoing.Idol;

public enum JudgeGuiStatus
{
    Closed = 0,

    Waiting = 1,

    Voting = 2,
}

public sealed class JudgeGuiStatusMessageEvent : IMessageEvent
{
    private readonly JudgeGuiStatus _status;
    private readonly int _performerId;
    private readonly string _performerName;
    private readonly string _performerFigure;

    private JudgeGuiStatusMessageEvent(JudgeGuiStatus status, int performerId, string performerName, string performerFigure)
    {
        _status = status;
        _performerId = performerId;
        _performerName = performerName;
        _performerFigure = performerFigure;
    }

    public static JudgeGuiStatusMessageEvent Closed() => new(JudgeGuiStatus.Closed, 0, "", "");

    public static JudgeGuiStatusMessageEvent Waiting() => new(JudgeGuiStatus.Waiting, 0, "", "");

    public static JudgeGuiStatusMessageEvent Voting(int performerId, string performerName, string performerFigure) =>
        new(JudgeGuiStatus.Voting, performerId, performerName, performerFigure);

    public ServerPacket Compose()
    {
        var packet = new ServerPacket(OutgoingOpcodes.Idol.JudgeGuiStatusMessageEvent);
        packet.AppendInt32((int)_status);
        if (_status == JudgeGuiStatus.Voting)
        {
            packet.AppendInt32(_performerId);
            packet.AppendString(_performerName);
            packet.AppendString(_performerFigure);
        }

        return packet;
    }
}
