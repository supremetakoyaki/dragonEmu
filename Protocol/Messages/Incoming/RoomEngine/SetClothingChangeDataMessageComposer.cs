namespace DragonEmu.Protocol.Messages.Incoming.RoomEngine;

public sealed class SetClothingChangeDataMessageComposer : IMessageComposer
{
    private int _objectId;
    private string _gender = string.Empty;
    private string _figure = string.Empty;

    public int ObjectId => _objectId;

    public string Gender => _gender;

    public string Figure => _figure;

    public void Parse(ClientPacket packet)
    {
        _objectId = packet.PopInt32();
        _gender = packet.PopString();
        _figure = packet.PopString();
    }

    public ValueTask HandleAsync(IGameSession session, CancellationToken cancellationToken) => ValueTask.CompletedTask;
}
