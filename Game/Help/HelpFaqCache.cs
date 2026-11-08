using DragonEmu.Domain;
using DragonEmu.Persistence;
using Microsoft.Extensions.Hosting;

namespace DragonEmu.Game.Help;

public sealed class HelpFaqCache : IHostedService
{
    private readonly IDefinitionModerationRepository _repository;
    private volatile IReadOnlyDictionary<Language, IReadOnlyList<FaqCategory>> _categories = new Dictionary<Language, IReadOnlyList<FaqCategory>>();

    public HelpFaqCache(IDefinitionModerationRepository repository)
    {
        _repository = repository;
    }

    public IReadOnlyList<FaqCategory> Categories(Language language) =>
        _categories.GetValueOrDefault(language) ?? _categories.GetValueOrDefault(Language.En) ?? [];

    public (IReadOnlyList<FaqEntry> Urgent, IReadOnlyList<FaqEntry> Normal) FrontPage(Language language)
    {
        List<FaqEntry> all = Entries(language).ToList();
        return (all.Where(entry => entry.FrontPage == FaqFrontPage.Urgent).ToList(), all.Where(entry => entry.FrontPage == FaqFrontPage.Normal).ToList());
    }

    public FaqEntry? GetEntry(int id, Language language) => Entries(language).FirstOrDefault(entry => entry.Id == id);

    public FaqCategory? GetCategory(int id, Language language) => Categories(language).FirstOrDefault(category => category.Id == id);

    public IReadOnlyList<FaqEntry> Search(string text, int limit, Language language) =>
        Entries(language)
            .Where(entry => entry.Question.Contains(text, StringComparison.OrdinalIgnoreCase) || entry.Answer.Contains(text, StringComparison.OrdinalIgnoreCase))
            .Take(limit)
            .ToList();

    private IEnumerable<FaqEntry> Entries(Language language) => Categories(language).SelectMany(category => category.Entries);

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        _categories = await _repository.GetFaqAsync(cancellationToken);
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
