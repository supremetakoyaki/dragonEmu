namespace DragonEmu.Protocol.Messages.Outgoing.RoomEngine;

public enum PlaceObjectErrorReason
{
    Generic = 1,
    NotEnoughSpace = 11,
    ItemNotFound = 2,
    MaxStickies = 12,
    MaxFurniture = 20,
    MaxPets = 21,
    MaxQueueTiles = 22,

    MaxSoundFurni = 23,
}

public sealed class PlaceObjectErrorMessageEvent : IMessageEvent
{
    private readonly PlaceObjectErrorReason _reason;

    public PlaceObjectErrorMessageEvent(PlaceObjectErrorReason reason)
    {
        _reason = reason;
    }

    public ServerPacket Compose()
    {
        var packet = new ServerPacket(OutgoingOpcodes.RoomEngine.PlaceObjectErrorMessageEvent);
        packet.AppendInt32((int)_reason);
        return packet;
    }
}
