namespace DragonEmu.Protocol.Messages.Outgoing.InventoryAchievements;

public sealed class AchievementsScoreEvent : IMessageEvent
{
    private readonly int _score;

    public AchievementsScoreEvent(int score)
    {
        _score = score;
    }

    public ServerPacket Compose()
    {
        var packet = new ServerPacket(OutgoingOpcodes.InventoryAchievements.AchievementsScoreEvent);
        packet.AppendInt32(_score);
        return packet;
    }
}
