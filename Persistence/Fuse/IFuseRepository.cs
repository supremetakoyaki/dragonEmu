namespace DragonEmu.Persistence;

public interface IFuseRepository
{
    Task<IReadOnlyList<(string Label, string Requirement)>> GetAllAsync(CancellationToken cancellationToken = default);
}
