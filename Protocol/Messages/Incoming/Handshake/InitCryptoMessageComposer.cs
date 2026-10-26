using DragonEmu.Protocol.Messages.Outgoing.Handshake;

namespace DragonEmu.Protocol.Messages.Incoming.Handshake;

public sealed class InitCryptoMessageComposer : IMessageComposer
{
    public void Parse(ClientPacket packet)
    {
    }

    public ValueTask HandleAsync(IGameSession session, CancellationToken cancellationToken)
    {
        session.Send(new SessionParamsMessageEvent().Compose());
        return ValueTask.CompletedTask;
    }
}
