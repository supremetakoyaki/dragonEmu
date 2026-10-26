using System.Threading.Channels;
using DragonEmu.Domain;
using DragonEmu.Persistence;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace DragonEmu.Game.Rooms;

public sealed class FurniturePositionWriter : IHostedService
{
    private readonly Channel<(Room Room, IReadOnlyList<FloorItemPosition> Positions)> _queue =
        Channel.CreateUnbounded<(Room, IReadOnlyList<FloorItemPosition>)>(new UnboundedChannelOptions { SingleReader = true });

    private readonly IUserItemRepository _userItemRepository;
    private readonly ILogger<FurniturePositionWriter> _logger;
    private Task? _loop;

    public FurniturePositionWriter(IUserItemRepository userItemRepository, ILogger<FurniturePositionWriter> logger)
    {
        _userItemRepository = userItemRepository;
        _logger = logger;
    }

    public void Save(Room room, IReadOnlyList<FloorItemPosition> positions)
    {
        if (positions.Count > 0)
        {
            _queue.Writer.TryWrite((room, positions));
        }
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _loop = Task.Run(WriteAllAsync, CancellationToken.None);
        return Task.CompletedTask;
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        _queue.Writer.TryComplete();

        if (_loop is not null)
        {
            try
            {
                await _loop.WaitAsync(cancellationToken);
            }
            catch (OperationCanceledException)
            {
                _logger.LogWarning("Shutdown didn't wait for every rolled furniture position to be saved.");
            }
        }
    }

    private async Task WriteAllAsync()
    {
        await foreach ((Room room, IReadOnlyList<FloorItemPosition> positions) in _queue.Reader.ReadAllAsync())
        {
            try
            {
                await _userItemRepository.SaveFloorPositionsAsync(room.Id, positions);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Saving {Count} rolled furniture position(s) in room {RoomId} failed; retrying next tick.", positions.Count, room.Id);
                room.MarkFloorPositionsUnsaved(positions.Select(position => position.ItemId));
            }
        }
    }
}
