namespace DragonEmu.Persistence;

public interface IProfanityWordRepository
{
    Task<IReadOnlyList<string>> GetAllAsync(CancellationToken cancellationToken = default);
}
