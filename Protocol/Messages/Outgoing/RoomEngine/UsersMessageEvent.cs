using System.Globalization;
using DragonEmu.Domain;
using DragonEmu.Game.Rooms;

namespace DragonEmu.Protocol.Messages.Outgoing.RoomEngine;

public sealed class UsersMessageEvent : IMessageEvent
{
    private const int DegreesPerRotation = 45;
    private const int UserMemberType = 1;
    private const int PetMemberType = 2;
    private const int BotMemberType = 3;

    private readonly IReadOnlyList<RoomActor> _actors;

    public UsersMessageEvent(IReadOnlyList<RoomActor> actors)
    {
        _actors = actors;
    }

    public ServerPacket Compose()
    {
        var packet = new ServerPacket(OutgoingOpcodes.RoomEngine.UsersMessageEvent);
        packet.AppendInt32(_actors.Count);

        foreach (RoomActor actor in _actors)
        {
            packet.AppendUInt32((uint)(actor.Pet is { } pet ? PetWebId.Of(pet.PetId) : actor.AccountId));
            packet.AppendString(actor.Username);
            packet.AppendString(actor.Motto);
            packet.AppendString(actor.Figure);
            packet.AppendInt32(actor.VirtualId);
            packet.AppendInt32(actor.X);
            packet.AppendInt32(actor.Y);
            packet.AppendString(actor.Z.ToString(CultureInfo.InvariantCulture));
            packet.AppendInt32(actor.RotBody * DegreesPerRotation);
            if (actor.Type == ActorType.Bot)
            {
                packet.AppendInt32(BotMemberType);
                continue;
            }

            if (actor.Pet is { } brain)
            {
                packet.AppendInt32(PetMemberType);
                packet.AppendInt32(brain.Breed);
                continue;
            }

            packet.AppendInt32(UserMemberType);
            packet.AppendString(char.ToLowerInvariant(actor.Gender).ToString());
            packet.AppendInt32(-1);
            packet.AppendInt32(actor.FavouriteGroup?.GroupId ?? -1);
            packet.AppendInt32(-1);
            packet.AppendString(actor.Swimsuit ?? string.Empty);
        }

        return packet;
    }
}
