using DragonEmu.Domain;

namespace DragonEmu.Protocol.Messages.Outgoing.Pets;

internal static class PetData
{
    public static void Append(ServerPacket packet, int id, string name, string figure, int type)
    {
        packet.AppendInt32(id);
        packet.AppendString(name);
        packet.AppendString(figure);
        packet.AppendInt32(type);
    }

    public static void Append(ServerPacket packet, PetRecord pet) => Append(packet, pet.Id, pet.Name, pet.Figure, pet.Type);
}
