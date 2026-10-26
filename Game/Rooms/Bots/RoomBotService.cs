using DragonEmu.Persistence;
using Microsoft.Extensions.Logging;

namespace DragonEmu.Game.Rooms;

public sealed class RoomBotService
{
    private readonly IRoomBotRepository _repository;
    private readonly ILogger<RoomBotService> _logger;

    public RoomBotService(IRoomBotRepository repository, ILogger<RoomBotService> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task<IReadOnlyList<BotDefinition>> LoadAsync(int roomId, CancellationToken cancellationToken)
    {
        IReadOnlyList<RoomBotRecord> records = await _repository.GetForRoomAsync(roomId, cancellationToken);
        return records
            .Select(record => BotDefinitionParser.Parse(record, warning => _logger.LogWarning("Room {RoomId}: {Warning}", roomId, warning)))
            .ToList();
    }
}
