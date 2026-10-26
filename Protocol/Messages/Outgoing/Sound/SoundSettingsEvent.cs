namespace DragonEmu.Protocol.Messages.Outgoing.Sound;

public sealed class SoundSettingsEvent : IMessageEvent
{
    private readonly int _volume;

    public SoundSettingsEvent(int volume)
    {
        _volume = volume;
    }

    public ServerPacket Compose()
    {
        var packet = new ServerPacket(OutgoingOpcodes.Sound.SoundSettingsEvent);
        packet.AppendInt32(_volume);
        return packet;
    }
}
