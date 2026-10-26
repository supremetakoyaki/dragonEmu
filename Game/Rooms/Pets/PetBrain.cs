using System.Collections.Concurrent;
using DragonEmu.Domain;

namespace DragonEmu.Game.Rooms;

public enum PetMode
{
    Free,

    Posing,

    Staying,

    Following,

    GoingToOwner,

    Sleeping,

    Eating,

    Drinking,

    Playing,
}

public enum PetFollowSide
{
    Behind,
    Left,
    Right,
}

public sealed class PetBrain
{
    public const int RespectExperience = 10;

    public const int NutritionTicks = 40;
    public const int EnergyTicks = 10;
    public const int SleepingEnergyTicks = 2;
    public const int NestEnergyPerTick = 2;

    public const int RestedPercent = 95;

    private readonly ConcurrentQueue<(int CommandId, long SessionId)> _commands = new();
    private int _pendingExperience;
    private int _pendingRespect = -1;
    private PetStats _stats;
    private int _dirty;

    public PetBrain(PetRecord record, PetDefinitions definitions)
    {
        Definitions = definitions;
        PetId = record.Id;
        OwnerId = record.OwnerId;
        OwnerName = record.OwnerName;
        Type = record.Type;
        Breed = record.Breed;
        Color = record.Color;
        Name = record.Name;
        CreatedAt = record.CreatedAt;
        Level = Math.Clamp(record.Level, 1, definitions.MaxLevel);
        Experience = Math.Max(0, record.Experience);
        Energy = Math.Clamp(record.Energy, 0, EnergyCap);
        Nutrition = Math.Clamp(record.Nutrition, 0, NutritionCap);
        Respect = record.Respect;
        _stats = new PetStats(Level, Experience, Energy, Nutrition, Respect);
    }

    public PetDefinitions Definitions { get; }

    public int PetId { get; }

    public int OwnerId { get; }

    public string OwnerName { get; set; }

    public int Type { get; }

    public int Breed { get; }

    public string Color { get; }

    public string Name { get; }

    public DateTime CreatedAt { get; }

    public string Figure => PetFigure.Of(Type, Breed, Color);

    public PetStats Stats => Volatile.Read(ref _stats);

    public PetRecord AsInventoryRecord()
    {
        PetStats stats = Stats;
        return new PetRecord(
            PetId, OwnerId, OwnerName, Type, Breed, Color, Name, RoomId: null, X: 0, Y: 0, Z: 0,
            stats.Respect, stats.Level, stats.Experience, stats.Energy, stats.Nutrition, CreatedAt);
    }

    public void Hear(int commandId, long sessionId) => _commands.Enqueue((commandId, sessionId));

    public void Respected(int newTotal)
    {
        Volatile.Write(ref _pendingRespect, newTotal);
        Interlocked.Add(ref _pendingExperience, RespectExperience);
    }

    public bool TakeDirty() => Interlocked.Exchange(ref _dirty, 0) == 1;

    internal int Level { get; private set; }

    internal int Experience { get; private set; }

    internal int Energy { get; set; }

    internal int Nutrition { get; set; }

    internal int Respect { get; private set; }

    internal PetMode Mode { get; set; } = PetMode.Free;

    internal int ModeTicksLeft { get; set; }

    internal int TargetItemId { get; set; }

    internal PetFollowSide FollowSide { get; set; }

    internal bool InNest { get; set; }

    internal bool WelcomesHome { get; set; }

    internal int Ticks { get; set; }

    internal int NextIdleTick { get; set; }

    internal int SilentUntilTick { get; set; }

    internal int GestureTicksLeft { get; set; }

    internal int BiteTicksLeft { get; set; }

    internal int Bites { get; set; }

    internal int TravelTicks { get; set; }

    internal PetLevel LevelInfo => Definitions.LevelOf(Level);

    internal int NutritionCap => LevelInfo.NutritionCap;

    internal int EnergyCap => LevelInfo.EnergyCap;

    internal bool IsRested => Energy * 100 >= EnergyCap * RestedPercent;

    internal bool TryTakeCommand(out (int CommandId, long SessionId) command) => _commands.TryDequeue(out command);

    internal int TakeExperience() => Interlocked.Exchange(ref _pendingExperience, 0);

    internal void TakeRespect()
    {
        int respect = Interlocked.Exchange(ref _pendingRespect, -1);
        if (respect >= 0)
        {
            Respect = respect;
        }
    }

    internal bool PassTime()
    {
        Ticks++;
        if (Ticks % NutritionTicks == 0 && Nutrition > 0)
        {
            Nutrition--;
        }

        bool asleep = Mode == PetMode.Sleeping;
        if (asleep && InNest)
        {
            Energy = Math.Min(EnergyCap, Energy + NestEnergyPerTick);
            return IsRested;
        }

        int every = asleep ? SleepingEnergyTicks : EnergyTicks;
        if (Ticks % every == 0 && Energy < EnergyCap)
        {
            Energy = Math.Min(EnergyCap, Energy + (!asleep && Nutrition * 2 > NutritionCap ? 2 : 1));
        }

        return asleep && IsRested;
    }

    internal bool Obeys(Random random)
    {
        int chance = Math.Min(95, 50 + Level * 2 + (NutritionCap > 0 ? Nutrition * 30 / NutritionCap : 0));
        return random.Next(100) < chance;
    }

    internal List<int> Gain(int amount)
    {
        var reached = new List<int>();
        if (amount <= 0)
        {
            return reached;
        }

        Experience = Math.Min(Experience + amount, Definitions.LevelOf(Definitions.MaxLevel).ExperienceCap);
        while (Level < Definitions.MaxLevel && Experience >= LevelInfo.ExperienceCap)
        {
            Level++;
            reached.Add(Level);
        }

        return reached;
    }

    internal void Publish()
    {
        Energy = Math.Clamp(Energy, 0, EnergyCap);
        Nutrition = Math.Clamp(Nutrition, 0, NutritionCap);
        var stats = new PetStats(Level, Experience, Energy, Nutrition, Respect);
        if (stats != Volatile.Read(ref _stats))
        {
            Volatile.Write(ref _stats, stats);
            Interlocked.Exchange(ref _dirty, 1);
        }
    }

    internal void MarkDirty() => Interlocked.Exchange(ref _dirty, 1);
}
