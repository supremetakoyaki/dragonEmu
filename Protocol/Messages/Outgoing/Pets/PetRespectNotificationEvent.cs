using DragonEmu.Domain;

namespace DragonEmu.Protocol.Messages.Outgoing.Pets;

public sealed class PetRespectNotificationEvent : IMessageEvent
{
    private readonly int _respect;
    private readonly int _ownerId;
    private readonly int _webId;
    private readonly string _name;
    private readonly string _figure;
    private readonly int _type;

    public PetRespectNotificationEvent(int respect, int ownerId, int webId, string name, string figure, int type)
    {
        _respect = respect;
        _ownerId = ownerId;
        _webId = webId;
        _name = name;
        _figure = figure;
        _type = type;
    }

    public ServerPacket Compose()
    {
        var packet = new ServerPacket(OutgoingOpcodes.Users.PetRespectNotificationEvent);
        packet.AppendInt32(_respect);
        packet.AppendInt32(_ownerId);
        PetData.Append(packet, _webId, _name, _figure, _type);
        return packet;
    }
}
