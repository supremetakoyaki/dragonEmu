using DragonEmu.Domain;
using DragonEmu.Persistence;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace DragonEmu.Game.Moderation;

public sealed class ModerationPresetsCache : IHostedService
{
    private readonly IDefinitionModerationRepository _repository;
    private readonly ILogger<ModerationPresetsCache> _logger;
    private volatile ModerationPresets _presets = ModerationPresets.Empty;

    public ModerationPresetsCache(IDefinitionModerationRepository repository, ILogger<ModerationPresetsCache> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public ModerationPresets Current => _presets;

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        _presets = await _repository.GetPresetsAsync(cancellationToken);
        _logger.LogInformation(
            "Moderation presets loaded: {User} user, {Room} room message(s), {Categories} offence categories.",
            _presets.UserMessages.Count, _presets.RoomMessages.Count, _presets.Offences.Count);
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
