using DragonEmu.Domain;
using DragonEmu.Persistence;
using DragonEmu.Protocol.Messages.Outgoing.InventoryPurse;
using DragonEmu.Protocol.Messages.Outgoing.Notifications;

namespace DragonEmu.Protocol.Messages.Incoming.InventoryPurse;

public sealed class GetCreditsInfoComposer : IMessageComposer
{
    private readonly ICurrencyRepository _currencyRepository;

    public GetCreditsInfoComposer(ICurrencyRepository currencyRepository)
    {
        _currencyRepository = currencyRepository;
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

        long credits = await _currencyRepository.GetBalanceAsync(account.Id, CurrencyType.Credits, cancellationToken);
        long activityPoints = await _currencyRepository.GetBalanceAsync(account.Id, CurrencyType.ActivityPoints, cancellationToken);

        session.Send(new CreditBalanceEvent(credits).Compose());
        session.Send(new HabboActivityPointNotificationMessageEvent(activityPoints).Compose());
    }
}
