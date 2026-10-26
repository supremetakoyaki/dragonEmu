using DragonEmu.Protocol;

namespace DragonEmu.Gateway;

public interface IPacketDispatcher
{
    ValueTask DispatchAsync(ClientSession session, ClientPacket packet, CancellationToken cancellationToken);
}
