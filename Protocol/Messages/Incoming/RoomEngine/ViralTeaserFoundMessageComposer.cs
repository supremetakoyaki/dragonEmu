namespace DragonEmu.Protocol.Messages.Incoming.RoomEngine;

public sealed class ViralTeaserFoundMessageComposer : IMessageComposer
{
    public void Parse(ClientPacket packet)
    {
    }

    public ValueTask HandleAsync(IGameSession session, CancellationToken cancellationToken) => ValueTask.CompletedTask;
}
