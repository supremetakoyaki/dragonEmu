namespace DragonEmu.Protocol.Messages.Incoming.FriendList;

public sealed class FriendListUpdateMessageComposer : IMessageComposer
{
    public void Parse(ClientPacket packet)
    {
    }

    public ValueTask HandleAsync(IGameSession session, CancellationToken cancellationToken) => ValueTask.CompletedTask;
}
