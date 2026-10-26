using DragonEmu.Game.Catalog;
using DragonEmu.Protocol.Messages.Outgoing.Recycler;

namespace DragonEmu.Protocol.Messages.Incoming.Recycler;

public sealed class GetRecyclerPrizesMessageComposer : IMessageComposer
{
    private readonly RecyclerPrizeCache _prizes;

    public GetRecyclerPrizesMessageComposer(RecyclerPrizeCache prizes)
    {
        _prizes = prizes;
    }

    public void Parse(ClientPacket packet)
    {
    }

    public ValueTask HandleAsync(IGameSession session, CancellationToken cancellationToken)
    {
        if (session.AuthenticatedUser is not null)
        {
            session.Send(new RecyclerPrizesMessageEvent(_prizes.Levels).Compose());
        }

        return ValueTask.CompletedTask;
    }
}
