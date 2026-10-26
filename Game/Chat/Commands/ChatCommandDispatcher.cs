using DragonEmu.Game.Fuses;
using DragonEmu.Domain;
using DragonEmu.Persistence;
using DragonEmu.Protocol.Messages;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace DragonEmu.Game.Chat.Commands;

public enum ChatCommandSection
{
    Server,
    Hotel,
    Moderation,
    Rewards,
    Room,
    User,
}

public sealed class ChatCommandDispatcher : IChatCommandDispatcher
{
    private sealed record Registration(ChatCommandSection? Section, string[] Names, string Usage, Func<IServiceProvider, IChatCommand> Create)
    {
        public string HelpLine => string.Join(" / ", Names.Select(name => ":" + name)) + (Usage.Length > 0 ? " " + Usage : string.Empty);
    }

    private static readonly Registration[] Commands =
    [
        new(ChatCommandSection.Server, ["update_catalog"], "", Create<UpdateCatalogChatCommand>),
        new(ChatCommandSection.Server, ["info"], "", Create<InfoChatCommand>),
        new(ChatCommandSection.Server, ["update_bots"], "", Create<UpdateBotsChatCommand>),
        new(ChatCommandSection.Server, ["update_public_rooms"], "", Create<UpdatePublicRoomsChatCommand>),
        new(ChatCommandSection.Server, ["shutdown"], "[confirm] [HH:mm]", Create<ShutdownChatCommand>),
        new(ChatCommandSection.Hotel, ["close_infobus"], "", Create<CloseInfobusChatCommand>),
        new(ChatCommandSection.Hotel, ["open_infobus"], "", Create<OpenInfobusChatCommand>),
        new(ChatCommandSection.Hotel, ["hotelalert", "ha"], "[message]", Create<HotelAlertChatCommand>),
        new(ChatCommandSection.Moderation, ["alert"], "[user] [message]", Create<AlertChatCommand>),
        new(ChatCommandSection.Moderation, ["roomalert"], "[message]", Create<RoomAlertChatCommand>),
        new(ChatCommandSection.Moderation, ["ban"], "[user] <hours> <reason>", Create<BanChatCommand>),
        new(ChatCommandSection.Moderation, ["superban"], "[user] <days> <reason>", Create<SuperbanChatCommand>),
        new(ChatCommandSection.Moderation, ["mute"], "[user] <hours>", Create<MuteChatCommand>),
        new(ChatCommandSection.Moderation, ["unmute"], "[user]", Create<UnmuteChatCommand>),
        new(ChatCommandSection.Moderation, ["roommute"], "<hours>", Create<RoomMuteChatCommand>),
        new(ChatCommandSection.Moderation, ["roomunmute"], "", Create<RoomUnmuteChatCommand>),
        new(ChatCommandSection.Rewards, ["credits", "givecredits"], "[user] [amount]", Create<GiveCreditsChatCommand>),
        new(ChatCommandSection.Rewards, ["pixels", "givepixels"], "[user] [amount]", Create<GivePixelsChatCommand>),
        new(ChatCommandSection.Rewards, ["givebadge"], "[user] [code]", Create<GiveBadgeChatCommand>),
        new(ChatCommandSection.Room, ["pos", "position"], "", Create<PositionChatCommand>),
        new(ChatCommandSection.Room, ["sit"], "", Create<SitChatCommand>),
        new(ChatCommandSection.Room, ["lay"], "", Create<LayChatCommand>),
        new(ChatCommandSection.Room, ["pickall"], "", Create<PickAllChatCommand>),
        new(ChatCommandSection.Room, ["teleport"], "[on/off] <z>", Create<TeleportChatCommand>),
        new(ChatCommandSection.Room, ["handitem"], "[id]", Create<HandItemChatCommand>),
        new(ChatCommandSection.Room, ["fx"], "[id]", Create<EffectChatCommand>),
        new(ChatCommandSection.Room, ["roller_speed"], "[1-4]", Create<RollerSpeedChatCommand>),
        new(ChatCommandSection.Room, ["kick"], "[user]", Create<KickChatCommand>),
        new(ChatCommandSection.Room, ["softkick"], "[user]", Create<SoftKickChatCommand>),
        new(ChatCommandSection.Room, ["roomkick"], "", Create<RoomKickChatCommand>),
        new(ChatCommandSection.Room, ["start_vote"], "[question]", Create<StartVoteChatCommand>),
        new(ChatCommandSection.Room, ["end_vote"], "", Create<EndVoteChatCommand>),
        new(ChatCommandSection.User, ["motto"], "<new motto>", Create<MottoChatCommand>),
        new(ChatCommandSection.User, ["changename"], "", Create<ChangeNameChatCommand>),
        new(null, ["commands"], "", Create<CommandsChatCommand>),
    ];

    private static readonly Dictionary<string, Registration> ByName = Commands
        .SelectMany(command => command.Names, (command, name) => (name, command))
        .ToDictionary(entry => entry.name, entry => entry.command, StringComparer.OrdinalIgnoreCase);

    private readonly IServiceProvider _serviceProvider;
    private readonly FuseService _fuses;
    private readonly ILogger<ChatCommandDispatcher> _logger;

    public ChatCommandDispatcher(IServiceProvider serviceProvider, FuseService fuses, ILogger<ChatCommandDispatcher> logger)
    {
        _serviceProvider = serviceProvider;
        _fuses = fuses;
        _logger = logger;
    }

    public static void EnsureEveryCommandCanBeMade(IServiceProvider serviceProvider)
    {
        var broken = new List<string>();
        foreach (Registration registration in Commands)
        {
            try
            {
                registration.Create(serviceProvider);
            }
            catch (InvalidOperationException ex)
            {
                broken.Add($":{registration.Names[0]}: {ex.Message}");
            }
        }

        if (broken.Count > 0)
        {
            throw new InvalidOperationException($"{broken.Count} chat command(s) can't be made:\n{string.Join("\n", broken)}");
        }
    }

    public async ValueTask<bool> TryHandleAsync(IGameSession session, string message, CancellationToken cancellationToken)
    {
        string[] parts = message[1..].Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length == 0 || !ByName.TryGetValue(parts[0], out Registration? registration))
        {
            return false;
        }

        if (session.AuthenticatedUser is not { } account)
        {
            return false;
        }

        IChatCommand command = registration.Create(_serviceProvider);

        if (command.RequiredFuse is { } requiredFuse
            && !await _fuses.HoldsAsync(account, requiredFuse, cancellationToken: cancellationToken))
        {
            return false;
        }

        string[] args = parts[1..];

        try
        {
            await command.ExecuteAsync(session, args, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Chat command '{Command}' failed for session {SessionId}", command.Name, session.Id);
        }

        return true;
    }

    public async Task<IReadOnlyList<string>> GetAvailableCommandsAsync(UserAccount account, CancellationToken cancellationToken)
    {
        IReadOnlyList<string> fuses = await _fuses.GetHeldAsync(account, cancellationToken);

        var lines = new List<string>();
        foreach (IGrouping<ChatCommandSection, Registration> section in Commands
                     .Where(registration => registration.Section is not null && MayRun(registration.Create(_serviceProvider), fuses))
                     .GroupBy(registration => registration.Section!.Value)
                     .OrderBy(section => section.Key))
        {
            if (lines.Count > 0)
            {
                lines.Add(string.Empty);
            }

            lines.Add($"-- {section.Key} --");
            lines.AddRange(section.Select(registration => registration.HelpLine));
        }

        return lines;
    }

    private static bool MayRun(IChatCommand command, IReadOnlyList<string> fuses) =>
        command.RequiredFuse is not { } requiredFuse || fuses.Contains(requiredFuse);

    private static IChatCommand Create<TCommand>(IServiceProvider serviceProvider) where TCommand : IChatCommand =>
        ActivatorUtilities.CreateInstance<TCommand>(serviceProvider);
}
