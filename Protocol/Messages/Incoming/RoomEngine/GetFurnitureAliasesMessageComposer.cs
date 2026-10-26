using DragonEmu.Game.Furniture;
using DragonEmu.Protocol.Messages.Outgoing.RoomEngine;

namespace DragonEmu.Protocol.Messages.Incoming.RoomEngine;

public sealed class GetFurnitureAliasesMessageComposer : IMessageComposer
{
    private readonly FurnitureAliasCache _furnitureAliasCache;

    public GetFurnitureAliasesMessageComposer(FurnitureAliasCache furnitureAliasCache)
    {
        _furnitureAliasCache = furnitureAliasCache;
    }

    public void Parse(ClientPacket packet)
    {
    }

    public ValueTask HandleAsync(IGameSession session, CancellationToken cancellationToken)
    {
        if (session.LoadingRoomId is null && session.CurrentRoomId is null)
        {
            return ValueTask.CompletedTask;
        }

        session.Send(new FurnitureAliasesMessageEvent(_furnitureAliasCache.Aliases).Compose());
        return ValueTask.CompletedTask;
    }
}
