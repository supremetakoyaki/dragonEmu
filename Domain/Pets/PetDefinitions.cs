namespace DragonEmu.Domain;

public sealed class PetDefinitions
{
    private static readonly PetLevel FallbackLevel = new(1, 100, 100, 100);

    private readonly Dictionary<int, PetLevel> _levels;
    private readonly Dictionary<int, PetCommand> _commands;
    private readonly Dictionary<string, PetCommand> _commandsByWord;
    private readonly Dictionary<(int Type, PetSpeechType Kind), string[]> _speech;
    private readonly HashSet<(int Type, int Breed)> _breeds;
    private readonly HashSet<(int Type, string Color)> _colors;
    private readonly Dictionary<string, PetFood> _foodByClassname;

    public static PetDefinitions Empty { get; } = new([], [], [], [], [], new Dictionary<string, PetFood>());

    public PetDefinitions(
        IEnumerable<PetLevel> levels,
        IEnumerable<PetCommand> commands,
        IEnumerable<(int Type, PetSpeechType Kind, string Text)> speech,
        IEnumerable<(int Type, int Breed)> breeds,
        IEnumerable<(int Type, string Color)> colors,
        IReadOnlyDictionary<string, PetFood> foodByClassname)
    {
        _levels = levels.ToDictionary(level => level.Level);
        MaxLevel = _levels.Count == 0 ? 1 : _levels.Keys.Max();
        _commands = commands.ToDictionary(command => command.Id);
        _commandsByWord = _commands.Values.ToDictionary(command => Normalize(command.Name), StringComparer.Ordinal);
        _speech = speech.GroupBy(line => (line.Type, line.Kind)).ToDictionary(group => group.Key, group => group.Select(line => line.Text).ToArray());
        _breeds = breeds.ToHashSet();
        _colors = colors.Select(color => (color.Type, color.Color.ToUpperInvariant())).ToHashSet();
        _foodByClassname = new Dictionary<string, PetFood>(foodByClassname, StringComparer.OrdinalIgnoreCase);
    }

    public int MaxLevel { get; }

    public IReadOnlyCollection<PetCommand> Commands => _commands.Values;

    public PetLevel LevelOf(int level)
    {
        if (_levels.TryGetValue(level, out PetLevel? found))
        {
            return found;
        }

        return _levels.Count == 0 ? FallbackLevel : _levels[_levels.Keys.OrderBy(known => Math.Abs(known - level)).First()];
    }

    public PetCommand? CommandById(int id) => _commands.GetValueOrDefault(id);

    public PetCommand? CommandByWords(string words) => _commandsByWord.GetValueOrDefault(Normalize(words));

    public IReadOnlyList<int> CommandsKnownAt(int level) =>
        _commands.Values.Where(command => command.RequiredLevel <= level).Select(command => command.Id).Order().ToList();

    public string? Speech(int type, PetSpeechType kind, Random random) =>
        _speech.TryGetValue((type, kind), out string[]? lines) && lines.Length > 0 ? lines[random.Next(lines.Length)] : null;

    public bool IsValidBreed(int type, int breed) => _breeds.Contains((type, breed));

    public bool IsValidColor(int type, string color) => _colors.Contains((type, color.ToUpperInvariant()));

    public PetFood? FoodFor(string classname) => _foodByClassname.GetValueOrDefault(classname);

    private static string Normalize(string words) =>
        string.Join(' ', words.Split(' ', StringSplitOptions.RemoveEmptyEntries)).ToLowerInvariant();
}
