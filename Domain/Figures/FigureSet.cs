namespace DragonEmu.Domain;

public sealed record FigureSet(int Id, string SettypeRef, char Gender, int ClubLevel, bool Colorable, bool Selectable)
{
    public FigureSet(int Id, string SettypeRef, char Gender, bool Club, bool Colorable, bool Selectable)
        : this(Id, SettypeRef, Gender, Club ? 1 : 0, Colorable, Selectable)
    {
    }

    public bool Club => ClubLevel > 0;
}
