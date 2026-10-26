using DragonEmu.Domain;
using DragonEmu.Game.Rooms;
using DragonEmu.Gateway;
using DragonEmu.Persistence;
using DragonEmu.Protocol.Messages.Incoming.RoomEngine.AuxiliaryFunctions;
using DragonEmu.Protocol.Messages.Outgoing.InventoryFurni;
using DragonEmu.Protocol.Messages.Outgoing.RoomEngine;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace DragonEmu.Game.Furniture;

public sealed class RentalExpiryService : IHostedService, IDisposable
{
    private const int RentalsPerSweep = 500;

    private static readonly TimeSpan SweepInterval = TimeSpan.FromMinutes(1);

    private readonly IUserItemRepository _userItemRepository;
    private readonly IRoomManager _roomManager;
    private readonly RoomBroadcaster _broadcaster;
    private readonly ClientSessionManager _sessionManager;
    private readonly ILogger<RentalExpiryService> _logger;
    private readonly PeriodicTimer _timer = new(SweepInterval);
    private CancellationTokenSource? _stoppingCts;
    private Task? _loop;

    public RentalExpiryService(
        IUserItemRepository userItemRepository, IRoomManager roomManager, RoomBroadcaster broadcaster, ClientSessionManager sessionManager,
        ILogger<RentalExpiryService> logger)
    {
        _userItemRepository = userItemRepository;
        _roomManager = roomManager;
        _broadcaster = broadcaster;
        _sessionManager = sessionManager;
        _logger = logger;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _stoppingCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        _loop = RunAsync(_stoppingCts.Token);
        return Task.CompletedTask;
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        _stoppingCts?.Cancel();
        if (_loop is not null)
        {
            try
            {
                await _loop.WaitAsync(cancellationToken);
            }
            catch (OperationCanceledException)
            {
            }
        }
    }

    public async Task<int> SweepAsync(CancellationToken cancellationToken)
    {
        int taken = 0;
        foreach (ExpiredRental rental in await _userItemRepository.GetExpiredRentalsAsync(RentalsPerSweep, cancellationToken))
        {
            if (!await _userItemRepository.DeleteExpiredRentalAsync(rental.ItemId, cancellationToken))
            {
                continue;
            }

            taken++;
            if (rental.RoomId is { } roomId)
            {
                if (_roomManager.TryGetLoadedRoom(roomId, out Room? room))
                {
                    TakeOutOfRoom(room!, rental);
                }
            }
            else if (_sessionManager.TryGetByUserId(rental.UserId, out ClientSession? renter))
            {
                renter!.EnqueueSend(new FurniListRemoveEvent(rental.ItemId).Compose());
            }
        }

        if (taken > 0)
        {
            _logger.LogDebug("{Count} rented items expired and went back.", taken);
        }

        return taken;
    }

    private void TakeOutOfRoom(Room room, ExpiredRental rental)
    {
        if (rental.Type == FurnitureType.Floor)
        {
            FloorItemRemoval.RemoveFromRoom(room, rental.ItemId, _broadcaster);
            return;
        }

        room.RemoveWallItem(rental.ItemId);
        _broadcaster.Broadcast(room, new ItemRemoveMessageEvent(rental.ItemId).Compose());
    }

    private async Task RunAsync(CancellationToken cancellationToken)
    {
        try
        {
            do
            {
                try
                {
                    await SweepAsync(cancellationToken);
                }
                catch (Exception ex) when (ex is not OperationCanceledException)
                {
                    _logger.LogError(ex, "Rental sweep failed; the next one tries again.");
                }
            }
            while (await _timer.WaitForNextTickAsync(cancellationToken));
        }
        catch (OperationCanceledException)
        {
        }
    }

    public void Dispose() => _timer.Dispose();
}
