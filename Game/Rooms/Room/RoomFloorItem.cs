using DragonEmu.Domain;

namespace DragonEmu.Game.Rooms;

public sealed class RoomFloorItem
{
    private readonly bool _isWalkable;

    public int Id { get; }

    public string Classname { get; }

    public int X { get; private set; }

    public int Y { get; private set; }

    public double Z => Pressed ? PressedZ : BaseZ;

    public double BaseZ { get; private set; }

    public bool Pressed { get; set; }

    public const double PressedZ = 0;

    public int Rotation { get; private set; }

    public int Width { get; }

    public int Length { get; }

    public bool IsWalkable => IsGate ? IsGateOpen : _isWalkable;

    public bool IsSittable { get; }

    public bool IsBed { get; }

    public bool IsStackable { get; }

    public double StackHeight => Heights is { Count: > 0 } heights ? heights[Math.Clamp(HeightIndex, 0, heights.Count - 1)] : _stackHeight;

    private readonly double _stackHeight;

    public IReadOnlyList<double>? Heights { get; init; }

    public int HeightIndex { get; set; }

    public static int HeightIndexOf(IReadOnlyList<int>? states, string? extraData)
    {
        if (states is null || !int.TryParse(extraData, System.Globalization.NumberStyles.Integer, System.Globalization.CultureInfo.InvariantCulture, out int state))
        {
            return 0;
        }

        int index = -1;
        for (int i = 0; i < states.Count; i++)
        {
            if (states[i] == state)
            {
                index = i;
                break;
            }
        }

        return Math.Max(0, index);
    }

    public FurnitureInteractionLogic InteractionLogic { get; }

    public IReadOnlyList<int>? States { get; }

    public FurnitureLogicClass LogicClass { get; }

    public FurnitureVisualizationClass VisualizationClass { get; }

    public bool IsPositionPending { get; set; }

    public IReadOnlyList<int>? InteractionIds { get; }

    public bool IsGate => InteractionLogic == FurnitureInteractionLogic.Gate;

    public bool IsGateOpen { get; set; }

    public bool IsRoller => InteractionLogic == FurnitureInteractionLogic.Roller;

    public bool IsAutoState => InteractionLogic is FurnitureInteractionLogic.PoolInflatable or FurnitureInteractionLogic.PressurePlate;

    public bool IsWaterArea => VisualizationClass == FurnitureVisualizationClass.WaterArea;

    public int WaterEffectId => IsWaterArea && InteractionIds is { Count: > 0 } ids ? ids[0] : 0;

    public bool IsClothingGate => InteractionLogic == FurnitureInteractionLogic.ClothingGate;

    public string? GateOutfits { get; set; }

    public bool IsFootball => InteractionLogic == FurnitureInteractionLogic.Football;

    public bool IsFootballGoal => InteractionLogic == FurnitureInteractionLogic.FootballGoal;

    public bool IsBanzaiPuck => InteractionLogic == FurnitureInteractionLogic.BanzaiPuck;

    public bool IsBall => IsFootball || IsBanzaiPuck;

    public bool IsBanzaiTile => InteractionLogic == FurnitureInteractionLogic.BanzaiTile;

    public bool IsBanzaiGate => InteractionLogic == FurnitureInteractionLogic.BanzaiGate;

    public bool IsBanzaiScore => InteractionLogic == FurnitureInteractionLogic.BanzaiScore;

    public int TileState { get; set; }

    public static int ParseState(string? extraData) =>
        int.TryParse(extraData, System.Globalization.NumberStyles.Integer, System.Globalization.CultureInfo.InvariantCulture, out int state) ? state : 0;

    public int RefFurnidata { get; init; }

    public bool IsTotemPiece => InteractionLogic is FurnitureInteractionLogic.TotemLeg or FurnitureInteractionLogic.TotemHead or FurnitureInteractionLogic.TotemPlanet;

    public bool IsOnTopOf(RoomFloorItem below) => X == below.X && Y == below.Y && Math.Abs(Z - below.SurfaceZ) < 0.001;

    public bool IsSingleTile => Width == 1 && Length == 1;

    public RoomFloorItem(
        int id, int x, int y, double z, int rotation, int width, int length,
        bool isWalkable, bool isSittable, bool isBed, bool isStackable, double stackHeight,
        string classname = "", FurnitureInteractionLogic interactionLogic = FurnitureInteractionLogic.None,
        IReadOnlyList<int>? states = null, FurnitureLogicClass logicClass = FurnitureLogicClass.Basic,
        FurnitureVisualizationClass visualizationClass = FurnitureVisualizationClass.Static, IReadOnlyList<int>? interactionIds = null)
    {
        InteractionIds = interactionIds;
        LogicClass = logicClass;
        VisualizationClass = visualizationClass;
        Id = id;
        Classname = classname;
        X = x;
        Y = y;
        BaseZ = z;
        Rotation = rotation;
        Width = width < 1 ? 1 : width;
        Length = length < 1 ? 1 : length;
        _isWalkable = isWalkable;
        IsSittable = isSittable;
        IsBed = isBed;
        IsStackable = isStackable;
        _stackHeight = stackHeight;
        InteractionLogic = interactionLogic;
        States = states;
    }

    public int Direction => ((Rotation / 45) % 8 + 8) % 8;

    public (int X, int Y) SquareInFront() => Direction switch
    {
        0 => (X, Y - 1),
        1 => (X + 1, Y - 1),
        2 => (X + 1, Y),
        3 => (X + 1, Y + 1),
        4 => (X, Y + 1),
        5 => (X - 1, Y + 1),
        6 => (X - 1, Y),
        _ => (X - 1, Y - 1),
    };

    public (int X, int Y) SquareBehind()
    {
        (int frontX, int frontY) = SquareInFront();
        return (2 * X - frontX, 2 * Y - frontY);
    }

    public int StateValue(int index) => States is { } states && index < states.Count ? states[index] : index;

    public bool Steppable => IsWalkable || IsSittable || IsBed;

    public double SurfaceZ => Z + StackHeight;

    public IEnumerable<(int X, int Y)> OccupiedTiles() => OccupiedTilesAt(X, Y, Rotation);

    public IEnumerable<(int X, int Y)> OccupiedTilesAt(int x, int y, int rotation)
    {
        int rot = ((rotation / 45) % 8 + 8) % 8;
        bool rotated = rot == 2 || rot == 6;
        int spanX = rotated ? Length : Width;
        int spanY = rotated ? Width : Length;

        for (int dx = 0; dx < spanX; dx++)
        {
            for (int dy = 0; dy < spanY; dy++)
            {
                yield return (x + dx, y + dy);
            }
        }
    }

    public IEnumerable<(int X, int Y)> LayTiles()
    {
        if (!IsBed)
        {
            yield break;
        }

        int rot = ((Rotation / 45) % 8 + 8) % 8;

        for (int i = 0; i < Width; i++)
        {
            yield return rot switch
            {
                0 => (X + i, Y),
                4 => (X + i, Y + Length - 1),
                2 => (X, Y + i),
                6 => (X + Length - 1, Y + i),
                _ => (X + i, Y),
            };
        }
    }

    public bool IsNextTo(int x, int y) => OccupiedTiles().Any(tile => Math.Abs(tile.X - x) <= 1 && Math.Abs(tile.Y - y) <= 1);

    public void MoveTo(int x, int y, double z, int rotation)
    {
        X = x;
        Y = y;
        BaseZ = z;
        Rotation = rotation;
    }
}
