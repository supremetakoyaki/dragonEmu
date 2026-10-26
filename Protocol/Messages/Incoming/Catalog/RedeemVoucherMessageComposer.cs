using System.Globalization;
using DragonEmu.Game.Fuses;
using DragonEmu.Domain;
using DragonEmu.Game.Furniture;
using DragonEmu.Game.Subscriptions;
using DragonEmu.Game.Vouchers;
using DragonEmu.Persistence;
using DragonEmu.Protocol.Messages.Outgoing.Catalog;
using DragonEmu.Protocol.Messages.Outgoing.Handshake;
using DragonEmu.Protocol.Messages.Outgoing.InventoryFurni;
using DragonEmu.Protocol.Messages.Outgoing.InventoryPurse;
using DragonEmu.Protocol.Messages.Outgoing.Users;

namespace DragonEmu.Protocol.Messages.Incoming.Catalog;

public sealed class RedeemVoucherMessageComposer : IMessageComposer
{
    private const string CreditType = "credit";
    private const string FurniType = "furni";
    private const string ClubType = "club";

    private readonly IVoucherRepository _voucherRepository;
    private readonly VoucherAttemptGuard _attemptGuard;
    private readonly ICurrencyRepository _currencyRepository;
    private readonly IUserItemRepository _userItemRepository;
    private readonly FurnitureDefinitionCache _furnitureDefinitions;
    private readonly SubscriptionManager _subscriptionManager;
    private readonly FuseService _fuses;
    private readonly TeleportPairGrantService _teleportPairGrantService;

    private string _code = string.Empty;

    public RedeemVoucherMessageComposer(
        IVoucherRepository voucherRepository, VoucherAttemptGuard attemptGuard, ICurrencyRepository currencyRepository,
        IUserItemRepository userItemRepository, FurnitureDefinitionCache furnitureDefinitions,
        SubscriptionManager subscriptionManager, FuseService fuses, TeleportPairGrantService teleportPairGrantService)
    {
        _voucherRepository = voucherRepository;
        _attemptGuard = attemptGuard;
        _currencyRepository = currencyRepository;
        _userItemRepository = userItemRepository;
        _furnitureDefinitions = furnitureDefinitions;
        _subscriptionManager = subscriptionManager;
        _fuses = fuses;
        _teleportPairGrantService = teleportPairGrantService;
    }

    public void Parse(ClientPacket packet)
    {
        _code = packet.PopString();
    }

    public async ValueTask HandleAsync(IGameSession session, CancellationToken cancellationToken)
    {
        if (session.AuthenticatedUser is not { } account)
        {
            return;
        }

        if (_attemptGuard.IsInCooldown(account.Id))
        {
            session.Send(new VoucherRedeemErrorMessageEvent(VoucherRedeemErrorMessageEvent.InvalidFormat).Compose());
            return;
        }

        if (!VoucherCodeValidator.IsValidFormat(_code))
        {
            _attemptGuard.RecordFailure(account.Id);
            session.Send(new VoucherRedeemErrorMessageEvent(VoucherRedeemErrorMessageEvent.InvalidFormat).Compose());
            return;
        }

        VoucherCode? voucher = await _voucherRepository.GetActiveByHashAsync(VoucherHasher.Hash(_code), cancellationToken);
        if (voucher is null)
        {
            _attemptGuard.RecordFailure(account.Id);
            session.Send(new VoucherRedeemErrorMessageEvent(VoucherRedeemErrorMessageEvent.NotRedeemable).Compose());
            return;
        }

        int creditAmount = 0;
        double clubDays = 0;
        List<FurnitureDefinition> furniture = [];

        switch (voucher.RedeemableType)
        {
            case CreditType:
                if (!int.TryParse(voucher.RedeemableData, NumberStyles.Integer, CultureInfo.InvariantCulture, out creditAmount) || creditAmount <= 0)
                {
                    _attemptGuard.RecordFailure(account.Id);
                    session.Send(new VoucherRedeemErrorMessageEvent(VoucherRedeemErrorMessageEvent.NotRedeemable).Compose());
                    return;
                }

                break;

            case FurniType:
                string[] classnames = voucher.RedeemableData.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
                if (classnames.Length == 0)
                {
                    _attemptGuard.RecordFailure(account.Id);
                    session.Send(new VoucherRedeemErrorMessageEvent(VoucherRedeemErrorMessageEvent.NotRedeemable).Compose());
                    return;
                }

                foreach (string classname in classnames)
                {
                    FurnitureDefinition? definition = _furnitureDefinitions.GetByClassname(classname);
                    if (definition is null)
                    {
                        _attemptGuard.RecordFailure(account.Id);
                        session.Send(new VoucherRedeemErrorMessageEvent(VoucherRedeemErrorMessageEvent.NotRedeemable).Compose());
                        return;
                    }

                    furniture.Add(definition);
                }

                break;

            case ClubType:
                if (!double.TryParse(voucher.RedeemableData, NumberStyles.Float, CultureInfo.InvariantCulture, out clubDays) || clubDays <= 0)
                {
                    _attemptGuard.RecordFailure(account.Id);
                    session.Send(new VoucherRedeemErrorMessageEvent(VoucherRedeemErrorMessageEvent.NotRedeemable).Compose());
                    return;
                }

                break;

            default:
                _attemptGuard.RecordFailure(account.Id);
                session.Send(new VoucherRedeemErrorMessageEvent(VoucherRedeemErrorMessageEvent.NotRedeemable).Compose());
                return;
        }

        if (!await _voucherRepository.TryClaimAsync(voucher.Id, account.Id, session.RemoteAddress, cancellationToken))
        {
            _attemptGuard.RecordFailure(account.Id);
            session.Send(new VoucherRedeemErrorMessageEvent(VoucherRedeemErrorMessageEvent.NotRedeemable).Compose());
            return;
        }

        _attemptGuard.RecordSuccess(account.Id);

        switch (voucher.RedeemableType)
        {
            case CreditType:
                long credits = await _currencyRepository.GetBalanceAsync(account.Id, CurrencyType.Credits, cancellationToken);
                await _currencyRepository.SetBalanceAsync(account.Id, CurrencyType.Credits, credits + creditAmount, cancellationToken);
                session.Send(new CreditBalanceEvent(credits + creditAmount).Compose());
                session.Send(new VoucherRedeemOkMessageEvent().Compose());
                break;

            case FurniType:
                foreach (FurnitureDefinition definition in furniture)
                {
                    IReadOnlyList<int> itemIds = await _teleportPairGrantService.GrantAsync(
                        account.Id, definition, extraData: null, seen: true, count: 1, cancellationToken);

                    foreach (UserItem item in await _userItemRepository.GetByIdsAsync([.. itemIds], cancellationToken))
                    {
                        session.Send(new FurniListInsertEvent(item).Compose());
                    }
                }

                session.Send(new VoucherRedeemOkMessageEvent(
                    productDescription: string.Join(", ", furniture.Select(f => f.LocalizationDesc)),
                    productName: string.Join(", ", furniture.Select(f => f.LocalizationName))).Compose());
                break;

            case ClubType:
                SubscriptionGrantResult grant = await _subscriptionManager.GrantDaysAsync(account.Id, clubDays, cancellationToken);
                session.Send(ScrSendUserInfoEvent.HabboClubJustSubscribed(grant.ClubExpiresAt).Compose());
                await _fuses.SendHeldAsync(session, account, cancellationToken);

                session.Send(new VoucherRedeemOkMessageEvent().Compose());
                break;
        }
    }
}
