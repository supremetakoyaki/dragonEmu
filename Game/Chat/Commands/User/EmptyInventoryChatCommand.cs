using DragonEmu.Domain;
using DragonEmu.Game.Trading;
using DragonEmu.Persistence;
using DragonEmu.Protocol.Messages;
using DragonEmu.Protocol.Messages.Outgoing.InventoryFurni;

namespace DragonEmu.Game.Chat.Commands;

public sealed class EmptyInventoryChatCommand : IChatCommand
{
    private readonly IUserItemRepository _userItems;
    private readonly TradingService _trading;

    public EmptyInventoryChatCommand(IUserItemRepository userItems, TradingService trading)
    {
        _userItems = userItems;
        _trading = trading;
    }

    public string Name => "empty";

    public string? RequiredFuse => null;

    public async ValueTask ExecuteAsync(IGameSession session, string[] args, CancellationToken cancellationToken)
    {
        if (session.AuthenticatedUser is not { } account)
        {
            return;
        }

        IReadOnlyList<int> deleted = await _userItems.DeleteInventoryAsync(account.Id, itemId => _trading.IsOffered(session, itemId), cancellationToken);
        if (deleted.Count > 0)
        {
            session.Send(new FurniListUpdateEvent().Compose());
        }
    }
}
