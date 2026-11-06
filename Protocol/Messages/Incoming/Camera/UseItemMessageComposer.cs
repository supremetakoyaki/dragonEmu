using DragonEmu.Game.Camera;

namespace DragonEmu.Protocol.Messages.Incoming.Camera;

public sealed class UseItemMessageComposer : IMessageComposer
{
    private readonly CameraService _camera;

    private int _item;

    public UseItemMessageComposer(CameraService camera)
    {
        _camera = camera;
    }

    public void Parse(ClientPacket packet)
    {
        _item = packet.PopInt32();
    }

    public ValueTask HandleAsync(IGameSession session, CancellationToken cancellationToken)
    {
        if (session.AuthenticatedUser is not null)
        {
            _camera.Use(session, _item);
        }

        return ValueTask.CompletedTask;
    }
}
