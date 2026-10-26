namespace DragonEmu.Protocol.Messages;

public interface IMessageComposer
{
    void Parse(ClientPacket packet);

    ValueTask HandleAsync(IGameSession session, CancellationToken cancellationToken);
}
