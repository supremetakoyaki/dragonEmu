using DragonEmu.Game.Camera;

namespace DragonEmu.Protocol.Messages.Incoming.Camera;

public sealed class CameraClientMessageComposer : IMessageComposer
{
    private readonly CameraService _camera;

    public CameraClientMessageComposer(CameraService camera)
    {
        _camera = camera;
    }

    public void Parse(ClientPacket packet)
    {
    }

    public ValueTask HandleAsync(IGameSession session, CancellationToken cancellationToken)
    {
        _camera.WindowsLoaded(session);
        return ValueTask.CompletedTask;
    }
}
