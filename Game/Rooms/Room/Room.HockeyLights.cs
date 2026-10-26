namespace DragonEmu.Game.Rooms;

public sealed partial class Room
{
    private const string HockeyLightOff = "0";
    private const string HockeyLightOn = "1";
    private const int HockeyLightLitTicks = 3;

    private readonly object _hockeyLightLock = new();
    private readonly Dictionary<int, int> _hockeyLights = new();

    public bool TryLightHockeyLight(RoomFloorItem light)
    {
        if (GetFloorItem(light.Id) is null)
        {
            return false;
        }

        lock (_hockeyLightLock)
        {
            return _hockeyLights.TryAdd(light.Id, 0);
        }
    }

    public List<FurnitureStateChange> AdvanceHockeyLights(out bool needsFurtherTicks)
    {
        lock (_hockeyLightLock)
        {
            var stateChanges = new List<FurnitureStateChange>();

            foreach ((int lightId, int ticks) in _hockeyLights.ToList())
            {
                if (GetFloorItem(lightId) is null)
                {
                    _hockeyLights.Remove(lightId);
                    continue;
                }

                int tick = ticks + 1;
                if (tick == 1)
                {
                    stateChanges.Add(new FurnitureStateChange(lightId, HockeyLightOn));
                }

                if (tick > HockeyLightLitTicks)
                {
                    stateChanges.Add(new FurnitureStateChange(lightId, HockeyLightOff));
                    _hockeyLights.Remove(lightId);
                }
                else
                {
                    _hockeyLights[lightId] = tick;
                }
            }

            needsFurtherTicks = _hockeyLights.Count > 0;
            return stateChanges;
        }
    }
}
