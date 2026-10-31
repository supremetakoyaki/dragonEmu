using System.Globalization;
using DragonEmu.Game.Rooms;

namespace DragonEmu.Protocol.Messages.Outgoing.RoomEngine;

public sealed class UserUpdateMessageEvent : IMessageEvent
{
    private readonly IReadOnlyList<RoomActor> _actors;

    public UserUpdateMessageEvent(IReadOnlyList<RoomActor> actors)
    {
        _actors = actors;
    }

    public ServerPacket Compose()
    {
        var packet = new ServerPacket(OutgoingOpcodes.RoomEngine.UserUpdateMessageEvent);
        packet.AppendInt32(_actors.Count);

        foreach (RoomActor actor in _actors)
        {
            lock (actor.Lock)
            {
                packet.AppendInt32(actor.VirtualId);
                packet.AppendInt32(actor.X);
                packet.AppendInt32(actor.Y);
                packet.AppendString(actor.ShownZ.ToString(CultureInfo.InvariantCulture));
                packet.AppendInt32(actor.RotHead);
                packet.AppendInt32(actor.RotBody);

                var tokens = new List<string>();
                if (actor.FlatControl is { } flatControl)
                {
                    tokens.Add(flatControl.Length > 0 ? $"flatctrl {flatControl}" : "flatctrl");
                }

                if (actor.PendingStep is { } pending)
                {
                    tokens.Add(string.Create(CultureInfo.InvariantCulture, $"mv {pending.X},{pending.Y},{pending.Z}"));
                }
                else if (actor.PetPosture is { } pose)
                {
                    tokens.Add(pose is "sit" or "lay" ? $"{pose} 0.0" : pose);
                }
                else
                {
                    switch (actor.Posture)
                    {
                        case ActorPosture.Sit:
                            tokens.Add(string.Create(CultureInfo.InvariantCulture, $"sit {actor.PostureHeight:0.0###}"));
                            break;
                        case ActorPosture.Lay:
                            tokens.Add(string.Create(CultureInfo.InvariantCulture, $"lay {actor.PostureHeight:0.0###}"));
                            break;
                    }
                }

                if (actor.IsSwimming)
                {
                    tokens.Add("swim");
                }

                if (actor.PetGesture is { } gesture)
                {
                    tokens.Add($"gst {gesture}");
                }

                if (actor.PendingSignId is { } signId)
                {
                    tokens.Add(string.Create(CultureInfo.InvariantCulture, $"sign {signId}"));
                    actor.PendingSignId = null;
                }

                packet.AppendString(tokens.Count > 0 ? $"/{string.Join('/', tokens)}//" : "//");
            }
        }

        return packet;
    }
}
