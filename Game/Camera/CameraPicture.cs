using System.Buffers.Binary;
using System.IO.Compression;
using System.Text;

namespace DragonEmu.Game.Camera;

public static class CameraPicture
{
    public const int Width = 161;
    public const int Height = 117;

    public const int MaxEncodedLength = 40_000;

    private static readonly (int R, int G, int B) Background = (0xFF, 0xCC, 0x66);
    private static readonly (int R, int G, int B) Foreground = (0x68, 0x1F, 0x10);

    private static readonly byte[] PngSignature = [0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A];
    private static readonly uint[] CrcTable = BuildCrcTable();

    public static bool TryRead(string encoded, out byte[] pixels)
    {
        pixels = [];
        if (encoded.Length is 0 or > MaxEncodedLength)
        {
            return false;
        }

        byte[] compressed;
        try
        {
            compressed = Convert.FromBase64String(encoded);
        }
        catch (FormatException)
        {
            return false;
        }

        var read = new byte[Width * Height + 1];
        int length = 0;
        try
        {
            using var zlib = new ZLibStream(new MemoryStream(compressed), CompressionMode.Decompress);
            int got;
            while (length < read.Length && (got = zlib.Read(read, length, read.Length - length)) > 0)
            {
                length += got;
            }
        }
        catch (InvalidDataException)
        {
            return false;
        }

        if (length != Width * Height)
        {
            return false;
        }

        pixels = read[..length];
        return true;
    }

    public static uint Checksum(string token, ReadOnlySpan<byte> pixels)
    {
        const uint Modulus = 65521;
        uint a = 1;
        uint b = 0;
        foreach (byte value in Encoding.UTF8.GetBytes(token))
        {
            a = (a + value) % Modulus;
            b = (b + a) % Modulus;
        }

        foreach (byte value in pixels)
        {
            a = (a + value) % Modulus;
            b = (b + a) % Modulus;
        }

        return (b << 16) | a;
    }

    public static (byte R, byte G, byte B) Sepia(byte gray) =>
        (Tint(gray, Background.R, Foreground.R), Tint(gray, Background.G, Foreground.G), Tint(gray, Background.B, Foreground.B));

    public static byte[] ToPng(ReadOnlySpan<byte> pixels)
    {
        if (pixels.Length != Width * Height)
        {
            throw new ArgumentException($"A picture is {Width}x{Height} pixels.", nameof(pixels));
        }

        var raw = new byte[Height * (1 + Width * 3)];
        for (int y = 0, at = 0; y < Height; y++)
        {
            raw[at++] = 0;
            for (int x = 0; x < Width; x++)
            {
                (byte r, byte g, byte b) = Sepia(pixels[y * Width + x]);
                raw[at++] = r;
                raw[at++] = g;
                raw[at++] = b;
            }
        }

        var header = new byte[13];
        BinaryPrimitives.WriteInt32BigEndian(header, Width);
        BinaryPrimitives.WriteInt32BigEndian(header.AsSpan(4), Height);
        header[8] = 8;
        header[9] = 2;

        using var compressed = new MemoryStream();
        using (var zlib = new ZLibStream(compressed, CompressionLevel.SmallestSize, leaveOpen: true))
        {
            zlib.Write(raw);
        }

        using var png = new MemoryStream();
        png.Write(PngSignature);
        WriteChunk(png, "IHDR", header);
        WriteChunk(png, "IDAT", compressed.ToArray());
        WriteChunk(png, "IEND", []);
        return png.ToArray();
    }

    private static byte Tint(byte gray, int background, int foreground) =>
        (byte)Math.Min(255, (background == 255 ? gray : gray * background >> 8) + foreground);

    private static void WriteChunk(Stream png, string type, ReadOnlySpan<byte> data)
    {
        Span<byte> number = stackalloc byte[4];
        BinaryPrimitives.WriteInt32BigEndian(number, data.Length);
        png.Write(number);

        byte[] typeBytes = Encoding.ASCII.GetBytes(type);
        png.Write(typeBytes);
        png.Write(data);

        uint crc = Crc(Crc(0xFFFFFFFF, typeBytes), data) ^ 0xFFFFFFFF;
        BinaryPrimitives.WriteUInt32BigEndian(number, crc);
        png.Write(number);
    }

    private static uint Crc(uint crc, ReadOnlySpan<byte> data)
    {
        foreach (byte value in data)
        {
            crc = CrcTable[(crc ^ value) & 0xFF] ^ (crc >> 8);
        }

        return crc;
    }

    private static uint[] BuildCrcTable()
    {
        var table = new uint[256];
        for (uint n = 0; n < 256; n++)
        {
            uint c = n;
            for (int k = 0; k < 8; k++)
            {
                c = (c & 1) != 0 ? 0xEDB88320 ^ (c >> 1) : c >> 1;
            }

            table[n] = c;
        }

        return table;
    }
}
