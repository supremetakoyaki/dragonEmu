namespace DragonEmu.Game.Rooms;

public readonly record struct RollStep(int AfterTicks, string ExtraData);

public readonly record struct FurnitureStateChange(int ItemId, string ExtraData);

public sealed class FurnitureRollSequence
{
    private readonly IReadOnlyList<RollStep> _steps;
    private int _tick;
    private int _next;
    private int _dueTick;

    public FurnitureRollSequence(int itemId, IReadOnlyList<RollStep> steps)
    {
        ItemId = itemId;
        _steps = steps;
        _dueTick = 1 + steps[0].AfterTicks;
    }

    public int ItemId { get; }

    public int StepsShown => _next;

    public bool IsDone => _next >= _steps.Count;

    internal void Advance(List<FurnitureStateChange> changes)
    {
        _tick++;
        while (!IsDone && _tick >= _dueTick)
        {
            changes.Add(new FurnitureStateChange(ItemId, _steps[_next].ExtraData));
            _next++;
            if (!IsDone)
            {
                _dueTick += _steps[_next].AfterTicks;
            }
        }
    }
}
