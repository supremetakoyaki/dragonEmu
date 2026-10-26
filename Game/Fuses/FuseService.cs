using DragonEmu.Domain;
using DragonEmu.Game.Achievements;
using DragonEmu.Game.Subscriptions;
using DragonEmu.Persistence;
using DragonEmu.Protocol.Messages;
using DragonEmu.Protocol.Messages.Outgoing.Handshake;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace DragonEmu.Game.Fuses;

public sealed class FuseService : IHostedService
{
    private readonly IFuseRepository _fuseRepository;
    private readonly SubscriptionManager _subscriptions;
    private readonly IUserAchievementRepository _achievementProgress;
    private readonly AchievementDefinitionCache _achievementDefinitions;
    private readonly ILogger<FuseService> _logger;

    private volatile IReadOnlyDictionary<string, FuseRequirement> _requirements = new Dictionary<string, FuseRequirement>();
    private volatile IReadOnlySet<string> _achievementsLookedAt = new HashSet<string>();

    public FuseService(
        IFuseRepository fuseRepository, SubscriptionManager subscriptions, IUserAchievementRepository achievementProgress,
        AchievementDefinitionCache achievementDefinitions, ILogger<FuseService> logger)
    {
        _fuseRepository = fuseRepository;
        _subscriptions = subscriptions;
        _achievementProgress = achievementProgress;
        _achievementDefinitions = achievementDefinitions;
        _logger = logger;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        var requirements = new Dictionary<string, FuseRequirement>();
        foreach ((string label, string requirement) in await _fuseRepository.GetAllAsync(cancellationToken))
        {
            try
            {
                requirements[label] = FuseRequirement.Parse(requirement);
            }
            catch (FormatException ex)
            {
                throw new InvalidOperationException($"definition_fuse '{label}': {ex.Message}", ex);
            }
        }

        _requirements = requirements;
        _achievementsLookedAt = requirements.Values.SelectMany(r => r.Achievements).ToHashSet();
        _logger.LogInformation("Fuses loaded: {Count}", requirements.Count);
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    public async Task<bool> HoldsAsync(UserAccount account, string fuse, PrivateRoomSummary? room = null, CancellationToken cancellationToken = default) =>
        _requirements.TryGetValue(fuse, out FuseRequirement? requirement)
        && await requirement.IsMetAsync(new Facts(this, account, room, cancellationToken));

    public async Task<IReadOnlyList<string>> GetHeldAsync(UserAccount account, CancellationToken cancellationToken = default)
    {
        var facts = new Facts(this, account, room: null, cancellationToken);
        var held = new List<string>();
        foreach ((string label, FuseRequirement requirement) in _requirements)
        {
            if (await requirement.IsMetAsync(facts))
            {
                held.Add(label);
            }
        }

        return held;
    }

    public async Task SendHeldAsync(IGameSession session, UserAccount account, CancellationToken cancellationToken = default) =>
        session.Send(new UserRightsMessageEvent(await GetHeldAsync(account, cancellationToken)).Compose());

    public bool LooksAtAchievement(string codeBase) => _achievementsLookedAt.Contains(codeBase);

    private sealed class Facts(FuseService fuses, UserAccount account, PrivateRoomSummary? room, CancellationToken cancellationToken) : IFuseFacts
    {
        private readonly Dictionary<string, int> _achievementLevels = [];
        private bool? _isClubMember;

        public int Rank => account.Rank;

        public bool IsRoomOwner => room?.OwnerId == account.Id;

        public async ValueTask<bool> HasSubscriptionAsync(string subscription) =>
            _isClubMember ??= await fuses._subscriptions.IsClubMemberAsync(account.Id, cancellationToken);

        public async ValueTask<int> AchievementLevelAsync(string codeBase)
        {
            if (!_achievementLevels.TryGetValue(codeBase, out int level))
            {
                level = fuses._achievementDefinitions.GetByCodeBase(codeBase) is { } definition
                    ? (await fuses._achievementProgress.GetProgressAsync(account.Id, definition.Id, cancellationToken)).CurrentLevel
                    : 0;
                _achievementLevels[codeBase] = level;
            }

            return level;
        }
    }
}
