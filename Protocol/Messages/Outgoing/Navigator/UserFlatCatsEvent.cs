namespace DragonEmu.Protocol.Messages.Outgoing.Navigator;

public sealed record FlatCategory(int Id, string Caption, bool Visible);

public sealed class UserFlatCatsEvent : IMessageEvent
{
    private readonly IReadOnlyList<FlatCategory> _categories;

    public UserFlatCatsEvent(IReadOnlyList<FlatCategory> categories)
    {
        _categories = categories;
    }

    public ServerPacket Compose()
    {
        var packet = new ServerPacket(OutgoingOpcodes.Navigator.UserFlatCatsEvent);
        packet.AppendInt32(_categories.Count);

        foreach (FlatCategory category in _categories)
        {
            packet.AppendInt32(category.Id);
            packet.AppendString(category.Caption);
            packet.AppendBoolean(category.Visible);
        }

        return packet;
    }
}
