using DragonEmu.Domain;
using DragonEmu.Game.Trading;

namespace DragonEmu.Game.Rooms;

public enum ActorType
{
    Player,
    Bot,
    Pet,
}

public enum ActorPosture
{
    Stand,
    Sit,
    Lay,
}

public sealed class RoomActor
{
    public long SessionId { get; }

    public int VirtualId { get; }

    public ActorType Type { get; }

    public int AccountId { get; }

    public string Username { get; set; }

    public GroupBadge? FavouriteGroup { get; init; }

    public object Lock { get; } = new();

    public string Motto { get; set; }

    public string Figure { get; set; }

    public char Gender { get; set; }

    public int X { get; set; }

    public int Y { get; set; }

    public double Z { get; set; }

    public int RotHead { get; set; }

    public int RotBody { get; set; }

    public int DanceId { get; set; }

    public int BotDanceTicksLeft { get; set; }

    public int EffectId { get; set; }

    public int CarryItemId { get; set; }

    public int CarryTicksLeft { get; set; }

    public int? PendingSignId { get; set; }

    public ActorPosture Posture { get; set; } = ActorPosture.Stand;

    public double PostureHeight { get; set; }

    public bool IsWalking { get; set; }

    public List<(int X, int Y)>? Path { get; set; }

    public int PathIndex { get; set; }

    public (int X, int Y)? Goal { get; set; }

    public (int X, int Y, double Z)? PendingStep { get; set; }

    public TeleporterSequence? ActiveTeleport { get; set; }

    public OneWayDoorPassage? ActiveOneWayDoor { get; set; }

    public string? FlatControl { get; set; }

    public BotBrain? Bot { get; init; }

    public string? Swimsuit { get; set; }

    public bool HasSwimwear => Swimsuit is not null;

    public bool IsSwimming { get; set; }

    public bool InLockerRoom { get; set; }

    public PetBrain? Pet { get; init; }

    public string? PetPosture { get; set; }

    public string? PetGesture { get; set; }

    public const double PetSeatDrop = 0.5;

    public double ShownZ => Type == ActorType.Pet && Posture == ActorPosture.Sit && PetPosture is (null or "sit") && PendingStep is null
        ? Z - PetSeatDrop
        : Z;

    public bool IsHeldByFurniture => ActiveTeleport is not null || ActiveOneWayDoor is not null;

    public int? KickTicksLeft { get; set; }

    public bool IsKicked => KickTicksLeft is not null;

    public DateTime EnteredAt { get; } = DateTime.UtcNow;

    public DateTime LastActiveAt { get; set; } = DateTime.UtcNow;

    public DateTime? AsleepSince { get; set; }

    public bool SleptTooLong { get; set; }

    private Trade? _trade;

    public Trade? Trade => Volatile.Read(ref _trade);

    public bool TryJoinTrade(Trade trade) => Interlocked.CompareExchange(ref _trade, trade, null) is null;

    public void LeaveTrade(Trade trade) => Interlocked.CompareExchange(ref _trade, null, trade);

    public Trade? TakeTrade() => Interlocked.Exchange(ref _trade, null);

    public RoomActor(
        long SessionId,
        int VirtualId,
        ActorType Type,
        int AccountId,
        string Username,
        string Motto,
        string Figure,
        char Gender,
        int X,
        int Y,
        double Z)
    {
        this.SessionId = SessionId;
        this.VirtualId = VirtualId;
        this.Type = Type;
        this.AccountId = AccountId;
        this.Username = Username;
        this.Motto = Motto;
        this.Figure = Figure;
        this.Gender = Gender;
        this.X = X;
        this.Y = Y;
        this.Z = Z;
    }
}
