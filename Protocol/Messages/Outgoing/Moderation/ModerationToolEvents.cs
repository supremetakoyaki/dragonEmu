using DragonEmu.Domain;

namespace DragonEmu.Protocol.Messages.Outgoing.Moderation;

public sealed record ModeratorPermissions(
    bool Tickets, bool Chatlogs, bool AlertUser, bool KickUser, bool BanUser, bool AlertRoom, bool KickRoom, bool WordFilter);

public sealed class ModeratorInitMessageEvent : IMessageEvent
{
    private readonly IReadOnlyList<ModerationIssue> _issues;
    private readonly ModerationPresets _presets;
    private readonly ModeratorPermissions _permissions;
    private readonly DateTime _utcNow;

    public ModeratorInitMessageEvent(IReadOnlyList<ModerationIssue> issues, ModerationPresets presets, ModeratorPermissions permissions, DateTime utcNow)
    {
        _issues = issues;
        _presets = presets;
        _permissions = permissions;
        _utcNow = utcNow;
    }

    public ServerPacket Compose()
    {
        var packet = new ServerPacket(OutgoingOpcodes.Moderation.ModeratorInitMessageEvent);
        packet.AppendInt32(_issues.Count);
        foreach (ModerationIssue issue in _issues)
        {
            IssueInfoMessageEvent.AppendIssue(packet, issue, _utcNow);
        }

        AppendStrings(packet, _presets.UserMessages);
        packet.AppendInt32(_presets.Offences.Count);
        foreach (OffenceCategory category in _presets.Offences)
        {
            packet.AppendString(category.Name);
            packet.AppendInt32(category.Offences.Count);
            foreach ((string name, string message) in category.Offences)
            {
                packet.AppendString(name);
                packet.AppendString(message);
            }
        }

        packet.AppendBoolean(_permissions.Tickets);
        packet.AppendBoolean(_permissions.Chatlogs);
        packet.AppendBoolean(_permissions.AlertUser);
        packet.AppendBoolean(_permissions.KickUser);
        packet.AppendBoolean(_permissions.BanUser);
        packet.AppendBoolean(_permissions.AlertRoom);
        packet.AppendBoolean(_permissions.KickRoom);
        AppendStrings(packet, _presets.RoomMessages);
        packet.AppendBoolean(_permissions.WordFilter);
        return packet;
    }

    private static void AppendStrings(ServerPacket packet, IReadOnlyList<string> values)
    {
        packet.AppendInt32(values.Count);
        foreach (string value in values)
        {
            packet.AppendString(value);
        }
    }
}

public sealed class IssueInfoMessageEvent : IMessageEvent
{
    private const int PublicRoom = 0;
    private const int GuestRoom = 1;

    private readonly ModerationIssue _issue;
    private readonly DateTime _utcNow;

    public IssueInfoMessageEvent(ModerationIssue issue, DateTime utcNow)
    {
        _issue = issue;
        _utcNow = utcNow;
    }

    public ServerPacket Compose()
    {
        var packet = new ServerPacket(OutgoingOpcodes.Moderation.IssueInfoMessageEvent);
        AppendIssue(packet, _issue, _utcNow);
        return packet;
    }

    public static void AppendIssue(ServerPacket packet, ModerationIssue issue, DateTime utcNow)
    {
        packet.AppendInt32(issue.Id);
        packet.AppendInt32((int)issue.State);
        packet.AppendInt32(issue.Category);
        packet.AppendInt32(issue.Category);
        packet.AppendInt32((int)Math.Clamp((utcNow - issue.CreatedAt).TotalMilliseconds, 0, int.MaxValue));
        packet.AppendInt32(1);
        packet.AppendInt32(issue.ReporterId);
        packet.AppendString(issue.ReporterName);
        packet.AppendInt32(issue.ReportedId ?? 0);
        packet.AppendString(issue.ReportedName);
        packet.AppendInt32(issue.PickerId ?? 0);
        packet.AppendString(issue.PickerName);
        packet.AppendString(issue.Message);
        packet.AppendInt32(0);
        packet.AppendString(issue.RoomName);
        if (issue.RoomIsPublic)
        {
            packet.AppendInt32(PublicRoom);
            packet.AppendString(string.Empty);
            packet.AppendInt32(issue.RoomId ?? 0);
            packet.AppendInt32(0);
        }
        else
        {
            packet.AppendInt32(GuestRoom);
            packet.AppendString(string.Empty);
            packet.AppendInt32(issue.RoomId ?? 0);
            packet.AppendString(string.Empty);
        }
    }
}

public sealed class IssueDeletedMessageEvent : IMessageEvent
{
    private readonly int _issueId;

    public IssueDeletedMessageEvent(int issueId)
    {
        _issueId = issueId;
    }

    public ServerPacket Compose()
    {
        var packet = new ServerPacket(OutgoingOpcodes.Moderation.IssueDeletedMessageEvent);
        packet.AppendString(_issueId.ToString(System.Globalization.CultureInfo.InvariantCulture));
        return packet;
    }
}

public sealed class IssuePickFailedMessageEvent : IMessageEvent
{
    private readonly int _issueId;
    private readonly int _pickerId;
    private readonly string _pickerName;

    public IssuePickFailedMessageEvent(int issueId, int pickerId, string pickerName)
    {
        _issueId = issueId;
        _pickerId = pickerId;
        _pickerName = pickerName;
    }

    public ServerPacket Compose()
    {
        var packet = new ServerPacket(OutgoingOpcodes.Moderation.IssuePickFailedMessageEvent);
        packet.AppendInt32(_issueId);
        packet.AppendInt32(_pickerId);
        packet.AppendString(_pickerName);
        return packet;
    }
}

public sealed class ModeratorUserInfoEvent : IMessageEvent
{
    private readonly ModeratorUserInfo _info;
    private readonly bool _online;
    private readonly DateTime _utcNow;

    public ModeratorUserInfoEvent(ModeratorUserInfo info, bool online, DateTime utcNow)
    {
        _info = info;
        _online = online;
        _utcNow = utcNow;
    }

    public ServerPacket Compose()
    {
        var packet = new ServerPacket(OutgoingOpcodes.Moderation.ModeratorUserInfoEvent);
        packet.AppendInt32(_info.UserId);
        packet.AppendString(_info.Username);
        packet.AppendInt32(MinutesSince(_info.CreatedAt));
        packet.AppendInt32(_info.LastLoginAt is { } lastLogin ? MinutesSince(lastLogin) : 0);
        packet.AppendBoolean(_online);
        packet.AppendInt32(_info.Reports);
        packet.AppendInt32(_info.AbusiveReports);
        packet.AppendInt32(_info.Cautions);
        packet.AppendInt32(_info.Bans);
        return packet;
    }

    private int MinutesSince(DateTime utc) => (int)Math.Clamp((_utcNow - utc).TotalMinutes, 0, int.MaxValue);
}

public sealed class RoomChatlogEvent : IMessageEvent
{
    private readonly ChatlogRoom _room;
    private readonly Func<DateTime, DateTime> _hotelTime;

    public RoomChatlogEvent(ChatlogRoom room, Func<DateTime, DateTime> hotelTime)
    {
        _room = room;
        _hotelTime = hotelTime;
    }

    public ServerPacket Compose()
    {
        var packet = new ServerPacket(OutgoingOpcodes.Moderation.RoomChatlogEvent);
        ChatlogWireFormat.AppendRoom(packet, _room, _hotelTime);
        return packet;
    }
}

public sealed class UserChatlogEvent : IMessageEvent
{
    private readonly int _userId;
    private readonly string _username;
    private readonly IReadOnlyList<ChatlogRoom> _visits;
    private readonly Func<DateTime, DateTime> _hotelTime;

    public UserChatlogEvent(int userId, string username, IReadOnlyList<ChatlogRoom> visits, Func<DateTime, DateTime> hotelTime)
    {
        _userId = userId;
        _username = username;
        _visits = visits;
        _hotelTime = hotelTime;
    }

    public ServerPacket Compose()
    {
        var packet = new ServerPacket(OutgoingOpcodes.Moderation.UserChatlogEvent);
        packet.AppendInt32(_userId);
        packet.AppendString(_username);
        packet.AppendInt32(_visits.Count);
        foreach (ChatlogRoom visit in _visits)
        {
            ChatlogWireFormat.AppendRoom(packet, visit, _hotelTime);
        }

        return packet;
    }
}

public sealed class CfhChatlogEvent : IMessageEvent
{
    private readonly ModerationIssue _issue;
    private readonly ChatlogRoom _room;
    private readonly Func<DateTime, DateTime> _hotelTime;

    public CfhChatlogEvent(ModerationIssue issue, ChatlogRoom room, Func<DateTime, DateTime> hotelTime)
    {
        _issue = issue;
        _room = room;
        _hotelTime = hotelTime;
    }

    public ServerPacket Compose()
    {
        var packet = new ServerPacket(OutgoingOpcodes.Moderation.CfhChatlogEvent);
        packet.AppendInt32(_issue.Id);
        packet.AppendInt32(_issue.ReporterId);
        packet.AppendInt32(_issue.ReportedId ?? 0);
        packet.AppendInt32(_issue.Id);
        ChatlogWireFormat.AppendRoom(packet, _room, _hotelTime);
        return packet;
    }
}

public sealed class RoomVisitsEvent : IMessageEvent
{
    private readonly int _userId;
    private readonly string _username;
    private readonly IReadOnlyList<RoomVisit> _visits;
    private readonly Func<DateTime, DateTime> _hotelTime;

    public RoomVisitsEvent(int userId, string username, IReadOnlyList<RoomVisit> visits, Func<DateTime, DateTime> hotelTime)
    {
        _userId = userId;
        _username = username;
        _visits = visits;
        _hotelTime = hotelTime;
    }

    public ServerPacket Compose()
    {
        var packet = new ServerPacket(OutgoingOpcodes.Moderation.RoomVisitsEvent);
        packet.AppendInt32(_userId);
        packet.AppendString(_username);
        packet.AppendInt32(_visits.Count);
        foreach (RoomVisit visit in _visits)
        {
            DateTime entered = _hotelTime(visit.EnteredAt);
            packet.AppendBoolean(visit.IsPublic);
            packet.AppendInt32(visit.RoomId);
            packet.AppendString(visit.RoomName);
            packet.AppendInt32(entered.Hour);
            packet.AppendInt32(entered.Minute);
        }

        return packet;
    }
}

public sealed class ModeratorRoomInfoEvent : IMessageEvent
{
    private readonly PrivateRoomSummary _room;
    private readonly int _users;
    private readonly bool _ownerInRoom;
    private readonly RoomEvent? _event;

    public ModeratorRoomInfoEvent(PrivateRoomSummary room, int users, bool ownerInRoom, RoomEvent? roomEvent)
    {
        _room = room;
        _users = users;
        _ownerInRoom = ownerInRoom;
        _event = roomEvent;
    }

    public ServerPacket Compose()
    {
        var packet = new ServerPacket(OutgoingOpcodes.Moderation.ModeratorRoomInfoEvent);
        packet.AppendInt32(_room.Id);
        packet.AppendInt32(_users);
        packet.AppendBoolean(_ownerInRoom);
        packet.AppendInt32(_room.OwnerId ?? 0);
        packet.AppendString(_room.OwnerUsername);
        AppendNamed(packet, true, _room.Label, _room.Description, _room.Tags);
        AppendNamed(packet, _event is not null, _event?.Name ?? string.Empty, _event?.Description ?? string.Empty, _event?.Tags ?? []);
        return packet;
    }

    private static void AppendNamed(ServerPacket packet, bool exists, string name, string description, IReadOnlyList<string> tags)
    {
        packet.AppendBoolean(exists);
        if (!exists)
        {
            return;
        }

        packet.AppendString(name);
        packet.AppendString(description);
        packet.AppendInt32(tags.Count);
        foreach (string tag in tags)
        {
            packet.AppendString(tag);
        }
    }
}

public sealed class ModeratorActionResultMessageEvent : IMessageEvent
{
    private readonly int _userId;
    private readonly bool _success;

    public ModeratorActionResultMessageEvent(int userId, bool success)
    {
        _userId = userId;
        _success = success;
    }

    public ServerPacket Compose()
    {
        var packet = new ServerPacket(OutgoingOpcodes.Moderation.ModeratorActionResultMessageEvent);
        packet.AppendInt32(_userId);
        packet.AppendBoolean(_success);
        return packet;
    }
}

internal static class ChatlogWireFormat
{
    public static void AppendRoom(ServerPacket packet, ChatlogRoom room, Func<DateTime, DateTime> hotelTime)
    {
        packet.AppendBoolean(room.IsPublic);
        packet.AppendInt32(room.RoomId);
        packet.AppendString(room.RoomName);
        packet.AppendInt32(room.Lines.Count);
        foreach (ChatlogLine line in room.Lines)
        {
            DateTime said = hotelTime(line.SaidAt);
            packet.AppendInt32(said.Hour);
            packet.AppendInt32(said.Minute);
            packet.AppendInt32(line.UserId);
            packet.AppendString(line.Username);
            packet.AppendString(line.Message);
        }
    }
}
