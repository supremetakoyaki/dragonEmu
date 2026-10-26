using System.Text;

namespace DragonEmu.Protocol;

public sealed class ServerPacket
{
    private readonly List<byte> _body = new();

    public int Id { get; }

    public ServerPacket(int id)
    {
        Id = id;
    }

    public void AppendInt32(int value)
    {
        Span<byte> buffer = stackalloc byte[6];
        int bytesWritten = WedgieCodec.EncodeVariable(value, buffer);
        _body.AddRange(buffer[..bytesWritten].ToArray());
    }

    public void AppendUInt32(uint value) => AppendInt32(unchecked((int)value));

    public void AppendBoolean(bool value) =>
        _body.Add(value ? WedgieCodec.WireBooleanTrue : WedgieCodec.WireBooleanFalse);

    public void AppendString(string? value) => AppendString(value, Encoding.UTF8);

    public void AppendString(string? value, Encoding encoding)
    {
        if (!string.IsNullOrEmpty(value))
        {
            _body.AddRange(encoding.GetBytes(value));
        }

        _body.Add(WedgieCodec.StringTerminator);
    }

    public void AppendBytes(ReadOnlySpan<byte> data) => _body.AddRange(data.ToArray());

    public byte[] ToArray()
    {
        byte[] frame = new byte[2 + _body.Count + 1];
        WedgieCodec.EncodeFixed(Id, frame.AsSpan(0, 2));
        _body.CopyTo(frame, 2);
        frame[^1] = WedgieCodec.FrameTerminator;
        return frame;
    }
}
