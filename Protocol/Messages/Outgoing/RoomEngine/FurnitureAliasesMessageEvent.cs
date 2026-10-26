using DragonEmu.Domain;

namespace DragonEmu.Protocol.Messages.Outgoing.RoomEngine;

public sealed class FurnitureAliasesMessageEvent : IMessageEvent
{
    private readonly IReadOnlyList<FurnitureAlias> _aliases;

    public FurnitureAliasesMessageEvent(IReadOnlyList<FurnitureAlias> aliases)
    {
        _aliases = aliases;
    }

    public ServerPacket Compose()
    {
        var packet = new ServerPacket(OutgoingOpcodes.RoomEngine.FurnitureAliasesMessageEvent);
        packet.AppendInt32(_aliases.Count);
        foreach (FurnitureAlias alias in _aliases)
        {
            packet.AppendString(alias.Classname);
            packet.AppendString(alias.ClassnameAlias);
        }

        return packet;
    }
}
