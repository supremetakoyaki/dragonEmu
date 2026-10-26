using System.Text;

namespace DragonEmu.Protocol;

public sealed class ClientPacket
{
    private readonly byte[] _body;
    private int _position;

    public int Id { get; }

    public int Length => _body.Length;

    public ReadOnlySpan<byte> Body => _body;

    public int RemainingLength => _body.Length - _position;

    public ClientPacket(int id, byte[] body)
    {
        Id = id;
        _body = body;
    }

    public int PopInt32()
    {
        if (RemainingLength < 1)
        {
            return 0;
        }

        int value = WedgieCodec.DecodeVariable(_body.AsSpan(_position), out int bytesRead);
        _position += bytesRead;
        return value;
    }

    public uint PopUInt32() => unchecked((uint)PopInt32());

    public IReadOnlyList<int> PopInt32List(int max)
    {
        int count = Math.Clamp(PopInt32(), 0, max);
        var values = new List<int>(count);
        while (values.Count < count && RemainingLength > 0)
        {
            values.Add(PopInt32());
        }

        return values;
    }

    public bool PopBoolean()
    {
        if (RemainingLength < 1)
        {
            return false;
        }

        return _body[_position++] == WedgieCodec.BooleanTrue;
    }

    public string PopString() => PopString(Encoding.UTF8);

    public string PopString(Encoding encoding)
    {
        if (RemainingLength < 2)
        {
            return string.Empty;
        }

        int length = WedgieCodec.DecodeFixed(_body.AsSpan(_position, 2));
        _position += 2;

        length = Math.Clamp(length, 0, RemainingLength);
        string value = encoding.GetString(_body, _position, length);
        _position += length;
        return value;
    }

    public byte[] PopBytes(int count)
    {
        count = Math.Clamp(count, 0, RemainingLength);
        byte[] slice = new byte[count];
        Array.Copy(_body, _position, slice, 0, count);
        _position += count;
        return slice;
    }
}
