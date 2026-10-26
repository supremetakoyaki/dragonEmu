namespace DragonEmu.Domain;

public static class PetType
{
    public const int Dog = 0;
    public const int Cat = 1;
    public const int Croco = 2;
    public const int Terrier = 3;
    public const int Bear = 4;
    public const int Pig = 5;

    public static bool IsValid(int type) => type is >= Dog and <= Pig;
}

public sealed record PetRecord(
    int Id,
    int OwnerId,
    string OwnerName,
    int Type,
    int Breed,
    string Color,
    string Name,
    int? RoomId,
    int X,
    int Y,
    double Z,
    int Respect,
    int Level,
    int Experience,
    int Energy,
    int Nutrition,
    DateTime CreatedAt)
{
    public string Figure => PetFigure.Of(Type, Breed, Color);
}

public static class PetWebId
{
    public const int Offset = 1_000_000_000;

    public static int Of(int petId) => petId + Offset;

    public static int? PetIdOf(int webId) => webId > Offset ? webId - Offset : null;
}

public static class PetFigure
{
    public static string Of(int type, int breed, string color) => $"{type} {breed} {color}";
}

public sealed record PetStats(int Level, int Experience, int Energy, int Nutrition, int Respect);

public sealed record PetLevel(int Level, int ExperienceCap, int NutritionCap, int EnergyCap);

public sealed record PetCommand(int Id, string Name, int RequiredLevel, int Experience, int EnergyCost, int NutritionCost = 0);

public enum PetSpeechType
{
    RandomSpeech,
    TrainingPositive,
    TrainingNegative,
    Eating,
    Playful,
    Drinking,

    Sleeping,

    WelcomeHome,
}

public sealed record PetFood(int FurniDefId, int Portions, int FullState, int Nutrition, int Energy, int Experience)
{
    public (int Nutrition, int Energy, int Experience) Portion(int index) =>
        (Split(Nutrition, index), Split(Energy, index), Split(Experience, index));

    private int Split(int total, int index)
    {
        int portions = Math.Max(1, Portions);
        int each = total / portions;
        return index >= portions - 1 ? total - each * (portions - 1) : each;
    }
}

public sealed record PetPurchase(int Type, string Name, int Breed, string Color)
{
    public static PetPurchase? Parse(int type, string extraData)
    {
        string[] parts = extraData.Split('\n');
        if (parts.Length != 3
            || parts[1].Length is 0 or > 3
            || !parts[1].All(char.IsAsciiDigit)
            || parts[2].Length != 6
            || !parts[2].All(char.IsAsciiHexDigit))
        {
            return null;
        }

        return new PetPurchase(type, parts[0].Trim(), int.Parse(parts[1], System.Globalization.CultureInfo.InvariantCulture), parts[2].ToUpperInvariant());
    }
}

public static class PetOffer
{
    private const string CodePrefix = "a0 pet";

    public static int? TypeOf(CatalogOffer offer)
    {
        string code = offer.OfferCode;
        if (code.Length != CodePrefix.Length + 1
            || !code.StartsWith(CodePrefix, StringComparison.OrdinalIgnoreCase)
            || offer.Products is not [{ ProductType: CatalogProductType.Floor, FurnitureDefId: null }])
        {
            return null;
        }

        int type = code[^1] - '0';
        return PetType.IsValid(type) ? type : null;
    }
}

public enum PetNameCheck
{
    Ok = 0,
    TooLong = 1,
    ForbiddenCharacters = 2,
    Bobba = 3,
}

public enum PetPlacingError
{
    ForbiddenInHotel = 0,
    ForbiddenInRoom = 1,
    TooManyPets = 2,
    NoFreeTileInFront = 3,
    SelectedTileNotFree = 4,
}

public sealed record PetSettings(bool Enabled, int NameMaxLength, string NameAllowedChars, int RespectMinimumAccountDays)
{
    public const string DefaultNameAllowedChars = "^[A-Za-z0-9 À-ÖØ-öø-ɏḀ-ỿ_¡!#¿?|:.,;ƒ¬—ªº†»•‡‘¥µ-]+$"; // dingbats allowed! :-)

    public static readonly PetSettings Default = new(true, 20, DefaultNameAllowedChars, 3);
}
