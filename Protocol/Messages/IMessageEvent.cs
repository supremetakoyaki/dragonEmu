namespace DragonEmu.Protocol.Messages;

public interface IMessageEvent
{
    ServerPacket Compose();
}
