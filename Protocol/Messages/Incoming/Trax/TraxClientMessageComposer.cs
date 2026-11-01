namespace DragonEmu.Protocol.Messages.Incoming.Trax;

public sealed class TraxClientMessageComposer : IMessageComposer
{
    public void Parse(ClientPacket packet)
    {
    }

    public ValueTask HandleAsync(IGameSession session, CancellationToken cancellationToken)
    {
        session.TraxClient = true;
        return ValueTask.CompletedTask;
    }
}
