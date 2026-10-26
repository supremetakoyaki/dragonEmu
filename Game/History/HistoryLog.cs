using System.Threading.Channels;
using DragonEmu.Domain;
using DragonEmu.Persistence;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace DragonEmu.Game.History;

public sealed class HistoryLog : IHostedService, IDisposable
{
    public const int RecordsPerInsert = 500;
    public const int MaxQueued = 100_000;

    private static readonly TimeSpan FlushInterval = TimeSpan.FromSeconds(5);

    private readonly ILogger<HistoryLog> _logger;
    private readonly PeriodicTimer _timer = new(FlushInterval);
    private readonly Buffer<RoomChatRecord> _roomChat;
    private readonly Buffer<InstantMessageRecord> _instantMessages;
    private readonly Buffer<CatalogPurchaseRecord> _catalogPurchases;
    private readonly Buffer<RoomVisitRecord> _roomVisits;
    private CancellationTokenSource? _stoppingCts;
    private Task? _loop;

    public HistoryLog(IHistoryRepository repository, ILogger<HistoryLog> logger)
    {
        _logger = logger;
        _roomChat = new Buffer<RoomChatRecord>("room chat", repository.AddRoomChatAsync);
        _instantMessages = new Buffer<InstantMessageRecord>("messenger messages", repository.AddInstantMessagesAsync);
        _catalogPurchases = new Buffer<CatalogPurchaseRecord>("catalog purchases", repository.AddCatalogPurchasesAsync);
        _roomVisits = new Buffer<RoomVisitRecord>("room visits", repository.AddRoomVisitsAsync);
    }

    public void Record(RoomChatRecord record) => _roomChat.Add(record);

    public void Record(InstantMessageRecord record) => _instantMessages.Add(record);

    public void Record(CatalogPurchaseRecord record) => _catalogPurchases.Add(record);

    public void Record(RoomVisitRecord record) => _roomVisits.Add(record);

    public Task FlushRoomChatAsync() => _roomChat.FlushAsync(_logger);

    public Task FlushRoomVisitsAsync() => _roomVisits.FlushAsync(_logger);

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

        await FlushAsync();
    }

    private async Task FlushAsync()
    {
        await _roomChat.FlushAsync(_logger);
        await _instantMessages.FlushAsync(_logger);
        await _catalogPurchases.FlushAsync(_logger);
        await _roomVisits.FlushAsync(_logger);
    }

    private async Task RunAsync(CancellationToken cancellationToken)
    {
        try
        {
            while (await _timer.WaitForNextTickAsync(cancellationToken))
            {
                await FlushAsync();
            }
        }
        catch (OperationCanceledException)
        {
        }
    }

    public void Dispose() => _timer.Dispose();

    private sealed class Buffer<T>
    {
        private readonly string _name;
        private readonly Func<IReadOnlyList<T>, CancellationToken, Task> _write;
        private readonly Channel<T> _channel;
        private readonly List<T> _batch = new(RecordsPerInsert);
        private readonly SemaphoreSlim _flushing = new(1, 1);
        private long _dropped;

        public Buffer(string name, Func<IReadOnlyList<T>, CancellationToken, Task> write)
        {
            _name = name;
            _write = write;
            _channel = Channel.CreateBounded<T>(
                new BoundedChannelOptions(MaxQueued) { FullMode = BoundedChannelFullMode.DropOldest, SingleReader = true },
                _ => Interlocked.Increment(ref _dropped));
        }

        public void Add(T record) => _channel.Writer.TryWrite(record);

        public async Task FlushAsync(ILogger logger)
        {
            await _flushing.WaitAsync();
            try
            {
                await FlushQueuedAsync(logger);
            }
            finally
            {
                _flushing.Release();
            }
        }

        private async Task FlushQueuedAsync(ILogger logger)
        {
            if (Interlocked.Exchange(ref _dropped, 0) is var dropped and > 0)
            {
                logger.LogWarning("{Count} {Name} history record(s) were dropped because the queue was full.", dropped, _name);
            }

            try
            {
                while (true)
                {
                    while (_batch.Count < RecordsPerInsert && _channel.Reader.TryRead(out T? record))
                    {
                        _batch.Add(record);
                    }

                    if (_batch.Count == 0)
                    {
                        return;
                    }

                    await _write(_batch, CancellationToken.None);
                    _batch.Clear();
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "{Count} {Name} history record(s) failed to be written, will retry on next iteration.", _batch.Count, _name);
            }
        }
    }
}
