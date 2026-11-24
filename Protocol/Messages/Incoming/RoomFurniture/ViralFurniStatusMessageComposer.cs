namespace DragonEmu.Protocol.Messages.Incoming.RoomFurniture;

public sealed class ViralFurniStatusMessageComposer : IMessageComposer
{
    public void Parse(ClientPacket packet)
    {
    }

    public ValueTask HandleAsync(IGameSession session, CancellationToken cancellationToken) => ValueTask.CompletedTask;
}
