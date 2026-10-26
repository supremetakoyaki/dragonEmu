using DragonEmu.Domain;
using DragonEmu.Game.Furniture;
using DragonEmu.Persistence;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace DragonEmu.Game.Pets;

public sealed class PetDefinitionCache : IHostedService
{
    private readonly IPetDefinitionRepository _repository;
    private readonly FurnitureDefinitionCache _furnitureDefinitions;
    private readonly ILogger<PetDefinitionCache> _logger;

    private volatile PetDefinitions _current = PetDefinitions.Empty;

    public PetDefinitionCache(IPetDefinitionRepository repository, FurnitureDefinitionCache furnitureDefinitions, ILogger<PetDefinitionCache> logger)
    {
        _logger = logger;
        _repository = repository;
        _furnitureDefinitions = furnitureDefinitions;
    }

    public PetDefinitions Current => _current;

    public async Task ReloadAsync(CancellationToken cancellationToken = default)
    {
        var foodByClassname = new Dictionary<string, PetFood>(StringComparer.OrdinalIgnoreCase);
        foreach (PetFood food in await _repository.GetFoodAsync(cancellationToken))
        {
            if (_furnitureDefinitions.GetById(food.FurniDefId) is { } definition)
            {
                foodByClassname[definition.Classname] = food;
            }
            else
            {
                _logger.LogWarning("Pet food furniture definition {FurniDefId} doesn't exist.", food.FurniDefId);
            }
        }

        IReadOnlyList<PetLevel> levels = await _repository.GetLevelsAsync(cancellationToken);
        IReadOnlyList<PetCommand> commands = await _repository.GetCommandsAsync(cancellationToken);
        IReadOnlyList<(int Type, PetSpeechType Kind, string Text)> speech = await _repository.GetSpeechAsync(cancellationToken);
        IReadOnlyList<(int Type, int Breed)> breeds = await _repository.GetBreedsAsync(cancellationToken);
        IReadOnlyList<(int Type, string Color)> colors = await _repository.GetColorsAsync(cancellationToken);
        _current = new PetDefinitions(levels, commands, speech, breeds, colors, foodByClassname);

        _logger.LogInformation(
            "Pet definitions loaded: {Levels} levels, {Commands} commands, {Breeds} breeds, {Colors} colours, {Food} pet interactive items.",
            levels.Count, commands.Count, breeds.Count, colors.Count, foodByClassname.Count);
    }

    public Task StartAsync(CancellationToken cancellationToken) => ReloadAsync(cancellationToken);

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
