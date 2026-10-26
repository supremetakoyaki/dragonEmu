namespace DragonEmu.Protocol.Messages.Outgoing.RoomChat;

internal static class SpeechMessage
{
    public static ServerPacket Compose(int opcode, int virtualId, string message)
    {
        var packet = new ServerPacket(opcode);
        packet.AppendInt32(virtualId);
        packet.AppendString(message);
        packet.AppendInt32(DetectEmotion(message));
        return packet;
    }

    private static int DetectEmotion(string message)
    {
        if (message.Contains(":)") || message.Contains(":D") || message.Contains("=)"))
        {
            return 1;
        }

        if (message.Contains(">:(") || message.Contains(":@"))
        {
            return 2;
        }

        if (message.Contains(":o") || message.Contains(":O") || message.Contains(":0"))
        {
            return 3;
        }

        if (message.Contains(":(") || message.Contains(":'("))
        {
            return 4;
        }

        return 0;
    }
}
