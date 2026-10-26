using System.Text;
using DragonEmu.Protocol;
using Microsoft.Extensions.Logging;

namespace DragonEmu.Gateway;

public static class PacketLog
{
    public static void LogIncoming(this ILogger logger, ClientPacket packet, bool handled)
    {
        if (logger.IsEnabled(LogLevel.Debug))
        {
            string name = handled ? OpcodeNames.Incoming(packet.Id) : $"{OpcodeNames.Incoming(packet.Id)} (unhandled)";
            logger.LogDebug("in:  {Opcode} {Name} {Body}", packet.Id, name, Render(packet.Body));
        }
    }

    public static void LogOutgoing(this ILogger logger, byte[] frame)
    {
        if (logger.IsEnabled(LogLevel.Debug) && frame.Length >= 3)
        {
            int opcode = WedgieCodec.DecodeFixed(frame.AsSpan(0, 2));
            logger.LogDebug("out: {Opcode} {Name} {Body}", opcode, OpcodeNames.Outgoing(opcode), Render(frame.AsSpan(2, frame.Length - 3)));
        }
    }

    private static string Render(ReadOnlySpan<byte> body)
    {
        string text = Encoding.UTF8.GetString(body);
        var rendered = new StringBuilder(text.Length);
        foreach (char character in text)
        {
            if (character < ' ')
            {
                rendered.Append('[').Append((int)character).Append(']');
            }
            else
            {
                rendered.Append(character);
            }
        }

        return rendered.ToString();
    }
}
