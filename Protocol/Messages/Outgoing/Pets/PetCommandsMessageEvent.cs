using DragonEmu.Domain;

namespace DragonEmu.Protocol.Messages.Outgoing.Pets;

public sealed class PetCommandsMessageEvent : IMessageEvent
{
    private readonly int _webId;
    private readonly IReadOnlyList<int> _commandIds;

    public PetCommandsMessageEvent(int webId, IReadOnlyList<int> commandIds)
    {
        _webId = webId;
        _commandIds = commandIds;
    }

    public ServerPacket Compose()
    {
        var packet = new ServerPacket(OutgoingOpcodes.RoomPets.PetCommandsMessageEvent);
        packet.AppendInt32(_webId);
        packet.AppendInt32(_commandIds.Count);
        foreach (int commandId in _commandIds)
        {
            packet.AppendInt32(commandId);
        }

        return packet;
    }
}
