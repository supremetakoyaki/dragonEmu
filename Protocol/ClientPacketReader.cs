using System.Net;

namespace DragonEmu.Protocol;

public sealed class ClientPacketReader
{
    private const int LengthPrefixSize = 3;
    private const int HeaderSize = 2;

    private readonly int _maxFrameSize;
    private byte[] _buffer;
    private int _count;

    public ClientPacketReader(int maxFrameSize = 64 * 1024, int initialCapacity = 512)
    {
        _maxFrameSize = maxFrameSize;
        _buffer = new byte[initialCapacity];
    }

    public void Append(ReadOnlySpan<byte> data)
    {
        EnsureCapacity(_count + data.Length);
        data.CopyTo(_buffer.AsSpan(_count));
        _count += data.Length;
    }

    public bool TryReadPacket(out ClientPacket? packet)
    {
        packet = null;

        if (_count < LengthPrefixSize)
        {
            return false;
        }

        int frameLength = WedgieCodec.DecodeFixed(_buffer.AsSpan(0, LengthPrefixSize));
        if (frameLength < HeaderSize)
        {
            throw new ProtocolViolationException($"Invalid frame length {frameLength}.");
        }

        if (frameLength > _maxFrameSize)
        {
            throw new ProtocolViolationException($"Frame length {frameLength} exceeds the maximum of {_maxFrameSize}.");
        }

        int totalFrameSize = LengthPrefixSize + frameLength;
        if (_count < totalFrameSize)
        {
            return false;
        }

        int id = WedgieCodec.DecodeFixed(_buffer.AsSpan(LengthPrefixSize, HeaderSize));
        int bodyLength = frameLength - HeaderSize;
        byte[] body = new byte[bodyLength];
        Array.Copy(_buffer, LengthPrefixSize + HeaderSize, body, 0, bodyLength);

        Consume(totalFrameSize);

        packet = new ClientPacket(id, body);
        return true;
    }

    private void Consume(int byteCount)
    {
        int remaining = _count - byteCount;
        if (remaining > 0)
        {
            Array.Copy(_buffer, byteCount, _buffer, 0, remaining);
        }

        _count = remaining;
    }

    private void EnsureCapacity(int required)
    {
        if (required <= _buffer.Length)
        {
            return;
        }

        int ceiling = _maxFrameSize + LengthPrefixSize + HeaderSize;
        if (required > ceiling)
        {
            throw new ProtocolViolationException("Buffered data exceeds the maximum allowed for a single frame. The peer is likely misbehaving >:(");
        }

        int newCapacity = _buffer.Length * 2;
        while (newCapacity < required)
        {
            newCapacity *= 2;
        }

        Array.Resize(ref _buffer, Math.Min(newCapacity, ceiling));
    }
}
