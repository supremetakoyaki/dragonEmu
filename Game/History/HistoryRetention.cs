using DragonEmu.Domain;
using DragonEmu.Persistence;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace DragonEmu.Game.History;

public sealed class HistoryRetention : IHostedService, IDisposable
{
    public const int RowsPerDelete = 5000;

    private static readonly TimeSpan SweepInterval = TimeSpan.FromHours(6);
    private static readonly TimeSpan PauseBetweenDeletes = TimeSpan.FromMilliseconds(200);

    private readonly IHistoryRepository _repository;
    private readonly ILogger<HistoryRetention> _logger;
    private readonly PeriodicTimer _timer = new(SweepInterval);
    private CancellationTokenSource? _stoppingCts;
    private Task? _loop;

    public HistoryRetention(IHistoryRepository repository, ILogger<HistoryRetention> logger)
    {
        _repository = repository;
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
                    _logger.LogError(ex, "History retention sweep failed; the next one tries again.");
                }
            }
            while (await _timer.WaitForNextTickAsync(cancellationToken));
        }
        catch (OperationCanceledException)
        {
        }
    }

    private async Task SweepAsync(CancellationToken cancellationToken)
    {
        HistoryRetentionDays retention = await _repository.GetRetentionAsync(cancellationToken);
        foreach (HistoryKind kind in Enum.GetValues<HistoryKind>())
        {
            int days = retention.For(kind);
            if (days <= 0)
            {
                continue;
            }

            int deleted = 0;
            int batch;
            do
            {
                batch = await _repository.DeleteOlderThanAsync(kind, days, RowsPerDelete, cancellationToken);
                deleted += batch;
                if (batch == RowsPerDelete)
                {
                    await Task.Delay(PauseBetweenDeletes, cancellationToken);
                }
            }
            while (batch == RowsPerDelete);

            if (deleted > 0)
            {
                _logger.LogDebug("History: {Count} {Kind} record(s) older than {Days} days deleted.", deleted, kind, days);
            }
        }
    }

    public void Dispose() => _timer.Dispose();
}
