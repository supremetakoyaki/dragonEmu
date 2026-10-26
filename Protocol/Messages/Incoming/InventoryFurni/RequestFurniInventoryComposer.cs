using DragonEmu.Domain;
using DragonEmu.Persistence;
using DragonEmu.Protocol.Messages.Outgoing.InventoryFurni;

namespace DragonEmu.Protocol.Messages.Incoming.InventoryFurni;

public sealed class RequestFurniInventoryComposer : IMessageComposer
{
    private readonly IUserItemRepository _userItemRepository;

    public RequestFurniInventoryComposer(IUserItemRepository userItemRepository)
    {
        _userItemRepository = userItemRepository;
    }

    public void Parse(ClientPacket packet)
    {
        packet.PopInt32();
        packet.PopInt32();
    }

    public async ValueTask HandleAsync(IGameSession session, CancellationToken cancellationToken)
    {
        if (session.AuthenticatedUser is not { } account)
        {
            return;
        }

        IReadOnlyList<UserItem> items = await _userItemRepository.GetInventoryAsync(account.Id, cancellationToken);
        session.Send(new FurniListEvent(items).Compose());
    }
}
