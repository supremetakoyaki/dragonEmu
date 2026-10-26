using DragonEmu.Domain;
using DragonEmu.Persistence;
using Microsoft.Extensions.Hosting;

namespace DragonEmu.Game.Help;

public sealed class HelpFaqCache : IHostedService
{
    private readonly IDefinitionModerationRepository _repository;
    private volatile IReadOnlyList<FaqCategory> _categories = [];
    private volatile IReadOnlyDictionary<int, FaqEntry> _entries = new Dictionary<int, FaqEntry>();

    public HelpFaqCache(IDefinitionModerationRepository repository)
    {
        _repository = repository;
    }

    public IReadOnlyList<FaqCategory> Categories => _categories;

    public (IReadOnlyList<FaqEntry> Urgent, IReadOnlyList<FaqEntry> Normal) FrontPage
    {
        get
        {
            List<FaqEntry> all = _categories.SelectMany(category => category.Entries).ToList();
            return (all.Where(entry => entry.FrontPage == FaqFrontPage.Urgent).ToList(), all.Where(entry => entry.FrontPage == FaqFrontPage.Normal).ToList());
        }
    }

    public FaqEntry? GetEntry(int id) => _entries.TryGetValue(id, out FaqEntry? entry) ? entry : null;

    public FaqCategory? GetCategory(int id) => _categories.FirstOrDefault(category => category.Id == id);

    public IReadOnlyList<FaqEntry> Search(string text, int limit) =>
        _categories.SelectMany(category => category.Entries)
            .Where(entry => entry.Question.Contains(text, StringComparison.OrdinalIgnoreCase) || entry.Answer.Contains(text, StringComparison.OrdinalIgnoreCase))
            .Take(limit)
            .ToList();

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        IReadOnlyList<FaqCategory> categories = await _repository.GetFaqAsync(cancellationToken);
        _entries = categories.SelectMany(category => category.Entries).ToDictionary(entry => entry.Id);
        _categories = categories;
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
