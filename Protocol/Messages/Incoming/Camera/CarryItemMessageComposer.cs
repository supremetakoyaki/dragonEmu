using DragonEmu.Game.Camera;

namespace DragonEmu.Protocol.Messages.Incoming.Camera;

public sealed class CarryItemMessageComposer : IMessageComposer
{
    private readonly CameraService _camera;

    private int _item;

    public CarryItemMessageComposer(CameraService camera)
    {
        _camera = camera;
    }

    public void Parse(ClientPacket packet)
    {
        _item = packet.PopInt32();
    }

    public async ValueTask HandleAsync(IGameSession session, CancellationToken cancellationToken)
    {
        if (session.AuthenticatedUser is { } account)
        {
            await _camera.CarryAsync(session, account, _item, cancellationToken);
        }
    }
}
