namespace DragonEmu.Gateway;

public interface IClientConnection : IAsyncDisposable
{
    string RemoteAddress { get; }

    ValueTask<int> ReceiveAsync(Memory<byte> buffer, CancellationToken cancellationToken);

    ValueTask SendAsync(ReadOnlyMemory<byte> data, CancellationToken cancellationToken);
}
