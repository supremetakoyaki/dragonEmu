using DragonEmu.Game.Hotel;
using System.Reflection;
using DragonEmu.Game.Achievements;
using DragonEmu.Game.Advertisement;
using DragonEmu.Game.Avatars;
using DragonEmu.Game.Catalog;
using DragonEmu.Game.Chat;
using DragonEmu.Game.Chat.Commands;
using DragonEmu.Game.Fuses;
using DragonEmu.Game.History;
using DragonEmu.Game.Ignoring;
using DragonEmu.Game.Help;
using DragonEmu.Game.Moderation;
using DragonEmu.Game.Furniture;
using DragonEmu.Game.Messenger;
using DragonEmu.Game.Marketplace;
using DragonEmu.Game.Rewards;
using DragonEmu.Game.Pets;
using DragonEmu.Game.Polls;
using DragonEmu.Game.Rooms;
using DragonEmu.Game.Settings;
using DragonEmu.Game.Subscriptions;
using DragonEmu.Game.Trading;
using DragonEmu.Game.Vouchers;
using DragonEmu.Gateway;
using DragonEmu.Logging;
using DragonEmu.Persistence;
using Microsoft.Extensions.Logging.Console;

namespace DragonEmu.Infrastructure;

public static class ServiceRegistration
{
    public static IHostApplicationBuilder AddDragonConsoleLogging(this IHostApplicationBuilder builder)
    {
        builder.Logging.ClearProviders();
        builder.Logging.AddConsole(options => options.FormatterName = DragonConsoleFormatter.FormatterName);
        builder.Logging.AddConsoleFormatter<DragonConsoleFormatter, ConsoleFormatterOptions>();
        builder.Services.Configure<ConsoleLifetimeOptions>(options => options.SuppressStatusMessages = true);
        return builder;
    }

    public static IServiceCollection AddPersistence(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<MySqlOptions>(configuration.GetSection(MySqlOptions.SectionName));
        services.AddSingleton<IDbConnectionFactory, MySqlConnectionFactory>();

        IEnumerable<Type> repositories = Assembly.GetExecutingAssembly().GetTypes().Where(type =>
            type is { IsClass: true, IsAbstract: false, Namespace: "DragonEmu.Persistence" } && type.Name.EndsWith("Repository", StringComparison.Ordinal));
        foreach (Type repository in repositories)
        {
            Type contract = repository.GetInterface($"I{repository.Name}")
                ?? throw new InvalidOperationException($"{repository.Name} doesn't implement I{repository.Name}.");
            services.AddSingleton(contract, repository);
        }

        return services;
    }

    public static IServiceCollection AddSettingsAndCaches(this IServiceCollection services)
    {
        services.AddHostedSingleton<GameSettingsCache>();
        services.AddHostedSingleton<MessageTexts>();
        services.AddSingleton<HotelClock>();

        services.AddSingleton<FurnitureDefinitionCache>();
        services.AddSingleton<TotemConfigCache>();
        services.AddSingleton<FurnitureAliasCache>();
        services.AddHostedService<FurnitureCachesLoader>();

        services.AddHostedSingleton<RecyclerPrizeCache>();
        services.AddHostedSingleton<CatalogCache>();
        services.AddHostedSingleton<ProfanityFilterCache>();
        services.AddHostedSingleton<FigureDataCache>();
        services.AddHostedSingleton<AchievementDefinitionCache>();
        services.AddHostedSingleton<FuseService>();
        services.AddHostedSingleton<InterstitialAdCache>();
        services.AddHostedSingleton<PetDefinitionCache>();
        services.AddHostedSingleton<PublicRoomCache>();
        return services;
    }

    public static IServiceCollection AddHistory(this IServiceCollection services)
    {
        services.AddHostedSingleton<HistoryLog>();
        services.AddHostedService<HistoryRetention>();
        return services;
    }

    public static IServiceCollection AddUsersAndRewards(this IServiceCollection services)
    {
        services.AddSingleton<AchievementService>();
        services.AddHostedSingleton<HappyHourService>();
        services.AddHostedSingleton<PresenceService>();
        services.AddHostedSingleton<OnlineRewardService>();
        services.AddHostedSingleton<AvatarEffectService>();
        services.AddSingleton<SubscriptionManager>();
        services.AddHostedService<SubscriptionExpiryService>();
        services.AddSingleton<MottoService>();
        services.AddSingleton<UsernameRules>();
        services.AddSingleton<UsernameChangeService>();
        services.AddSingleton<WardrobeService>();
        services.AddSingleton<UserTagService>();
        services.AddSingleton<RespectService>();
        services.AddSingleton<MessengerService>();
        services.AddSingleton<IgnoreService>();
        services.AddSingleton<ModerationService>();
        services.AddHostedSingleton<ModerationPresetsCache>();
        services.AddHostedSingleton<HelpFaqCache>();
        services.AddHostedSingleton<ModerationIssueService>();
        services.AddSingleton<ModerationToolService>();
        services.AddSingleton<TradingService>();
        services.AddSingleton<MarketplaceService>();
        services.AddSingleton<VoucherAttemptGuard>();
        return services;
    }

    public static IServiceCollection AddFurniture(this IServiceCollection services)
    {
        services.AddSingleton<TeleportPairGrantService>();
        services.AddSingleton<FurnitureStateService>();
        services.AddSingleton<FurnitureRollService>();
        services.AddSingleton<RoomDimmerService>();
        services.AddSingleton<TotemService>();
        services.AddSingleton<GateService>();
        services.AddSingleton<VendingService>();
        services.AddSingleton<HockeyLightService>();
        services.AddSingleton<TraxService>();
        services.AddSingleton<OneWayDoorService>();
        services.AddSingleton<StickieService>();
        services.AddHostedService<RentalExpiryService>();
        return services;
    }

    public static IServiceCollection AddRooms(this IServiceCollection services)
    {
        services.AddSingleton<RoomBotService>();
        services.AddHostedSingleton<RoomManager>();
        services.AddSingleton<IRoomManager>(sp => sp.GetRequiredService<RoomManager>());
        services.AddSingleton<RoomRightsService>();
        services.AddSingleton<RoomDeletionService>();
        services.AddSingleton<RoomEventService>();
        services.AddSingleton<RoomKickService>();
        services.AddSingleton<RoomCreationLimit>();
        services.AddSingleton<RoomBroadcaster>();
        services.AddSingleton<RoomPaintService>();
        services.AddSingleton<RoomTeleportService>();
        services.AddSingleton<PublicRoomTransfers>();
        services.AddSingleton<PetNameRules>();
        services.AddHostedSingleton<PetService>();
        services.AddHostedSingleton<PollService>();
        services.AddSingleton<RoomVoteService>();
        services.AddHostedSingleton<GuideBotService>();
        services.AddSingleton<HotelStatus>();
        services.AddSingleton<HotelShutdownService>();
        services.AddHostedSingleton<FurniturePositionWriter>();
        services.AddHostedSingleton<RoomMovementScheduler>();
        services.AddHostedSingleton<RoomIdleService>();
        return services;
    }

    public static IServiceCollection AddGateway(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<GameServerOptions>(configuration.GetSection(GameServerOptions.SectionName));
        services.AddSingleton<ClientSessionManager>();
        services.AddSingleton<IPacketDispatcher, GameMessageDispatcher>();
        services.AddSingleton<IChatCommandDispatcher, ChatCommandDispatcher>();
        services.AddHostedService<GameGatewayService>();
        return services;
    }

    private static IServiceCollection AddHostedSingleton<T>(this IServiceCollection services)
        where T : class, IHostedService
    {
        services.AddSingleton<T>();
        services.AddHostedService(sp => sp.GetRequiredService<T>());
        return services;
    }
}
