using DragonEmu.Domain;
using DragonEmu.Game.Rooms;

namespace DragonEmu.Game.Furniture;

public sealed record TotemStack(RoomFloorItem Leg, RoomFloorItem Head, RoomFloorItem? Planet)
{
    public static TotemStack? Find(IReadOnlyList<RoomFloorItem> pieces)
    {
        RoomFloorItem[] legs = pieces.Where(piece => piece.InteractionLogic == FurnitureInteractionLogic.TotemLeg).ToArray();
        RoomFloorItem[] heads = pieces.Where(piece => piece.InteractionLogic == FurnitureInteractionLogic.TotemHead).ToArray();
        if (legs is not [var leg] || heads is not [var head] || !head.IsOnTopOf(leg))
        {
            return null;
        }

        RoomFloorItem? planet = pieces.FirstOrDefault(piece => piece.InteractionLogic == FurnitureInteractionLogic.TotemPlanet && piece.IsOnTopOf(head));
        return new TotemStack(leg, head, planet);
    }
}
