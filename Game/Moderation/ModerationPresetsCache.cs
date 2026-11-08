using DragonEmu.Domain;
using DragonEmu.Persistence;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace DragonEmu.Game.Moderation;

public sealed class ModerationPresetsCache : IHostedService
{
    private readonly IDefinitionModerationRepository _repository;
    private readonly ILogger<ModerationPresetsCache> _logger;
    private volatile IReadOnlyDictionary<Language, ModerationPresets> _presets = new Dictionary<Language, ModerationPresets>();

    public ModerationPresetsCache(IDefinitionModerationRepository repository, ILogger<ModerationPresetsCache> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public ModerationPresets For(Language language) =>
        _presets.GetValueOrDefault(language) ?? _presets.GetValueOrDefault(Language.En) ?? ModerationPresets.Empty;

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        _presets = await _repository.GetPresetsAsync(cancellationToken);
        ModerationPresets english = For(Language.En);
        _logger.LogInformation(
            "Moderation presets loaded: {User} user, {Room} room message(s), {Categories} offence categories.",
            english.UserMessages.Count, english.RoomMessages.Count, english.Offences.Count);
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
