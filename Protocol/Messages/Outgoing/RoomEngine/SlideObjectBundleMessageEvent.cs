using System.Globalization;
using DragonEmu.Game.Rooms;

namespace DragonEmu.Protocol.Messages.Outgoing.RoomEngine;

public sealed class SlideObjectBundleMessageEvent : IMessageEvent
{
    private const int NoUser = 0;
    private const int StandingUser = 2;

    private readonly RollerSlide _slide;

    public SlideObjectBundleMessageEvent(RollerSlide slide)
    {
        _slide = slide;
    }

    public ServerPacket Compose()
    {
        var packet = new ServerPacket(OutgoingOpcodes.RoomEngine.SlideObjectBundleMessageEvent);
        packet.AppendInt32(_slide.From.X);
        packet.AppendInt32(_slide.From.Y);
        packet.AppendInt32(_slide.To.X);
        packet.AppendInt32(_slide.To.Y);

        packet.AppendInt32(_slide.Items.Count);
        foreach (SlidingItem item in _slide.Items)
        {
            packet.AppendInt32(item.ItemId);
            packet.AppendString(Height(item.FromZ));
            packet.AppendString(Height(item.ToZ));
        }

        packet.AppendInt32(_slide.RollerId);

        if (_slide.Rider is { IsResting: false } rider)
        {
            packet.AppendInt32(StandingUser);
            packet.AppendInt32(rider.VirtualId);
            packet.AppendString(Height(rider.FromZ));
            packet.AppendString(Height(rider.ToZ));
        }
        else
        {
            packet.AppendInt32(NoUser);
        }

        return packet;
    }

    private static string Height(double z) => z.ToString("0.0###", CultureInfo.InvariantCulture);
}
