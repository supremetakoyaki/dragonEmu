using DragonEmu.Game.Rooms.Pathfinding;

namespace DragonEmu.Game.Rooms;

public sealed record BotUtterance(RoomActor Bot, BotStepType Kind, string Text, long? ToSessionId);

public sealed record BotTickResult(
    List<BotUtterance> Utterances, List<(RoomActor Holder, int ItemId)> HandItems, List<RoomActor> Waves, List<(RoomActor Bot, int DanceId)> Dances)
{
    public static BotTickResult None => new([], [], [], []);
}

public sealed partial class Room
{
    private const int MaxDanceId = 4;

    private int _botCount;

    public bool HasBots => Volatile.Read(ref _botCount) > 0;

    public bool BotsAwake => HasBots && UserCount > 0;

    public static long BotSessionId(int botId) => -(long)botId;

    public RoomActor? GuideBot => _actors.Values.FirstOrDefault(actor => actor.Bot?.Definition.IsGuide == true);

    public List<RoomActor> AddBots(IEnumerable<BotDefinition> bots)
    {
        var added = new List<RoomActor>();
        foreach (BotDefinition bot in bots)
        {
            var actor = new RoomActor(
                SessionId: BotSessionId(bot.Id), VirtualId: NextVirtualId(), Type: ActorType.Bot, AccountId: 0, Username: bot.Name,
                Motto: bot.Motto, Figure: bot.Figure, Gender: bot.Gender, X: bot.X, Y: bot.Y, Z: bot.Z)
            {
                RotBody = bot.Rotation,
                RotHead = bot.Rotation,
                Bot = new BotBrain(bot),
            };

            if (TryAddActor(actor))
            {
                Interlocked.Increment(ref _botCount);
                added.Add(actor);
            }
        }

        return added;
    }

    public RoomActor? AddGuideBot(BotDefinition guide)
    {
        if (GuideBot is not null)
        {
            return null;
        }

        BotDefinition atTheDoor = guide with { X = Data.DoorX, Y = Data.DoorY, Z = Data.DoorZ, Rotation = Data.DoorDir, IsGuide = true };
        return AddBots([atTheDoor]) is [var actor] ? actor : null;
    }

    public RoomActor? RemoveGuideBot(int virtualId)
    {
        if (!TryGetActorByVirtualId(virtualId, out RoomActor? actor) || actor!.Bot?.Definition.IsGuide != true
            || !TryRemoveActor(actor.SessionId, out _))
        {
            return null;
        }

        Interlocked.Decrement(ref _botCount);
        return actor;
    }

    public List<RoomActor> RemoveBots()
    {
        var removed = new List<RoomActor>();
        foreach (RoomActor actor in _actors.Values.Where(actor => actor.Bot is { } brain && !brain.Definition.IsGuide))
        {
            if (TryRemoveActor(actor.SessionId, out _))
            {
                Interlocked.Decrement(ref _botCount);
                removed.Add(actor);
            }
        }

        return removed;
    }

    public void HearChat(RoomActor speaker, string message, bool shout)
    {
        if (HasPets && speaker.Type == ActorType.Player)
        {
            TellPets(speaker, message);
        }

        if (!HasBots)
        {
            return;
        }

        (int speakerX, int speakerY) = PositionOf(speaker);
        foreach (RoomActor bot in _actors.Values)
        {
            if (bot.Bot is { } brain)
            {
                (int botX, int botY) = PositionOf(bot);
                brain.Hear(speaker, message, shout, Math.Max(Math.Abs(botX - speakerX), Math.Abs(botY - speakerY)));
            }
        }
    }

    public BotTickResult AdvanceBots(List<RoomActor> changed)
    {
        BotTickResult result = BotTickResult.None;
        if (!BotsAwake)
        {
            return result;
        }

        foreach (RoomActor bot in _actors.Values)
        {
            if (bot.Bot is not { } brain)
            {
                continue;
            }

            bool walking;
            lock (bot.Lock)
            {
                walking = bot.IsWalking || bot.PendingStep is not null;
                if (bot.BotDanceTicksLeft > 0 && --bot.BotDanceTicksLeft == 0)
                {
                    bot.DanceId = 0;
                    result.Dances.Add((bot, 0));
                }
            }

            if (brain.Tick(walking) is not { } go)
            {
                continue;
            }

            RoomActor? target = go.Target is { } wanted && _actors.TryGetValue(wanted.SessionId, out RoomActor? present) && ReferenceEquals(present, wanted)
                ? wanted
                : null;

            if (go.FaceTarget && target is not null && TurnTowards(bot, target) && !changed.Contains(bot))
            {
                changed.Add(bot);
            }

            foreach (BotStep step in go.Steps)
            {
                Perform(bot, brain.Definition, step, target, result, changed);
            }
        }

        return result;
    }

    private void Perform(RoomActor bot, BotDefinition definition, BotStep step, RoomActor? target, BotTickResult result, List<RoomActor> changed)
    {
        switch (step.Type)
        {
            case BotStepType.Say:
            case BotStepType.Shout:
                result.Utterances.Add(new BotUtterance(bot, step.Type, Fill(step.Text, definition, target), ToSessionId: null));
                break;

            case BotStepType.Whisper when target is not null:
                result.Utterances.Add(new BotUtterance(bot, step.Type, Fill(step.Text, definition, target), target.SessionId));
                break;

            case BotStepType.Walk:
                if (step.Tile is { } tile)
                {
                    TryStartWalk(bot, tile.X, tile.Y);
                }
                else if (step.Text == "center")
                {
                    WalkToCentre(bot, definition);
                }
                else
                {
                    WalkSomewhere(bot, definition, step.Value);
                }

                break;

            case BotStepType.HandItem:
                RoomActor holder = target ?? bot;
                GiveHandItem(holder, step.Value);
                result.HandItems.Add((holder, step.Value));
                break;

            case BotStepType.Wave:
                result.Waves.Add(bot);
                break;

            case BotStepType.Dance:
                int danceId = Random.Shared.Next(1, MaxDanceId + 1);
                lock (bot.Lock)
                {
                    bot.DanceId = danceId;
                    bot.BotDanceTicksLeft = step.Value;
                }

                result.Dances.Add((bot, danceId));
                break;

            case BotStepType.LookAt:
                RoomActor? lookAt = step.Text == "owner"
                    ? _actors.Values.FirstOrDefault(actor => actor.Type == ActorType.Player && actor.AccountId == Data.OwnerId)
                    : target;
                if (lookAt is not null && TurnTowards(bot, lookAt) && !changed.Contains(bot))
                {
                    changed.Add(bot);
                }

                break;
        }
    }

    private static string Fill(string text, BotDefinition definition, RoomActor? target) =>
        text.Replace("{username}", target?.Username ?? string.Empty, StringComparison.Ordinal)
            .Replace("{botname}", definition.Name, StringComparison.Ordinal);

    private void WalkSomewhere(RoomActor bot, BotDefinition definition, int maxDistance)
    {
        (int botX, int botY) = PositionOf(bot);
        TryWalkToOneOf(bot, (definition.WalkableArea ?? AllWalkableTiles())
            .Where(tile => tile != (botX, botY) && Grid.IsWalkable(tile.X, tile.Y)
                && (maxDistance == 0 || Math.Max(Math.Abs(tile.X - botX), Math.Abs(tile.Y - botY)) <= maxDistance))
            .ToList());
    }

    private void WalkToCentre(RoomActor bot, BotDefinition definition)
    {
        const int Slack = 1;

        (int botX, int botY) = PositionOf(bot);
        double centreX = (Grid.Width - 1) / 2.0, centreY = (Grid.Height - 1) / 2.0;
        var tiles = (definition.WalkableArea ?? AllWalkableTiles())
            .Where(tile => tile != (botX, botY) && Grid.IsWalkable(tile.X, tile.Y))
            .Select(tile => (Tile: tile, Distance: Math.Max(Math.Abs(tile.X - centreX), Math.Abs(tile.Y - centreY))))
            .ToList();
        if (tiles.Count == 0)
        {
            return;
        }

        double nearest = tiles.Min(t => t.Distance);
        TryWalkToOneOf(bot, tiles.Where(t => t.Distance <= nearest + Slack).Select(t => t.Tile).ToList());
    }

    private void TryWalkToOneOf(RoomActor bot, List<(int X, int Y)> candidates)
    {
        const int Tries = 8;

        for (int i = 0; i < Tries && candidates.Count > 0; i++)
        {
            int pick = Random.Shared.Next(candidates.Count);
            (int x, int y) = candidates[pick];
            if (TryStartWalk(bot, x, y))
            {
                return;
            }

            candidates.RemoveAt(pick);
        }
    }

    private IEnumerable<(int X, int Y)> AllWalkableTiles()
    {
        for (int x = 0; x < Grid.Width; x++)
        {
            for (int y = 0; y < Grid.Height; y++)
            {
                if (Grid.IsWalkable(x, y))
                {
                    yield return (x, y);
                }
            }
        }
    }

    private static bool TurnTowards(RoomActor bot, RoomActor target)
    {
        (int targetX, int targetY) = PositionOf(target);
        lock (bot.Lock)
        {
            if (bot.IsWalking || bot.PendingStep is not null || (bot.X == targetX && bot.Y == targetY))
            {
                return false;
            }

            int rotation = RotationHelper.Calculate(bot.X, bot.Y, targetX, targetY);
            if (bot.RotBody == rotation && bot.RotHead == rotation)
            {
                return false;
            }

            bot.RotBody = rotation;
            bot.RotHead = rotation;
            return true;
        }
    }

    private static (int X, int Y) PositionOf(RoomActor actor)
    {
        lock (actor.Lock)
        {
            return (actor.X, actor.Y);
        }
    }
}
