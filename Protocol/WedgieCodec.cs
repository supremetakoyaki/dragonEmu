namespace DragonEmu.Protocol;

public static class WedgieCodec
{
    public const byte FrameTerminator = 1;
    public const byte StringTerminator = 2;

    public const byte BooleanFalse = 0x40;
    public const byte BooleanTrue = 0x41;

    public const byte WireBooleanFalse = 72;
    public const byte WireBooleanTrue = 73;

    private const byte ByteBase = 0x40;
    private const byte ValueMask = 0x3F;

    public static void EncodeFixed(int value, Span<byte> destination)
    {
        int byteCount = destination.Length;
        for (int j = 1; j <= byteCount; j++)
        {
            int shift = (byteCount - j) * 6;
            destination[j - 1] = (byte)(ByteBase | ((value >> shift) & ValueMask));
        }
    }

    public static int DecodeFixed(ReadOnlySpan<byte> source)
    {
        int value = 0;
        for (int i = 0; i < source.Length; i++)
        {
            value = (value << 6) | (source[i] & ValueMask);
        }

        return value;
    }

    public static int EncodeVariable(int value, Span<byte> destination)
    {
        bool negative = value < 0;
        value = Math.Abs(value);

        Span<byte> temp = stackalloc byte[6];
        int numBytes = 1;
        temp[0] = (byte)(ByteBase | (value & 0x03));
        value >>= 2;
        while (value != 0)
        {
            temp[numBytes] = (byte)(ByteBase | (value & ValueMask));
            numBytes++;
            value >>= 6;
        }

        temp[0] = (byte)(temp[0] | (numBytes << 3) | (negative ? 0x04 : 0x00));
        temp[..numBytes].CopyTo(destination);
        return numBytes;
    }

    public static int GetVariableLength(int value)
    {
        value = Math.Abs(value);
        int numBytes = 1;
        value >>= 2;
        while (value != 0)
        {
            numBytes++;
            value >>= 6;
        }

        return numBytes;
    }

    public static int DecodeVariable(ReadOnlySpan<byte> source, out int bytesRead)
    {
        if (source.Length == 0)
        {
            bytesRead = 0;
            return 0;
        }

        byte first = (byte)(source[0] & ValueMask);
        int numBytes = Math.Max(1, (first & 0x38) >> 3);
        bool negative = (first & 0x04) == 0x04;
        int value = first & 0x03;

        int shift = 2;
        for (int i = 1; i < numBytes && i < source.Length; i++)
        {
            value |= (source[i] & ValueMask) << shift;
            shift += 6;
        }

        bytesRead = Math.Min(numBytes, source.Length);
        return negative ? -value : value;
    }
}
