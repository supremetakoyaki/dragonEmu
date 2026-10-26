using DragonEmu.Domain;

namespace DragonEmu.Protocol.Messages.Outgoing.Handshake;

public sealed class UserObjectEvent : IMessageEvent
{
    private readonly UserAccount _account;
    private readonly int _respectReceived;
    private readonly int _respectsLeft;
    private readonly int _petRespectsLeft;

    public UserObjectEvent(UserAccount account, int respectReceived, int respectsLeft, int petRespectsLeft)
    {
        _account = account;
        _respectReceived = respectReceived;
        _respectsLeft = respectsLeft;
        _petRespectsLeft = petRespectsLeft;
    }

    public ServerPacket Compose()
    {
        var packet = new ServerPacket(OutgoingOpcodes.Handshake.UserObjectEvent);
        packet.AppendString(_account.Id.ToString());
        packet.AppendString(_account.Username);
        packet.AppendString(_account.Figure);
        packet.AppendString(char.ToUpperInvariant(_account.Gender).ToString());
        packet.AppendString(_account.Motto);
        packet.AppendString(string.Empty);
        packet.AppendInt32(0);
        packet.AppendString(string.Empty);
        packet.AppendInt32(0);
        packet.AppendInt32(0);
        packet.AppendInt32(_respectReceived);
        packet.AppendInt32(_respectsLeft);
        packet.AppendInt32(_petRespectsLeft);
        return packet;
    }
}
