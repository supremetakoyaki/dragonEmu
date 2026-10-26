using System.Security.Cryptography;
using System.Text;

namespace DragonEmu.Game.Vouchers;

public static class VoucherHasher
{
    public static string Hash(string code) =>
        Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(code))).ToLowerInvariant();
}
