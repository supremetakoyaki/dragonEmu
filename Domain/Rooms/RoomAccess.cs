using System.Security.Cryptography;
using System.Text;

namespace DragonEmu.Domain;

public static class RoomAccess
{
    public const int Open = 0;

    public const int Doorbell = 1;

    public const int Password = 2;

    public static (int AccessType, string? PasswordHash) Resolve(int? ownerId, int accessType, string? passwordHash) =>
        ownerId is null ? (Open, null)
        : accessType == Password && !RoomPassword.IsHash(passwordHash) ? (Doorbell, null)
        : accessType == Password ? (Password, passwordHash)
        : (accessType is Open or Doorbell ? accessType : Doorbell, null);
}

public static class RoomPassword
{
    public static string Hash(string password) => Convert.ToHexStringLower(SHA1.HashData(Encoding.UTF8.GetBytes(password)));

    public static bool IsHash(string? stored) => stored is { Length: 40 } && stored.All(char.IsAsciiHexDigitLower);

    public static bool Matches(string attempt, string? stored) =>
        IsHash(stored) && CryptographicOperations.FixedTimeEquals(Encoding.ASCII.GetBytes(Hash(attempt)), Encoding.ASCII.GetBytes(stored!));
}
