using System.Security.Cryptography;
using System.Text;

namespace DragonEmu.Gateway;

public sealed record WebSocketUpgrade(string Key, string? Origin);

public static class WebSocketHandshake
{
    public const int MaxRequestBytes = 8192;

    private const string KeyGuid = "258EAFA5-E914-47DA-95CA-C5AB0DC85B11";

    public static int HeaderEnd(ReadOnlySpan<byte> data) =>
        data.IndexOf("\r\n\r\n"u8) is var at and >= 0 ? at + 4 : -1;

    public static WebSocketUpgrade? Parse(ReadOnlySpan<byte> head)
    {
        string[] lines = Encoding.ASCII.GetString(head).Split("\r\n");
        string[] requestLine = lines[0].Split(' ');
        if (requestLine.Length != 3 || requestLine[0] != "GET" || !requestLine[2].StartsWith("HTTP/1.1", StringComparison.Ordinal))
        {
            return null;
        }

        var headers = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        foreach (string line in lines.Skip(1))
        {
            int colon = line.IndexOf(':');
            if (colon > 0)
            {
                headers[line[..colon].Trim()] = line[(colon + 1)..].Trim();
            }
        }

        bool upgrade = headers.TryGetValue("Upgrade", out string? upgradeTo) && HasToken(upgradeTo, "websocket");
        bool connection = headers.TryGetValue("Connection", out string? connectionTokens) && HasToken(connectionTokens, "upgrade");
        bool version = headers.TryGetValue("Sec-WebSocket-Version", out string? versionNumber) && versionNumber == "13";
        if (!upgrade || !connection || !version || !headers.TryGetValue("Sec-WebSocket-Key", out string? key) || !IsValidKey(key))
        {
            return null;
        }

        return new WebSocketUpgrade(key, headers.GetValueOrDefault("Origin"));
    }

    public static string AcceptKey(string key) => Convert.ToBase64String(SHA1.HashData(Encoding.ASCII.GetBytes(key + KeyGuid)));

    public static byte[] SwitchingProtocols(string key) => Encoding.ASCII.GetBytes(
        "HTTP/1.1 101 Switching Protocols\r\n" +
        "Upgrade: websocket\r\n" +
        "Connection: Upgrade\r\n" +
        $"Sec-WebSocket-Accept: {AcceptKey(key)}\r\n\r\n");

    public static byte[] Refusal(int status, string reason) => Encoding.ASCII.GetBytes(
        $"HTTP/1.1 {status} {reason}\r\nConnection: close\r\nContent-Length: 0\r\n\r\n");

    public static bool IsAllowedOrigin(string? origin, IReadOnlyCollection<string> allowed) =>
        allowed.Count == 0
        || (origin is not null && allowed.Any(entry => string.Equals(entry.TrimEnd('/'), origin.TrimEnd('/'), StringComparison.OrdinalIgnoreCase)));

    private static bool HasToken(string value, string token) =>
        value.Split(',').Any(part => string.Equals(part.Trim(), token, StringComparison.OrdinalIgnoreCase));

    private static bool IsValidKey(string key)
    {
        Span<byte> bytes = stackalloc byte[24];
        return Convert.TryFromBase64String(key, bytes, out int written) && written == 16;
    }
}
