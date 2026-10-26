using DragonEmu.Domain;

namespace DragonEmu.Protocol.Messages.Outgoing.FriendList;

public sealed record FriendEntry(MessengerProfile Profile, bool Online, string LastAccess);

internal static class FriendWireFormat
{
    public static void AppendFriend(ServerPacket packet, FriendEntry entry)
    {
        MessengerProfile profile = entry.Profile;
        packet.AppendInt32(profile.Id);
        packet.AppendString(profile.Name);
        packet.AppendInt32(GenderOf(profile));
        packet.AppendBoolean(entry.Online);
        packet.AppendBoolean(profile.AllowsFollowing);
        packet.AppendString(profile.Figure);
        packet.AppendInt32(0);
        packet.AppendString(profile.Motto);
        packet.AppendString(entry.LastAccess);
        packet.AppendString(string.Empty);
    }

    public static void AppendSearchHit(ServerPacket packet, FriendEntry entry)
    {
        MessengerProfile profile = entry.Profile;
        packet.AppendInt32(profile.Id);
        packet.AppendString(profile.Name);
        packet.AppendString(profile.Motto);
        packet.AppendBoolean(entry.Online);
        packet.AppendBoolean(false);
        packet.AppendString(string.Empty);
        packet.AppendInt32(GenderOf(profile));
        packet.AppendString(profile.Figure);
        packet.AppendString(entry.LastAccess);
        packet.AppendString(string.Empty);
    }

    private static int GenderOf(MessengerProfile profile) => char.ToUpperInvariant(profile.Gender) == 'F' ? 0 : 1;
}
