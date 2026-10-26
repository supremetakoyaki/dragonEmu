using DragonEmu.Domain;
using DragonEmu.Persistence;
using DragonEmu.Protocol.Messages.Outgoing.InventoryBadges;

namespace DragonEmu.Protocol.Messages.Incoming.InventoryBadges;

public sealed class GetBadgesComposer : IMessageComposer
{
    private readonly IUserBadgeRepository _userBadgeRepository;

    public GetBadgesComposer(IUserBadgeRepository userBadgeRepository)
    {
        _userBadgeRepository = userBadgeRepository;
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

        IReadOnlyList<UserBadge> badges = await _userBadgeRepository.GetAllAsync(account.Id, cancellationToken);
        session.Send(BadgesEvent.For(badges).Compose());
    }
}
