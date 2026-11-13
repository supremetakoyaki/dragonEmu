using DragonEmu.Domain;

namespace DragonEmu.Game.Rooms;

public sealed partial class Room
{
    private readonly object _outfitLock = new();
    private readonly List<RoomActor> _figureChanges = [];

    public HashSet<(int X, int Y)> FurnitureStraightOnlyTiles()
    {
        var tiles = new HashSet<(int X, int Y)>();
        lock (_furnitureLock)
        {
            foreach (RoomFloorItem gate in _floorItems.Values)
            {
                if (gate.IsClothingGate)
                {
                    tiles.UnionWith(gate.OccupiedTiles());
                }
            }
        }

        return tiles;
    }

    private bool IsDiagonalThroughGate(int fromX, int fromY, int toX, int toY)
    {
        if (fromX == toX || fromY == toY)
        {
            return false;
        }

        lock (_furnitureLock)
        {
            return ItemsAtUnlocked(fromX, fromY).Any(item => item.IsClothingGate) || ItemsAtUnlocked(toX, toY).Any(item => item.IsClothingGate);
        }
    }

    public string? GetGateOutfits(int itemId)
    {
        lock (_furnitureLock)
        {
            return _floorItems.TryGetValue(itemId, out RoomFloorItem? gate) && gate.IsClothingGate ? gate.GateOutfits : null;
        }
    }

    public string? SetGateOutfit(int itemId, char gender, string figure)
    {
        lock (_furnitureLock)
        {
            if (!_floorItems.TryGetValue(itemId, out RoomFloorItem? gate) || !gate.IsClothingGate)
            {
                return null;
            }

            gate.GateOutfits = ClothingGateOutfits.With(gate.GateOutfits, gender, figure);
            return gate.GateOutfits;
        }
    }

    private bool TryUseClothingGate(RoomActor actor)
    {
        if (actor.Type != ActorType.Player)
        {
            return false;
        }

        RoomFloorItem? gate;
        lock (_furnitureLock)
        {
            gate = ItemsAtUnlocked(actor.X, actor.Y).LastOrDefault(item => item.IsClothingGate);
        }

        if (gate is null)
        {
            return false;
        }

        if (actor.RegularFigure is { } regular)
        {
            actor.Figure = regular;
            actor.Gender = actor.RegularGender;
            actor.RegularFigure = null;
        }
        else if (ClothingGateOutfits.For(GetGateOutfits(gate.Id), actor.Gender) is { } outfit)
        {
            actor.RegularFigure = actor.Figure;
            actor.RegularGender = actor.Gender;
            actor.Figure = ClothingGateOutfits.Combine(outfit, actor.Figure);
        }
        else
        {
            return false;
        }

        lock (_outfitLock)
        {
            if (!_figureChanges.Contains(actor))
            {
                _figureChanges.Add(actor);
            }
        }

        return true;
    }

    public IReadOnlyList<RoomActor> TakeFigureChanges()
    {
        lock (_outfitLock)
        {
            if (_figureChanges.Count == 0)
            {
                return [];
            }

            RoomActor[] changed = [.. _figureChanges];
            _figureChanges.Clear();
            return changed;
        }
    }
}
