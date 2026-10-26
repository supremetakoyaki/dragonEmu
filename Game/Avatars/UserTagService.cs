using DragonEmu.Game.Achievements;
using DragonEmu.Persistence;

namespace DragonEmu.Game.Avatars;

public sealed class UserTagService
{
    public const int MaxTags = 5;
    public const string Achievement = "ACH_AvatarTags";

    private const int MaxTagLength = 25;

    private readonly IUserTagRepository _repository;
    private readonly AchievementService _achievements;

    public UserTagService(IUserTagRepository repository, AchievementService achievements)
    {
        _repository = repository;
        _achievements = achievements;
    }

    public async Task<IReadOnlyList<string>> GetAsync(int userId, CancellationToken cancellationToken) =>
        (await _repository.GetAsync(userId, MaxTags, cancellationToken))
            .Select(Clean)
            .Where(tag => tag.Length > 0)
            .ToList();

    public async Task CheckAchievementAsync(int userId, CancellationToken cancellationToken) =>
        await _achievements.RaiseToAsync(userId, Achievement, (await GetAsync(userId, cancellationToken)).Count, cancellationToken);

    private static string Clean(string tag)
    {
        string text = new string(tag.Where(c => !char.IsControl(c)).ToArray()).Trim();
        return text.Length <= MaxTagLength ? text : text[..MaxTagLength];
    }
}
