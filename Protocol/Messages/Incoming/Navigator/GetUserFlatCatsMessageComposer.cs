using DragonEmu.Domain;
using DragonEmu.Persistence;
using DragonEmu.Protocol.Messages.Outgoing.Navigator;

namespace DragonEmu.Protocol.Messages.Incoming.Navigator;

public sealed class GetUserFlatCatsMessageComposer : IMessageComposer
{
    private readonly IRoomCategoryRepository _roomCategoryRepository;

    public GetUserFlatCatsMessageComposer(IRoomCategoryRepository roomCategoryRepository)
    {
        _roomCategoryRepository = roomCategoryRepository;
    }

    public void Parse(ClientPacket packet)
    {
    }

    public async ValueTask HandleAsync(IGameSession session, CancellationToken cancellationToken)
    {
        if (session.AuthenticatedUser is not { } account)
        {
            return;
        }

        IReadOnlyList<RoomCategory> categories = await _roomCategoryRepository.GetAllAsync(cancellationToken);
        var flatCategories = categories
            .Select(category => new FlatCategory(category.Id, category.Label, account.Rank >= category.RequiredRights))
            .ToList();

        session.Send(new UserFlatCatsEvent(flatCategories).Compose());
    }
}
