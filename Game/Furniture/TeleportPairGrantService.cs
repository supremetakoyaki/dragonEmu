using DragonEmu.Domain;
using DragonEmu.Persistence;

namespace DragonEmu.Game.Furniture;

public sealed class TeleportPairGrantService
{
    private readonly IUserItemRepository _userItemRepository;
    private readonly ITeleportPairRepository _teleportPairRepository;

    public TeleportPairGrantService(IUserItemRepository userItemRepository, ITeleportPairRepository teleportPairRepository)
    {
        _userItemRepository = userItemRepository;
        _teleportPairRepository = teleportPairRepository;
    }

    public static bool IsTeleporter(FurnitureDefinition definition) =>
        definition.Type == FurnitureType.Floor && definition.InteractionLogic == FurnitureInteractionLogic.Teleport;

    public async Task<IReadOnlyList<int>> GrantAsync(
        int userId, FurnitureDefinition definition, string? extraData, bool seen, int count, CancellationToken cancellationToken = default)
    {
        var createdItemIds = new List<int>();

        if (definition.LogicClass == FurnitureLogicClass.Stickie)
        {
            string notes = (count * Stickie.NotesPerPad).ToString(System.Globalization.CultureInfo.InvariantCulture);
            createdItemIds.Add(await _userItemRepository.CreateInInventoryAsync(userId, definition.Id, notes, seen, cancellationToken));
            return createdItemIds;
        }

        if (IsTeleporter(definition))
        {
            for (int i = 0; i < count; i++)
            {
                int itemId1 = await _userItemRepository.CreateInInventoryAsync(userId, definition.Id, extraData, seen, cancellationToken);
                int itemId2 = await _userItemRepository.CreateInInventoryAsync(userId, definition.Id, extraData, seen, cancellationToken);
                await _teleportPairRepository.CreateAsync(itemId1, itemId2, cancellationToken);

                createdItemIds.Add(itemId1);
                createdItemIds.Add(itemId2);
            }

            return createdItemIds;
        }

        for (int i = 0; i < count; i++)
        {
            createdItemIds.Add(await _userItemRepository.CreateInInventoryAsync(userId, definition.Id, extraData, seen, cancellationToken));
        }

        return createdItemIds;
    }
}
