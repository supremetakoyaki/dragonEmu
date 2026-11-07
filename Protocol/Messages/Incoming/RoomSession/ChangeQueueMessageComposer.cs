using DragonEmu.Domain;
using DragonEmu.Game.Rooms;

namespace DragonEmu.Protocol.Messages.Incoming.RoomSession;

public sealed class ChangeQueueMessageComposer : IMessageComposer
{
    private readonly RoomQueueService _queue;

    private int _target;

    public ChangeQueueMessageComposer(RoomQueueService queue)
    {
        _queue = queue;
    }

    public void Parse(ClientPacket packet)
    {
        _target = packet.PopInt32();
    }

    public ValueTask HandleAsync(IGameSession session, CancellationToken cancellationToken)
    {
        if (Enum.IsDefined((RoomQueueLine)_target))
        {
            _queue.ChangeLine(session, (RoomQueueLine)_target);
        }

        return ValueTask.CompletedTask;
    }
}
