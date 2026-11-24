using DragonEmu.Protocol.Messages.Outgoing.Notifications;

namespace DragonEmu.Protocol.Messages.Incoming.Users;

public sealed class GetMOTDMessageComposer : IMessageComposer
{
    public void Parse(ClientPacket packet)
    {
    }

    public ValueTask HandleAsync(IGameSession session, CancellationToken cancellationToken)
    {
        session.Send(new MOTDNotificationEvent([]).Compose());
        return ValueTask.CompletedTask;
    }
}
