namespace DragonEmu.Protocol.Messages.Incoming.Users;

public sealed class GetAchievementShareIdComposer : IMessageComposer
{
    public void Parse(ClientPacket packet)
    {
    }

    public ValueTask HandleAsync(IGameSession session, CancellationToken cancellationToken) => ValueTask.CompletedTask;
}
