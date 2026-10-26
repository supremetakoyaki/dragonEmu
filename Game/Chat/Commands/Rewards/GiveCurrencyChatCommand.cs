using DragonEmu.Domain;
using DragonEmu.Game.Rooms;
using DragonEmu.Gateway;
using DragonEmu.Persistence;
using DragonEmu.Protocol;
using DragonEmu.Protocol.Messages;
using DragonEmu.Protocol.Messages.Outgoing.InventoryPurse;
using DragonEmu.Protocol.Messages.Outgoing.Moderation;
using DragonEmu.Protocol.Messages.Outgoing.Notifications;
using DragonEmu.Game.Settings;

namespace DragonEmu.Game.Chat.Commands;

public abstract class GiveCurrencyChatCommand : IChatCommand
{
    private readonly IUserRepository _userRepository;
    private readonly ICurrencyRepository _currencyRepository;
    private readonly ClientSessionManager _sessionManager;
    private readonly MessageTexts _texts;

    protected GiveCurrencyChatCommand(IUserRepository userRepository, ICurrencyRepository currencyRepository, ClientSessionManager sessionManager, MessageTexts texts)
    {
        _texts = texts;
        _userRepository = userRepository;
        _currencyRepository = currencyRepository;
        _sessionManager = sessionManager;
    }

    public abstract string Name { get; }

    public string RequiredFuse => RoomRightsService.AdministratorAccessFuse;

    protected abstract int Currency { get; }

    protected abstract string CurrencyName { get; }

    protected abstract ServerPacket BalanceUpdate(long balance, int amount);

    public async ValueTask ExecuteAsync(IGameSession session, string[] args, CancellationToken cancellationToken)
    {
        if (args.Length != 2 || !int.TryParse(args[1], out int amount) || amount <= 0)
        {
            session.Send(new ModMessageEvent(_texts.Format(MessageKeys.UsageGiveCurrency, ("command", Name))).Compose());
            return;
        }

        if (await _userRepository.GetByUsernameAsync(args[0], cancellationToken) is not { } receiver)
        {
            session.Send(new ModMessageEvent(_texts.Format(MessageKeys.UserNotFound, ("username", args[0]))).Compose());
            return;
        }

        long balance = await _currencyRepository.AddBalanceAsync(receiver.Id, Currency, amount, cancellationToken);
        if (_sessionManager.TryGetByUserId(receiver.Id, out ClientSession? online))
        {
            online!.EnqueueSend(BalanceUpdate(balance, amount));
        }

        session.Send(new ModMessageEvent(_texts.Format(MessageKeys.CurrencyGiven, ("username", receiver.Username), ("amount", amount), ("currency", CurrencyName), ("balance", balance))).Compose());
    }
}

public sealed class GiveCreditsChatCommand : GiveCurrencyChatCommand
{
    public GiveCreditsChatCommand(IUserRepository userRepository, ICurrencyRepository currencyRepository, ClientSessionManager sessionManager, MessageTexts texts)
        : base(userRepository, currencyRepository, sessionManager, texts)
    {
    }

    public override string Name => "givecredits";

    protected override int Currency => CurrencyType.Credits;

    protected override string CurrencyName => "credits";

    protected override ServerPacket BalanceUpdate(long balance, int amount) => new CreditBalanceEvent(balance).Compose();
}

public sealed class GivePixelsChatCommand : GiveCurrencyChatCommand
{
    public GivePixelsChatCommand(IUserRepository userRepository, ICurrencyRepository currencyRepository, ClientSessionManager sessionManager, MessageTexts texts)
        : base(userRepository, currencyRepository, sessionManager, texts)
    {
    }

    public override string Name => "givepixels";

    protected override int Currency => CurrencyType.ActivityPoints;

    protected override string CurrencyName => "pixels";

    protected override ServerPacket BalanceUpdate(long balance, int amount) =>
        new HabboActivityPointNotificationMessageEvent(balance, amount).Compose();
}
