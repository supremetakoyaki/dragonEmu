using DragonEmu.Domain;

namespace DragonEmu.Persistence;

public sealed record PetSave(int PetId, PetStats Stats, int X, int Y, double Z);

public interface IPetRepository
{
    Task<IReadOnlyList<PetRecord>> GetInventoryAsync(int ownerId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<PetRecord>> GetInRoomAsync(int roomId, CancellationToken cancellationToken = default);

    Task<PetRecord?> GetByIdAsync(int petId, CancellationToken cancellationToken = default);

    Task<int> CreateAsync(int ownerId, int type, int breed, string color, string name, int energy, int nutrition, CancellationToken cancellationToken = default);

    Task<bool> PlaceAsync(int petId, int ownerId, int roomId, int x, int y, double z, CancellationToken cancellationToken = default);

    Task<bool> ReturnToInventoryAsync(int petId, PetStats stats, CancellationToken cancellationToken = default);

    Task<int> ReturnStrayPetsAsync(int roomId, int? roomOwnerId, CancellationToken cancellationToken = default);

    Task SaveAsync(IReadOnlyCollection<PetSave> pets, CancellationToken cancellationToken = default);

    Task<int> CountOwnedAsync(int ownerId, CancellationToken cancellationToken = default);

    Task<int?> AddRespectAsync(int petId, CancellationToken cancellationToken = default);

    Task<bool> TryUsePetRespectAsync(int userId, DateOnly today, int dailyGrant, CancellationToken cancellationToken = default);
}

public interface IPetDefinitionRepository
{
    Task<IReadOnlyList<PetLevel>> GetLevelsAsync(CancellationToken cancellationToken = default);

    Task<IReadOnlyList<PetCommand>> GetCommandsAsync(CancellationToken cancellationToken = default);

    Task<IReadOnlyList<(int Type, PetSpeechType Kind, string Text)>> GetSpeechAsync(CancellationToken cancellationToken = default);

    Task<IReadOnlyList<(int Type, int Breed)>> GetBreedsAsync(CancellationToken cancellationToken = default);

    Task<IReadOnlyList<(int Type, string Color)>> GetColorsAsync(CancellationToken cancellationToken = default);

    Task<IReadOnlyList<PetFood>> GetFoodAsync(CancellationToken cancellationToken = default);
}
