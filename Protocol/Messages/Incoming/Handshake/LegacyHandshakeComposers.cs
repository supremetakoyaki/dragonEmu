namespace DragonEmu.Protocol.Messages.Incoming.Handshake;

public sealed class DisconnectMessageComposer : IMessageComposer
{
    public void Parse(ClientPacket packet)
    {
    }

    public ValueTask HandleAsync(IGameSession session, CancellationToken cancellationToken)
    {
        session.Close("the client said goodbye");
        return ValueTask.CompletedTask;
    }
}

public sealed class TryLoginMessageComposer : IMessageComposer
{
    public void Parse(ClientPacket packet)
    {
    }

    public ValueTask HandleAsync(IGameSession session, CancellationToken cancellationToken)
    {
        session.Close("it sent the old password login");
        return ValueTask.CompletedTask;
    }
}

public sealed class UniqueIDMessageComposer : IMessageComposer
{
    public void Parse(ClientPacket packet)
    {
    }

    public ValueTask HandleAsync(IGameSession session, CancellationToken cancellationToken) => ValueTask.CompletedTask;
}

public sealed class VersionCheckMessageComposer : IMessageComposer
{
    public void Parse(ClientPacket packet)
    {
    }

    public ValueTask HandleAsync(IGameSession session, CancellationToken cancellationToken) => ValueTask.CompletedTask;
}

public sealed class GetSessionParametersMessageComposer : IMessageComposer
{
    public void Parse(ClientPacket packet)
    {
    }

    public ValueTask HandleAsync(IGameSession session, CancellationToken cancellationToken) => ValueTask.CompletedTask;
}

public sealed class GenerateSecretKeyMessageComposer : IMessageComposer
{
    public void Parse(ClientPacket packet)
    {
    }

    public ValueTask HandleAsync(IGameSession session, CancellationToken cancellationToken) => ValueTask.CompletedTask;
}
