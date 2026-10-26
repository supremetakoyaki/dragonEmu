using System.Globalization;
using DragonEmu.Domain;
using DragonEmu.Game.Rooms.Pathfinding;

namespace DragonEmu.Game.Rooms;

public sealed record PetUtterance(RoomActor Pet, string Text);

public sealed record PetExperienceGain(RoomActor Pet, int Amount, IReadOnlyList<int> LevelsReached);

public sealed record PetMeal(RoomActor Pet, int ItemId, int Nutrition, bool EatenUp);

public sealed record PetTickResult(
    List<PetUtterance> Utterances, List<PetExperienceGain> Experience, List<FurnitureStateChange> ItemStates, List<PetMeal> Meals)
{
    public static PetTickResult None => new([], [], [], []);
}

public sealed partial class Room
{
    private const int PoseTicks = 20;

    private const int WhimPoseTicks = 8;

    private const int JumpTicks = 3;
    private const int StayTicks = 120;
    private const int FollowTicks = 240;
    private const int SilentTicks = 240;
    private const int GestureTicks = 4;
    private const int PlayTicks = 16;

    public const int BiteTicks = 3;

    private const int MaxTravelTicks = 30;

    private const int RoamRadius = 5;

    private const int TiredPercent = 35;
    private const int ExhaustedPercent = 15;
    private const int TiredNestOdds = 12;
    private const int HungryPercent = 50;

    private const int StarvingPercent = 15;
    private const int StarvingCheckTicks = 4;

    private const int SnoreOdds = 16;

    private const int WelcomeHomeTicks = 4;

    private const int CommandFree = 0;
    private const int CommandSit = 1;
    private const int CommandDown = 2;
    private const int CommandHere = 3;
    private const int CommandBeg = 4;
    private const int CommandPlayDead = 5;
    private const int CommandStay = 6;
    private const int CommandFollow = 7;
    private const int CommandStand = 8;
    private const int CommandJump = 9;
    private const int CommandSpeak = 10;
    private const int CommandPlay = 11;
    private const int CommandSilent = 12;
    private const int CommandNest = 13;
    private const int CommandDrink = 14;
    private const int CommandFollowLeft = 15;
    private const int CommandFollowRight = 16;

    private const string PostureSit = "sit";
    private const string PostureLay = "lay";
    private const string PostureBeg = "beg";
    private const string PosturePlayDead = "ded";
    private const string PostureJump = "jmp";
    private const string PosturePlay = "pla";
    private const string PostureEat = "eat";
    private const string PostureSniff = "snf";
    private const string PostureScratch = "scr";

    private const string GestureHappy = "sml";
    private const string GestureJoy = "joy";
    private const string GestureSad = "sad";
    private const string GestureAngry = "agr";
    private const string GesturePuzzled = "puz";

    private int _petCount;

    private readonly object _petItemLock = new();
    private readonly Dictionary<int, int> _petItemStates = new();

    public bool HasPets => Volatile.Read(ref _petCount) > 0;

    public bool PetsAwake => HasPets && UserCount > 0;

    public int PetCount => Volatile.Read(ref _petCount);

    public static long PetSessionId(int petId) => -(1L << 32) - petId;

    public IReadOnlyList<RoomActor> Pets => _actors.Values.Where(actor => actor.Pet is not null).ToList();

    public RoomActor? GetPet(int petId) => _actors.TryGetValue(PetSessionId(petId), out RoomActor? actor) && actor.Pet is not null ? actor : null;

    public RoomActor? AddPet(PetRecord pet, PetDefinitions definitions, int x, int y, int rotation, bool welcomesHome = false)
    {
        var actor = new RoomActor(
            SessionId: PetSessionId(pet.Id), VirtualId: NextVirtualId(), Type: ActorType.Pet, AccountId: 0, Username: pet.Name,
            Motto: string.Empty, Figure: pet.Figure, Gender: 'M', X: x, Y: y, Z: SurfaceHeightAt(x, y))
        {
            RotBody = rotation,
            RotHead = rotation,
            Pet = new PetBrain(pet, definitions) { WelcomesHome = welcomesHome },
        };

        lock (actor.Lock)
        {
            ApplyTilePosture(actor);
        }

        if (!TryAddActor(actor))
        {
            return null;
        }

        Interlocked.Increment(ref _petCount);
        return actor;
    }

    public RoomActor? RemovePet(int petId)
    {
        if (!TryRemoveActor(PetSessionId(petId), out RoomActor? actor))
        {
            return null;
        }

        Interlocked.Decrement(ref _petCount);
        return actor;
    }

    public bool CanPlacePetAt(int x, int y)
    {
        if (!Grid.IsWalkable(x, y) || Grid.IsDoor(x, y) || !IsFurniturePassable(x, y, Grid.HeightAt(x, y), isGoal: true))
        {
            return false;
        }

        foreach (RoomActor actor in _actors.Values)
        {
            lock (actor.Lock)
            {
                if ((actor.X == x && actor.Y == y) || (actor.PendingStep is { } step && step.X == x && step.Y == y))
                {
                    return false;
                }
            }
        }

        return true;
    }

    public (int X, int Y)? FreeTileNear(int x, int y)
    {
        if (CanPlacePetAt(x, y))
        {
            return (x, y);
        }

        var seen = new HashSet<(int X, int Y)> { (x, y) };
        var queue = new Queue<(int X, int Y)>();
        queue.Enqueue((x, y));
        while (queue.TryDequeue(out (int X, int Y) tile))
        {
            for (int direction = 0; direction < 8; direction++)
            {
                (int dx, int dy) = Step(direction);
                (int X, int Y) next = (tile.X + dx, tile.Y + dy);
                if (next.X < 0 || next.Y < 0 || next.X >= Grid.Width || next.Y >= Grid.Height || !seen.Add(next))
                {
                    continue;
                }

                if (CanPlacePetAt(next.X, next.Y))
                {
                    return next;
                }

                queue.Enqueue(next);
            }
        }

        return null;
    }

    public static ((int X, int Y) Tile, int FacingBack) InFrontOf(RoomActor actor)
    {
        lock (actor.Lock)
        {
            (int dx, int dy) = Step(actor.RotBody);
            return ((actor.X + dx, actor.Y + dy), (actor.RotBody + 4) % 8);
        }
    }

    public static bool IsPetItem(FurnitureInteractionLogic logic) =>
        logic is FurnitureInteractionLogic.PetFood or FurnitureInteractionLogic.PetTreat or FurnitureInteractionLogic.PetWater or FurnitureInteractionLogic.PetToy;

    public void TrackPetItem(int itemId, string? extraData, int fullState)
    {
        int state = int.TryParse(extraData, NumberStyles.Integer, CultureInfo.InvariantCulture, out int parsed) ? parsed : fullState;
        lock (_petItemLock)
        {
            _petItemStates[itemId] = state;
        }
    }

    public bool RefillPetWater(int itemId, int fullState)
    {
        lock (_petItemLock)
        {
            if (!_petItemStates.ContainsKey(itemId))
            {
                return false;
            }

            _petItemStates[itemId] = fullState;
            return true;
        }
    }

    private void ForgetPetItem(int itemId)
    {
        lock (_petItemLock)
        {
            _petItemStates.Remove(itemId);
        }
    }

    private void TellPets(RoomActor speaker, string message)
    {
        if (PetActivityPaused)
        {
            return;
        }

        string said = message.Trim();
        foreach (RoomActor pet in _actors.Values)
        {
            if (pet.Pet is { } brain
                && brain.OwnerId == speaker.AccountId
                && said.Length > brain.Name.Length + 1
                && said.StartsWith(brain.Name, StringComparison.OrdinalIgnoreCase)
                && said[brain.Name.Length] == ' '
                && brain.Definitions.CommandByWords(said[(brain.Name.Length + 1)..]) is { } command)
            {
                brain.Hear(command.Id, speaker.SessionId);
            }
        }
    }

    public PetTickResult AdvancePets(List<RoomActor> changed, Random? random = null)
    {
        PetTickResult result = PetTickResult.None;
        if (!PetsAwake)
        {
            return result;
        }

        random ??= Random.Shared;
        foreach (RoomActor pet in _actors.Values)
        {
            if (pet.Pet is { } brain)
            {
                AdvancePet(pet, brain, changed, result, random);
                brain.Publish();
            }
        }

        return result;
    }

    private void AdvancePet(RoomActor pet, PetBrain brain, List<RoomActor> changed, PetTickResult result, Random random)
    {
        bool rested = brain.PassTime();
        brain.TakeRespect();
        Gain(pet, brain, brain.TakeExperience(), result);

        bool walking = IsMoving(pet);
        if (walking)
        {
            brain.MarkDirty();
        }

        if (brain.GestureTicksLeft > 0 && --brain.GestureTicksLeft == 0)
        {
            SetGesture(pet, brain, null, changed);
        }

        if (brain.WelcomesHome && brain.Ticks >= WelcomeHomeTicks)
        {
            brain.WelcomesHome = false;
            SetGesture(pet, brain, GestureJoy, changed);
            Say(pet, brain, PetSpeechType.WelcomeHome, result, random);
        }

        if (brain.TryTakeCommand(out (int CommandId, long SessionId) heard))
        {
            Obey(pet, brain, heard.CommandId, heard.SessionId, changed, result, random);
            return;
        }

        switch (brain.Mode)
        {
            case PetMode.Free:
                ActFree(pet, brain, walking, changed, result, random);
                break;

            case PetMode.Posing:
            case PetMode.Staying:
                if (--brain.ModeTicksLeft <= 0)
                {
                    BeFree(pet, brain, changed, result);
                }

                break;

            case PetMode.Following:
                Follow(pet, brain, walking, changed, result);
                break;

            case PetMode.GoingToOwner:
                GoToOwner(pet, brain, walking, changed, result);
                break;

            case PetMode.Sleeping:
                Sleep(pet, brain, walking, rested, changed, result, random);
                break;

            case PetMode.Eating:
            case PetMode.Drinking:
                Feed(pet, brain, walking, changed, result, random);
                break;

            case PetMode.Playing:
                Play(pet, brain, walking, changed, result, random);
                break;
        }
    }

    private void Obey(RoomActor pet, PetBrain brain, int commandId, long sessionId, List<RoomActor> changed, PetTickResult result, Random random)
    {
        if (brain.Definitions.CommandById(commandId) is not { } command)
        {
            return;
        }

        RoomActor? owner = _actors.TryGetValue(sessionId, out RoomActor? speaker) && speaker.Type == ActorType.Player ? speaker : null;
        if (owner is not null && TryFace(pet, PositionOf(owner)))
        {
            MarkChanged(changed, pet);
        }

        if (command.RequiredLevel > brain.Level)
        {
            SetGesture(pet, brain, GesturePuzzled, changed);
            Say(pet, brain, PetSpeechType.TrainingNegative, result, random);
            return;
        }

        if ((commandId == CommandNest && brain.IsRested) || (commandId == CommandDrink && brain.Energy >= brain.EnergyCap))
        {
            SetGesture(pet, brain, GesturePuzzled, changed);
            Say(pet, brain, PetSpeechType.TrainingNegative, result, random);
            return;
        }

        bool spent = brain.Energy < command.EnergyCost || brain.Nutrition < command.NutritionCost;
        if (spent || !brain.Obeys(random))
        {
            SetGesture(pet, brain, spent ? GestureSad : GestureAngry, changed);
            Say(pet, brain, PetSpeechType.TrainingNegative, result, random);
            return;
        }

        if (commandId is not (CommandSpeak or CommandSilent))
        {
            StopActivity(pet, brain, changed, result);
        }

        brain.Energy -= command.EnergyCost;
        brain.Nutrition -= command.NutritionCost;
        Gain(pet, brain, command.Experience, result);
        SetGesture(pet, brain, GestureHappy, changed);
        if (random.Next(2) == 0)
        {
            Say(pet, brain, PetSpeechType.TrainingPositive, result, random);
        }

        switch (commandId)
        {
            case CommandSit:
                Pose(pet, brain, PostureSit, PoseTicks, changed);
                break;
            case CommandDown:
                Pose(pet, brain, PostureLay, PoseTicks, changed);
                break;
            case CommandBeg:
                Pose(pet, brain, PostureBeg, PoseTicks, changed);
                break;
            case CommandPlayDead:
                Pose(pet, brain, PosturePlayDead, PoseTicks, changed);
                break;
            case CommandJump:
                Pose(pet, brain, PostureJump, JumpTicks, changed);
                break;

            case CommandHere when owner is not null:
                brain.Mode = PetMode.GoingToOwner;
                brain.TravelTicks = 0;
                WalkToOwner(pet, owner);
                break;

            case CommandStay:
                StopWalking(pet);
                brain.Mode = PetMode.Staying;
                brain.ModeTicksLeft = StayTicks;
                break;

            case CommandFollow or CommandFollowLeft or CommandFollowRight when owner is not null:
                brain.Mode = PetMode.Following;
                brain.ModeTicksLeft = FollowTicks;
                brain.FollowSide = commandId switch
                {
                    CommandFollowLeft => PetFollowSide.Left,
                    CommandFollowRight => PetFollowSide.Right,
                    _ => PetFollowSide.Behind,
                };
                break;

            case CommandSpeak:
                brain.SilentUntilTick = 0;
                Say(pet, brain, PetSpeechType.RandomSpeech, result, random);
                break;

            case CommandPlay:
                if (!TryGoPlay(pet, brain))
                {
                    Pose(pet, brain, PosturePlay, WhimPoseTicks, changed);
                }

                break;

            case CommandSilent:
                brain.SilentUntilTick = brain.Ticks + SilentTicks;
                break;

            case CommandNest:
                GoToSleep(pet, brain, changed);
                break;

            case CommandDrink:
                if (!TryGoFeed(pet, brain, PetMode.Drinking, FurnitureInteractionLogic.PetWater))
                {
                    SetGesture(pet, brain, GesturePuzzled, changed);
                }

                break;

            default:
                BeFree(pet, brain, changed, result);
                break;
        }
    }

    private void ActFree(RoomActor pet, PetBrain brain, bool walking, List<RoomActor> changed, PetTickResult result, Random random)
    {
        if (walking)
        {
            return;
        }

        if (IsStarving(brain) && brain.Ticks % StarvingCheckTicks == 0 && TryGoFeed(pet, brain, PetMode.Eating, FurnitureInteractionLogic.PetFood))
        {
            return;
        }

        if (brain.Ticks < brain.NextIdleTick)
        {
            return;
        }

        brain.NextIdleTick = brain.Ticks + random.Next(6, 17);

        if (brain.Energy * 100 < brain.EnergyCap * ExhaustedPercent)
        {
            GoToSleep(pet, brain, changed);
            return;
        }

        if (brain.Energy * 100 < brain.EnergyCap * TiredPercent)
        {
            if (random.Next(TiredNestOdds) == 0)
            {
                GoToSleep(pet, brain, changed);
                return;
            }

            if (TryGoFeed(pet, brain, PetMode.Drinking, FurnitureInteractionLogic.PetWater)
                || TryGoFeed(pet, brain, PetMode.Eating, FurnitureInteractionLogic.PetTreat))
            {
                return;
            }
        }

        if (brain.Nutrition * 100 < brain.NutritionCap * HungryPercent && TryGoFeed(pet, brain, PetMode.Eating, FurnitureInteractionLogic.PetFood))
        {
            return;
        }

        int roll = random.Next(100);
        if (roll < 35)
        {
            Roam(pet, random);
        }
        else if (roll < 47)
        {
            Pose(pet, brain, PostureSit, WhimPoseTicks, changed);
        }
        else if (roll < 54)
        {
            Pose(pet, brain, PostureLay, WhimPoseTicks, changed);
        }
        else if (roll < 62)
        {
            Pose(pet, brain, PostureSniff, WhimPoseTicks / 2, changed);
        }
        else if (roll < 68)
        {
            Pose(pet, brain, PostureScratch, WhimPoseTicks / 2, changed);
        }
        else if (roll < 74 && brain.Energy * 2 > brain.EnergyCap && TryGoPlay(pet, brain))
        {
            Say(pet, brain, PetSpeechType.Playful, result, random);
        }
        else if (roll < 90)
        {
            Say(pet, brain, PetSpeechType.RandomSpeech, result, random);
        }
        else
        {
            SetGesture(pet, brain, random.Next(2) == 0 ? GestureHappy : GestureJoy, changed);
        }
    }

    private void Roam(RoomActor pet, Random random)
    {
        const int Tries = 6;

        (int x, int y) = PositionOf(pet);
        for (int i = 0; i < Tries; i++)
        {
            int goalX = x + random.Next(-RoamRadius, RoamRadius + 1);
            int goalY = y + random.Next(-RoamRadius, RoamRadius + 1);
            if ((goalX, goalY) != (x, y) && Grid.IsWalkable(goalX, goalY) && !Grid.IsDoor(goalX, goalY) && TryWalkPet(pet, goalX, goalY))
            {
                return;
            }
        }
    }

    private void Follow(RoomActor pet, PetBrain brain, bool walking, List<RoomActor> changed, PetTickResult result)
    {
        if (--brain.ModeTicksLeft <= 0 || OwnerOf(brain) is not { } owner)
        {
            BeFree(pet, brain, changed, result);
            return;
        }

        (int ownerX, int ownerY, int facing) = PlaceOf(owner);
        int side = brain.FollowSide switch
        {
            PetFollowSide.Left => (facing + 6) % 8,
            PetFollowSide.Right => (facing + 2) % 8,
            _ => (facing + 4) % 8,
        };

        (int dx, int dy) = Step(side);
        (int goalX, int goalY) = (ownerX + dx, ownerY + dy);
        (int x, int y) = PositionOf(pet);
        if ((x, y) == (goalX, goalY))
        {
            if (!walking && TryTurn(pet, facing))
            {
                MarkChanged(changed, pet);
            }

            return;
        }

        if (walking && GoalOf(pet) == (goalX, goalY))
        {
            return;
        }

        if (!TryWalkPet(pet, goalX, goalY) && Math.Max(Math.Abs(x - ownerX), Math.Abs(y - ownerY)) > 1 && !walking)
        {
            WalkToOwner(pet, owner);
        }
    }

    private void GoToOwner(RoomActor pet, PetBrain brain, bool walking, List<RoomActor> changed, PetTickResult result)
    {
        if (OwnerOf(brain) is not { } owner)
        {
            BeFree(pet, brain, changed, result);
            return;
        }

        if (walking)
        {
            if (++brain.TravelTicks > MaxTravelTicks)
            {
                BeFree(pet, brain, changed, result);
            }

            return;
        }

        (int ownerX, int ownerY) = PositionOf(owner);
        if (TryFace(pet, (ownerX, ownerY)))
        {
            MarkChanged(changed, pet);
        }

        Pose(pet, brain, PostureSit, PoseTicks, changed);
    }

    private void GoToSleep(RoomActor pet, PetBrain brain, List<RoomActor> changed)
    {
        StopActivityQuietly(pet, brain);
        brain.Mode = PetMode.Sleeping;
        brain.TravelTicks = 0;
        brain.TargetItemId = 0;
        brain.InNest = false;

        (int x, int y) = PositionOf(pet);
        RoomFloorItem? nest = NearestItem(x, y, item => item.InteractionLogic == FurnitureInteractionLogic.PetNest);
        if (nest is not null && nest.OccupiedTiles().Contains((x, y)))
        {
            brain.TargetItemId = nest.Id;
            brain.InNest = true;
        }
        else if (nest is not null && TryWalkPet(pet, nest.X, nest.Y))
        {
            brain.TargetItemId = nest.Id;
            return;
        }

        SetPosture(pet, PostureLay, changed);
    }

    private void Sleep(RoomActor pet, PetBrain brain, bool walking, bool rested, List<RoomActor> changed, PetTickResult result, Random random)
    {
        if (rested)
        {
            brain.InNest = false;
            BeFree(pet, brain, changed, result);
            SetGesture(pet, brain, GestureHappy, changed);
            return;
        }

        if (!walking && IsStarving(brain) && brain.Ticks % StarvingCheckTicks == 0)
        {
            brain.InNest = false;
            if (TryGoFeed(pet, brain, PetMode.Eating, FurnitureInteractionLogic.PetFood))
            {
                return;
            }

            brain.InNest = IsOnItem(pet, brain.TargetItemId);
        }

        if (walking)
        {
            if (++brain.TravelTicks > MaxTravelTicks)
            {
                StopWalking(pet);
            }

            return;
        }

        brain.InNest = IsOnItem(pet, brain.TargetItemId);
        SetPosture(pet, PostureLay, changed);
        if (brain.InNest && random.Next(SnoreOdds) == 0)
        {
            Say(pet, brain, PetSpeechType.Sleeping, result, random);
        }
    }

    private static bool IsStarving(PetBrain brain) => brain.Nutrition * 100 <= brain.NutritionCap * StarvingPercent;

    private bool IsOnItem(RoomActor pet, int itemId) =>
        itemId != 0 && GetFloorItem(itemId) is { } item && item.OccupiedTiles().Contains(PositionOf(pet));

    private bool TryGoFeed(RoomActor pet, PetBrain brain, PetMode mode, FurnitureInteractionLogic kind)
    {
        if (PetActivityPaused)
        {
            return false;
        }

        if (kind != FurnitureInteractionLogic.PetWater && brain.OwnerId != Data.OwnerId && !Data.AllowPetsEat)
        {
            return false;
        }

        (int x, int y) = PositionOf(pet);
        RoomFloorItem? item = NearestItem(x, y, candidate => candidate.InteractionLogic == kind && HasPortionLeft(candidate, brain.Definitions));
        if (item is null || !TryGoNextTo(pet, item))
        {
            return false;
        }

        StopActivityQuietly(pet, brain);
        brain.Mode = mode;
        brain.TargetItemId = item.Id;
        brain.TravelTicks = 0;
        brain.BiteTicksLeft = 1;
        brain.Bites = 0;
        return true;
    }

    private void Feed(RoomActor pet, PetBrain brain, bool walking, List<RoomActor> changed, PetTickResult result, Random random)
    {
        bool drinking = brain.Mode == PetMode.Drinking;
        RoomFloorItem? item = GetFloorItem(brain.TargetItemId);
        PetFood? food = item is null ? null : brain.Definitions.FoodFor(item.Classname);
        if (item is null || food is null)
        {
            BeFree(pet, brain, changed, result);
            return;
        }

        if (walking)
        {
            if (++brain.TravelTicks > MaxTravelTicks)
            {
                StopWalking(pet);
                BeFree(pet, brain, changed, result);
            }

            return;
        }

        (int x, int y) = PositionOf(pet);
        if (!item.IsNextTo(x, y))
        {
            BeFree(pet, brain, changed, result);
            return;
        }

        if (TryFace(pet, (item.X, item.Y)))
        {
            MarkChanged(changed, pet);
        }

        SetPosture(pet, PostureEat, changed);
        if (--brain.BiteTicksLeft > 0)
        {
            return;
        }

        brain.BiteTicksLeft = BiteTicks;
        if (TakePortion(item, food) is not { } taken)
        {
            BeFree(pet, brain, changed, result);
            return;
        }

        (int portion, int energy, int experience) = food.Portion(taken.Index);
        int nutrition = Math.Clamp(brain.NutritionCap - brain.Nutrition, 0, portion);
        brain.Nutrition += nutrition;
        brain.Energy = Math.Min(brain.EnergyCap, brain.Energy + energy);
        brain.Bites++;
        Gain(pet, brain, experience, result);

        if (taken.State is { } state)
        {
            result.ItemStates.Add(new FurnitureStateChange(item.Id, state.ToString(CultureInfo.InvariantCulture)));
        }

        if (!drinking)
        {
            result.Meals.Add(new PetMeal(pet, item.Id, nutrition, EatenUp: taken.State is null));
        }

        if (random.Next(3) == 0)
        {
            Say(pet, brain, drinking ? PetSpeechType.Drinking : PetSpeechType.Eating, result, random);
        }

        bool hadEnough = drinking || item.InteractionLogic == FurnitureInteractionLogic.PetTreat
            ? brain.Energy >= brain.EnergyCap
            : brain.Nutrition >= brain.NutritionCap;
        if (taken.State is null || hadEnough || (drinking && taken.State == 0))
        {
            BeFree(pet, brain, changed, result);
        }
    }

    private bool HasPortionLeft(RoomFloorItem item, PetDefinitions definitions)
    {
        lock (_petItemLock)
        {
            if (!_petItemStates.TryGetValue(item.Id, out int state))
            {
                return false;
            }

            if (definitions.FoodFor(item.Classname) is not { } food)
            {
                return item.InteractionLogic == FurnitureInteractionLogic.PetToy;
            }

            return item.InteractionLogic == FurnitureInteractionLogic.PetWater
                ? state > 0
                : state < food.FullState + food.Portions;
        }
    }

    private (int Index, int? State)? TakePortion(RoomFloorItem item, PetFood food)
    {
        lock (_petItemLock)
        {
            if (!_petItemStates.TryGetValue(item.Id, out int state))
            {
                return null;
            }

            if (item.InteractionLogic == FurnitureInteractionLogic.PetWater)
            {
                if (state <= 0)
                {
                    return null;
                }

                _petItemStates[item.Id] = state - 1;
                return (Math.Clamp(food.FullState - state, 0, food.Portions - 1), state - 1);
            }

            int index = state - food.FullState;
            if (index < 0 || index >= food.Portions)
            {
                return null;
            }

            if (index == food.Portions - 1)
            {
                _petItemStates.Remove(item.Id);
                return (index, null);
            }

            _petItemStates[item.Id] = state + 1;
            return (index, state + 1);
        }
    }

    private bool TryGoPlay(RoomActor pet, PetBrain brain)
    {
        (int x, int y) = PositionOf(pet);
        RoomFloorItem? toy = NearestItem(x, y, item => item.InteractionLogic == FurnitureInteractionLogic.PetToy && HasPortionLeft(item, brain.Definitions));
        if (toy is null || !TryGoNextTo(pet, toy))
        {
            return false;
        }

        StopActivityQuietly(pet, brain);
        brain.Mode = PetMode.Playing;
        brain.TargetItemId = toy.Id;
        brain.TravelTicks = 0;
        brain.ModeTicksLeft = PlayTicks;
        return true;
    }

    private void Play(RoomActor pet, PetBrain brain, bool walking, List<RoomActor> changed, PetTickResult result, Random random)
    {
        RoomFloorItem? toy = GetFloorItem(brain.TargetItemId);
        if (toy is null)
        {
            BeFree(pet, brain, changed, result);
            return;
        }

        if (walking)
        {
            if (++brain.TravelTicks > MaxTravelTicks)
            {
                StopWalking(pet);
                BeFree(pet, brain, changed, result);
            }

            return;
        }

        (int x, int y) = PositionOf(pet);
        if (!toy.IsNextTo(x, y) || --brain.ModeTicksLeft <= 0)
        {
            BeFree(pet, brain, changed, result);
            return;
        }

        if (TryFace(pet, (toy.X, toy.Y)))
        {
            MarkChanged(changed, pet);
        }

        SetPosture(pet, PosturePlay, changed);
        if (SetToyRolling(toy.Id, rolling: true))
        {
            result.ItemStates.Add(new FurnitureStateChange(toy.Id, "1"));
        }

        if (brain.ModeTicksLeft % 4 == 0)
        {
            brain.Energy = Math.Max(0, brain.Energy - 1);
            if (random.Next(3) == 0)
            {
                Say(pet, brain, PetSpeechType.Playful, result, random);
            }
        }
    }

    private bool SetToyRolling(int toyId, bool rolling)
    {
        int state = rolling ? 1 : 0;
        lock (_petItemLock)
        {
            if (!_petItemStates.TryGetValue(toyId, out int current) || current == state)
            {
                return false;
            }

            _petItemStates[toyId] = state;
            return true;
        }
    }

    private void Pose(RoomActor pet, PetBrain brain, string posture, int ticks, List<RoomActor> changed)
    {
        StopWalking(pet);
        brain.Mode = PetMode.Posing;
        brain.ModeTicksLeft = ticks;
        SetPosture(pet, posture, changed);
    }

    private void BeFree(RoomActor pet, PetBrain brain, List<RoomActor> changed, PetTickResult result)
    {
        StopActivity(pet, brain, changed, result);
        brain.Mode = PetMode.Free;
        brain.ModeTicksLeft = 0;
        brain.TargetItemId = 0;
        SetPosture(pet, null, changed);
    }

    private void StopActivity(RoomActor pet, PetBrain brain, List<RoomActor> changed, PetTickResult result)
    {
        if (brain.Mode == PetMode.Playing && brain.TargetItemId != 0 && SetToyRolling(brain.TargetItemId, rolling: false))
        {
            result.ItemStates.Add(new FurnitureStateChange(brain.TargetItemId, "0"));
        }

        if (brain.Mode != PetMode.Free)
        {
            SetPosture(pet, null, changed);
        }
    }

    private static void StopActivityQuietly(RoomActor pet, PetBrain brain)
    {
        brain.ModeTicksLeft = 0;
        lock (pet.Lock)
        {
            pet.PetPosture = null;
        }
    }

    private static void SetPosture(RoomActor pet, string? posture, List<RoomActor> changed)
    {
        lock (pet.Lock)
        {
            if (pet.PetPosture == posture)
            {
                return;
            }

            pet.PetPosture = posture;
        }

        MarkChanged(changed, pet);
    }

    private static void SetGesture(RoomActor pet, PetBrain brain, string? gesture, List<RoomActor> changed)
    {
        brain.GestureTicksLeft = gesture is null ? 0 : GestureTicks;
        lock (pet.Lock)
        {
            if (pet.PetGesture == gesture)
            {
                return;
            }

            pet.PetGesture = gesture;
        }

        MarkChanged(changed, pet);
    }

    private static void Say(RoomActor pet, PetBrain brain, PetSpeechType kind, PetTickResult result, Random random)
    {
        if (brain.Ticks >= brain.SilentUntilTick && brain.Definitions.Speech(brain.Type, kind, random) is { } text)
        {
            result.Utterances.Add(new PetUtterance(pet, text));
        }
    }

    private static void Gain(RoomActor pet, PetBrain brain, int amount, PetTickResult result)
    {
        if (amount > 0)
        {
            result.Experience.Add(new PetExperienceGain(pet, amount, brain.Gain(amount)));
        }
    }

    private bool TryGoNextTo(RoomActor pet, RoomFloorItem item)
    {
        const int Tries = 4;

        (int x, int y) = PositionOf(pet);
        if (item.IsNextTo(x, y))
        {
            return true;
        }

        IEnumerable<(int X, int Y)> beside = item.OccupiedTiles()
            .SelectMany(tile => Enumerable.Range(0, 8).Select(direction => Step(direction)).Select(step => (X: tile.X + step.DX, Y: tile.Y + step.DY)))
            .Distinct()
            .Where(tile => Grid.IsWalkable(tile.X, tile.Y) && !Grid.IsDoor(tile.X, tile.Y))
            .OrderBy(tile => Math.Max(Math.Abs(tile.X - x), Math.Abs(tile.Y - y)))
            .Take(Tries)
            .Append((item.X, item.Y));

        foreach ((int goalX, int goalY) in beside)
        {
            if (TryWalkPet(pet, goalX, goalY))
            {
                return true;
            }
        }

        return false;
    }

    private void WalkToOwner(RoomActor pet, RoomActor owner)
    {
        ((int x, int y), _) = InFrontOf(owner);
        if (!TryWalkPet(pet, x, y))
        {
            (int ownerX, int ownerY) = PositionOf(owner);
            for (int direction = 0; direction < 8; direction++)
            {
                (int dx, int dy) = Step(direction);
                if (TryWalkPet(pet, ownerX + dx, ownerY + dy))
                {
                    return;
                }
            }
        }
    }

    private bool TryWalkPet(RoomActor pet, int x, int y)
    {
        if (Grid.IsDoor(x, y) || !TryStartWalk(pet, x, y))
        {
            return false;
        }

        lock (pet.Lock)
        {
            pet.PetPosture = null;
        }

        return true;
    }

    private static void StopWalking(RoomActor pet)
    {
        lock (pet.Lock)
        {
            pet.IsWalking = false;
            pet.Path = null;
            pet.Goal = null;
        }
    }

    private static bool IsMoving(RoomActor actor)
    {
        lock (actor.Lock)
        {
            return actor.IsWalking || actor.PendingStep is not null;
        }
    }

    private static (int X, int Y)? GoalOf(RoomActor actor)
    {
        lock (actor.Lock)
        {
            return actor.Goal;
        }
    }

    private static (int X, int Y, int Facing) PlaceOf(RoomActor actor)
    {
        lock (actor.Lock)
        {
            return (actor.X, actor.Y, actor.RotBody);
        }
    }

    private static bool TryFace(RoomActor pet, (int X, int Y) tile)
    {
        lock (pet.Lock)
        {
            if (pet.IsWalking || pet.PendingStep is not null || (pet.X == tile.X && pet.Y == tile.Y))
            {
                return false;
            }

            return TurnUnlocked(pet, RotationHelper.Calculate(pet.X, pet.Y, tile.X, tile.Y));
        }
    }

    private static bool TryTurn(RoomActor pet, int rotation)
    {
        lock (pet.Lock)
        {
            return !pet.IsWalking && pet.PendingStep is null && TurnUnlocked(pet, rotation);
        }
    }

    private static bool TurnUnlocked(RoomActor pet, int rotation)
    {
        if (pet.RotBody == rotation && pet.RotHead == rotation)
        {
            return false;
        }

        pet.RotBody = rotation;
        pet.RotHead = rotation;
        return true;
    }

    private static void MarkChanged(List<RoomActor> changed, RoomActor pet)
    {
        if (!changed.Contains(pet))
        {
            changed.Add(pet);
        }
    }

    private RoomActor? OwnerOf(PetBrain brain) =>
        _actors.Values.FirstOrDefault(actor => actor.Type == ActorType.Player && actor.AccountId == brain.OwnerId);

    private RoomFloorItem? NearestItem(int x, int y, Func<RoomFloorItem, bool> predicate) =>
        FloorItemsWhere(predicate).MinBy(item => Math.Max(Math.Abs(item.X - x), Math.Abs(item.Y - y)));

    private static (int DX, int DY) Step(int direction) => ((direction % 8 + 8) % 8) switch
    {
        0 => (0, -1),
        1 => (1, -1),
        2 => (1, 0),
        3 => (1, 1),
        4 => (0, 1),
        5 => (-1, 1),
        6 => (-1, 0),
        _ => (-1, -1),
    };
}
