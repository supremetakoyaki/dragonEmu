using System.Globalization;
using DragonEmu.Game.Rooms;

namespace DragonEmu.Protocol.Messages.Outgoing.RoomEngine;

public sealed class RestingRiderSlideMessageEvent : IMessageEvent
{
    private readonly IReadOnlyList<RollerSlide> _slides;

    public RestingRiderSlideMessageEvent(IReadOnlyList<RollerSlide> slides)
    {
        _slides = slides.Where(slide => slide.Rider is { IsResting: true }).ToList();
    }

    public bool IsEmpty => _slides.Count == 0;

    public ServerPacket Compose()
    {
        var packet = new ServerPacket(OutgoingOpcodes.RoomEngine.UserUpdateMessageEvent);
        packet.AppendInt32(_slides.Count);

        foreach (RollerSlide slide in _slides)
        {
            SlidingRider rider = slide.Rider!.Value;
            (string posture, double postureLift) = rider.Posture == ActorPosture.Sit
                ? ("sit", rider.PostureHeight + 0.02)
                : ("lay", Math.Abs(rider.PostureHeight));

            packet.AppendInt32(rider.VirtualId);
            packet.AppendInt32(slide.From.X);
            packet.AppendInt32(slide.From.Y);
            packet.AppendString(rider.FromZ.ToString(CultureInfo.InvariantCulture));
            packet.AppendInt32(rider.RotHead);
            packet.AppendInt32(rider.RotBody);
            packet.AppendString(string.Create(
                CultureInfo.InvariantCulture,
                $"/mv {slide.To.X},{slide.To.Y},{rider.ToZ + postureLift:0.0###}/{posture} {rider.PostureHeight:0.0###}//"));
        }

        return packet;
    }
}
