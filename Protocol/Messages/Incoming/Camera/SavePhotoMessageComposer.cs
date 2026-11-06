using System.Text;
using DragonEmu.Game.Camera;

namespace DragonEmu.Protocol.Messages.Incoming.Camera;

public sealed class SavePhotoMessageComposer : IMessageComposer
{
    public const int MaxParts = CameraPicture.MaxEncodedLength / 4000 + 1;

    private readonly CameraService _camera;

    private uint _checksum;
    private string _caption = string.Empty;
    private string _picture = string.Empty;

    public SavePhotoMessageComposer(CameraService camera)
    {
        _camera = camera;
    }

    public void Parse(ClientPacket packet)
    {
        _checksum = packet.PopUInt32();
        _caption = packet.PopString();
        var picture = new StringBuilder();
        for (int parts = Math.Clamp(packet.PopInt32(), 0, MaxParts); parts > 0; parts--)
        {
            picture.Append(packet.PopString());
        }

        _picture = picture.ToString();
    }

    public async ValueTask HandleAsync(IGameSession session, CancellationToken cancellationToken)
    {
        if (session.AuthenticatedUser is { } account)
        {
            await _camera.SaveAsync(session, account, _checksum, _caption, _picture, cancellationToken);
        }
    }
}
