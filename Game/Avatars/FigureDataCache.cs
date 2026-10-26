using DragonEmu.Domain;
using DragonEmu.Persistence;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace DragonEmu.Game.Avatars;

public sealed class FigureDataCache : IHostedService
{
    private readonly IFigureDataRepository _repository;
    private readonly ILogger<FigureDataCache> _logger;

    private volatile Snapshot _snapshot = Snapshot.Empty;

    public FigureDataCache(IFigureDataRepository repository, ILogger<FigureDataCache> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public FigureSettype? GetSettype(string type) => _snapshot.Settypes.GetValueOrDefault(type);

    public FigureSet? GetSet(int id) => _snapshot.Sets.GetValueOrDefault(id);

    public FigureColor? GetColor(int id) => _snapshot.Colors.GetValueOrDefault(id);

    public IReadOnlyList<string> MandatorySettypeTypes => _snapshot.MandatorySettypeTypes;

    public FigureSet? GetDefaultFreeSet(string type, char gender) =>
        _snapshot.Sets.Values
            .Where(s => s.SettypeRef == type && !s.Club && s.Selectable && (s.Gender == gender || s.Gender == 'U'))
            .OrderBy(s => s.Id)
            .FirstOrDefault();

    public FigureColor? GetDefaultFreeColor(int paletteId) =>
        _snapshot.Colors.Values
            .Where(c => c.PaletteId == paletteId && !c.Club && c.Selectable)
            .OrderBy(c => c.Id)
            .FirstOrDefault();

    public async Task ReloadAsync(CancellationToken cancellationToken = default)
    {
        IReadOnlyList<FigureSettype> settypes = await _repository.GetSettypesAsync(cancellationToken);
        IReadOnlyList<FigureSet> sets = await _repository.GetSetsAsync(cancellationToken);
        IReadOnlyList<FigureColor> colors = await _repository.GetColorsAsync(cancellationToken);

        _snapshot = new Snapshot(
            settypes.ToDictionary(s => s.Type),
            sets.ToDictionary(s => s.Id),
            colors.ToDictionary(c => c.Id),
            settypes.Where(s => s.Mandatory).Select(s => s.Type).ToList());

        _logger.LogInformation(
            "Figure data cache loaded: {SettypeCount} settypes, {SetCount} sets, {ColorCount} colors.",
            settypes.Count, sets.Count, colors.Count);
    }

    public Task StartAsync(CancellationToken cancellationToken) => ReloadAsync(cancellationToken);

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    private sealed record Snapshot(
        IReadOnlyDictionary<string, FigureSettype> Settypes,
        IReadOnlyDictionary<int, FigureSet> Sets,
        IReadOnlyDictionary<int, FigureColor> Colors,
        IReadOnlyList<string> MandatorySettypeTypes)
    {
        public static readonly Snapshot Empty = new(
            new Dictionary<string, FigureSettype>(), new Dictionary<int, FigureSet>(), new Dictionary<int, FigureColor>(), []);
    }
}
