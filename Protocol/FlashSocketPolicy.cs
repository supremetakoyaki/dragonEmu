using System.Text;

namespace DragonEmu.Protocol;

public static class FlashSocketPolicy
{
    private const byte WedgieByteFloor = 0x40;

    public static bool LooksLikePolicyRequest(byte firstByte) => firstByte < WedgieByteFloor;

    public static byte[] BuildResponse(params int[] allowedPorts)
    {
        string ports = string.Join(",", allowedPorts);
        string xml =
            "<?xml version=\"1.0\"?>\n" +
            "<!DOCTYPE cross-domain-policy SYSTEM \"/xml/dtds/cross-domain-policy.dtd\">\n" +
            "<cross-domain-policy>\n" +
            $"<allow-access-from domain=\"*\" to-ports=\"{ports}\" />\n" +
            "</cross-domain-policy>\0";

        return Encoding.UTF8.GetBytes(xml);
    }
}
